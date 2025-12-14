using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuantumIdleModels.Entities;
using QuantumIdleWEB.Data;
using QuantumIdleWEB.Services;
using System.Security.Claims;

namespace QuantumIdleWEB.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TelegramController : ControllerBase
    {
        private readonly TelegramClientService _telegramService;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<TelegramController> _logger;

        public TelegramController(
            TelegramClientService telegramService,
            ApplicationDbContext dbContext,
            ILogger<TelegramController> logger)
        {
            _telegramService = telegramService;
            _dbContext = dbContext;
            _logger = logger;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        /// <summary>
        /// 初始化 Telegram 客户端
        /// </summary>
        [HttpPost("initialize")]
        public async Task<IActionResult> Initialize([FromBody] InitializeTelegramRequest request)
        {
            try
            {
                var userName = User.FindFirstValue(ClaimTypes.Name) ?? CurrentUserId.ToString();
                var result = await _telegramService.InitializeClientAsync(CurrentUserId, request.PhoneNumber, userName);
                
                if (result.Success)
                {
                    return Ok(new { success = true, message = "Telegram 客户端初始化成功", requiresAuth = false });
                }
                else if (result.RequiresAuth)
                {
                    return Ok(new 
                    { 
                        success = false, 
                        requiresAuth = true,
                        authType = result.AuthType,
                        message = result.AuthType == "verification_code" ? "请输入验证码" : "请输入密码"
                    });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.Message ?? "初始化失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化 Telegram 客户端失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 提交验证码或密码
        /// </summary>
        [HttpPost("submit-auth")]
        public async Task<IActionResult> SubmitAuth([FromBody] SubmitAuthRequest request)
        {
            try
            {
                var result = await _telegramService.SubmitAuthAsync(CurrentUserId, request.Code);
                
                if (result.Success)
                {
                    return Ok(new { success = true, message = "登录成功", requiresAuth = false });
                }
                else if (result.RequiresAuth)
                {
                    return Ok(new 
                    { 
                        success = false, 
                        requiresAuth = true,
                        authType = result.AuthType,
                        message = result.Message ?? "验证码或密码错误，请重试"
                    });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.Message ?? "验证失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "提交验证信息失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 检查连接状态
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var isConnected = _telegramService.IsConnected(CurrentUserId);
            return Ok(new { success = true, isConnected });
        }

        /// <summary>
        /// 获取群组列表（从数据库读取）
        /// </summary>
        [HttpGet("chats")]
        public async Task<IActionResult> GetChats()
        {
            try
            {
                // 从数据库读取
                var chats = await _dbContext.TelegramChats
                    .Where(c => c.UserId == CurrentUserId)
                    .Select(c => new { id = c.ChatId, name = c.Name, isChannel = c.IsChannel })
                    .ToListAsync();
                
                return Ok(new { success = true, data = chats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取群组列表失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 刷新群组列表（从 Telegram 获取并更新数据库）
        /// </summary>
        [HttpPost("refresh-chats")]
        public async Task<IActionResult> RefreshChats()
        {
            try
            {
                // 从 Telegram 获取最新群组
                var chats = await _telegramService.GetChatsAsync(CurrentUserId);
                
                // 删除该用户旧的群组数据
                var oldChats = await _dbContext.TelegramChats
                    .Where(c => c.UserId == CurrentUserId)
                    .ToListAsync();
                _dbContext.TelegramChats.RemoveRange(oldChats);
                
                // 插入新的群组数据
                var newChats = chats.Select(c => new TelegramChat
                {
                    UserId = CurrentUserId,
                    ChatId = c.Id,
                    Name = c.Name,
                    IsChannel = c.IsChannel,
                    UpdateTime = DateTime.Now
                }).ToList();
                
                await _dbContext.TelegramChats.AddRangeAsync(newChats);
                await _dbContext.SaveChangesAsync();
                
                // 返回新数据
                var result = newChats.Select(c => new { id = c.ChatId, name = c.Name, isChannel = c.IsChannel });
                return Ok(new { success = true, data = result, message = $"已刷新 {newChats.Count} 个群组" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新群组列表失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 发送消息到群组
        /// </summary>
        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                var messageId = await _telegramService.SendMessageAsync(
                    CurrentUserId, 
                    request.GroupId, 
                    request.Message);
                
                return Ok(new { success = true, messageId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送消息失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        [HttpPost("disconnect")]
        public IActionResult Disconnect()
        {
            _telegramService.Disconnect(CurrentUserId);
            return Ok(new { success = true, message = "已断开连接" });
        }

        /// <summary>
        /// 获取已存在的 session 手机号列表（用于预填充）
        /// </summary>
        [HttpGet("sessions")]
        public IActionResult GetExistingSessions()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userName))
            {
                return Ok(new { success = true, data = new List<string>() });
            }
            
            var phoneNumbers = _telegramService.GetExistingSessionPhoneNumbers(userName);
            return Ok(new { success = true, data = phoneNumbers });
        }
    }

    public class InitializeTelegramRequest
    {
        public string PhoneNumber { get; set; }
    }

    public class SubmitAuthRequest
    {
        public string Code { get; set; }
    }

    public class SendMessageRequest
    {
        public long GroupId { get; set; }
        public string Message { get; set; }
    }
}

