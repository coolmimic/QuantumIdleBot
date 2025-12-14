using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuantumIdleModels.DTOs;
using QuantumIdleModels.Entities;
using QuantumIdleWEB.Data;
using System;
using System.Threading.Tasks;

namespace QuantumIdleWeb.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardKeyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public CardKeyController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 允许的管理员密码
        private readonly string[] _adminPasswords = { "mimic998", "adb168" };

        // 1. 生成接口
        // URL: POST api/CardKey/generate
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateCardRequest request)
        {
            // 1. 验证管理员密码
            if (string.IsNullOrEmpty(request.AdminPassword) || 
                !_adminPasswords.Contains(request.AdminPassword))
            {
                return Unauthorized(new { success = false, message = "管理员密码错误" });
            }

            // 2. 生成卡号 
            string newKeyCode = GenerateTimeBasedKey();

            // 3. 创建实体
            var card = new CardKey
            {
                KeyCode = newKeyCode,
                DurationDays = request.DurationDays,
                BatchName = string.IsNullOrEmpty(request.Remark) ? "Robot-Gen" : request.Remark,
                CreateTime = DateTime.Now,
                IsRedeemed = false
            };

            _context.CardKeys.Add(card);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                data = newKeyCode,
                days = request.DurationDays
            });
        }

       
        // 2. 激活接口 (供给 WinForms 客户端调用)
        // URL: POST api/CardKey/activate
        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromBody] ActivateCardRequest request)
        {
            // 1. 找卡
            var card = await _context.CardKeys.FirstOrDefaultAsync(c => c.KeyCode == request.CardCode);

            if (card == null) return BadRequest(new { success = false, message = "卡密无效" });
            if (card.IsRedeemed) return BadRequest(new { success = false, message = "卡密已被使用" });

            // 2. 找用户
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
            if (user == null) return BadRequest(new { success = false, message = "用户不存在" });

            // 3. 计算新过期时间
            DateTime oldExpire = user.ExpireTime;
            DateTime newExpire;

            if (user.ExpireTime > DateTime.Now)
            {
                // 没过期，直接续费
                newExpire = user.ExpireTime.AddDays(card.DurationDays);
            }
            else
            {
                // 已过期，从当前时间开始算
                newExpire = DateTime.Now.AddDays(card.DurationDays);
            }

            // 4. 更新数据
            user.ExpireTime = newExpire;
            user.IsActive = 1; // 确保激活

            card.IsRedeemed = true;
            card.UsedTime = DateTime.Now;
            card.UsedByAppUserId = user.Id;

            // 5. 记录日志
            var log = new CardUsageLog
            {
                UserId = user.Id,
                CardKeyId = card.Id,
                CardCodeSnapshot = card.KeyCode,
                PreviousExpireTime = oldExpire,
                NewExpireTime = newExpire,
                UseTime = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.CardUsageLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"激活成功！有效期至 {newExpire:yyyy-MM-dd HH:mm}",
                expireTime = newExpire
            });
        }

        // 核心修改：生成算法
        // ============================================================
        private string GenerateTimeBasedKey()
        {
            // 1. 获取时间部分: 20251122144500 (14位)
            string timeStr = DateTime.Now.ToString("yyyyMMddHHmmss");

            // 2. 获取 GUID 部分: 去掉横杠，转大写 (32位)
            // Guid.NewGuid().ToString("N") 会生成类似 "d9f4a2c86e2b..." 的字符串
            string guidStr = Guid.NewGuid().ToString("N").ToUpper();

            // 3. 组合
            // 结果示例: 20251122144500-A1B2C3D4E5F67890A1B2C3D4E5F67890
            return $"{timeStr}-{guidStr}";
        }
    }
}