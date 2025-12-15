using Microsoft.AspNetCore.Mvc;
using QuantumIdleWEB.Data;
using QuantumIdleWEB.Services;
using System.Security.Claims;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace QuantumIdleWeb.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly BotUpdateHandler _updateHandler;
        private readonly IConfiguration _config;
        private readonly ITelegramBotClient _botClient;
        private readonly GameContextService _gameService;
        private readonly ApplicationDbContext _dbContext;

        // 获取当前登录用户ID
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public BotController(
            BotUpdateHandler updateHandler,
            IConfiguration config,
            ITelegramBotClient botClient,
            GameContextService gameService,
            ApplicationDbContext dbContext)
        {
            _updateHandler = updateHandler;
            _config = config;
            _botClient = botClient;
            _gameService = gameService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// 接收 Telegram Webhook 更新
        /// POST: api/bot/update
        /// </summary>
        [HttpPost("update")]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            if (update == null) return Ok();

            _ = Task.Run(async () =>
            {
                try
                {
                    await _updateHandler.HandleUpdateAsync(update);
                }
                catch (Exception ex) { Console.WriteLine($"Bot Error: {ex.Message}"); }
            });

            return Ok();
        }

        /// <summary>
        /// 一键设置 Webhook (只需要运行一次)
        /// GET: api/bot/set-webhook
        /// </summary>
        [HttpGet("set-webhook")]
        public async Task<IActionResult> SetWebhook()
        {
            var webhookUrl = _config["Telegram:WebhookUrl"];
            var secretToken = _config["Telegram:SecretToken"];

            await _botClient.SetWebhook(
              url: webhookUrl,
              secretToken: secretToken
          );

            var info = await _botClient.GetWebhookInfo();

            return Ok(new
            {
                success = true,
                message = "Webhook 设置指令已发送",
                telegram_response = new
                {
                    url = info.Url,
                    has_custom_certificate = info.HasCustomCertificate,
                    pending_update_count = info.PendingUpdateCount,
                    last_error_date = info.LastErrorDate,
                    last_error_message = info.LastErrorMessage
                }
            });
        }

        // ========== 挂机控制 API ==========

        /// <summary>
        /// 获取挂机状态
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var userId = CurrentUserId;
            GameContextService.CurrentUserId = userId;
            var user = await _dbContext.Users.FindAsync(userId);

            return Ok(new
            {
                success = true,
                data = new
                {
                    isRunning = _gameService.IsRunning,
                    isSimulation = _gameService.IsSimulation,
                    balance = _gameService.Balance,
                    profit = user?.Profit ?? 0,
                    turnover = user?.Turnover ?? 0,
                    simProfit = user?.SimProfit ?? 0,
                    simTurnover = user?.SimTurnover ?? 0
                }
            });
        }

        /// <summary>
        /// 开始挂机
        /// </summary>
        [HttpPost("start")]
        public IActionResult Start()
        {
            var userId = CurrentUserId;
            GameContextService.CurrentUserId = userId;
            
            _gameService.IsRunning = true;
            var mode = _gameService.IsSimulation ? "模拟" : "实盘";
            _gameService.AddLog($">>> 开始挂机 ({mode})", userId);
            
            return Ok(new { success = true, message = "已开始挂机", data = _gameService.GetStatus() });
        }

        /// <summary>
        /// 停止挂机
        /// </summary>
        [HttpPost("stop")]
        public IActionResult Stop()
        {
            var userId = CurrentUserId;
            GameContextService.CurrentUserId = userId;
            _gameService.IsRunning = false;
            _gameService.AddLog(">>> 挂机已停止", userId);
            
            return Ok(new { success = true, message = "已停止挂机", data = _gameService.GetStatus() });
        }

        /// <summary>
        /// 切换模拟/真实模式
        /// </summary>
        [HttpPost("mode")]
        public IActionResult ToggleMode([FromBody] ToggleModeRequest request)
        {
            var userId = CurrentUserId;
            GameContextService.CurrentUserId = userId;
            _gameService.IsSimulation = request.IsSimulation;
            var mode = request.IsSimulation ? "模拟模式" : "真实模式";
            _gameService.AddLog($">>> 切换到{mode}", userId);
            
            return Ok(new { success = true, message = $"已切换到{mode}", data = _gameService.GetStatus() });
        }

        /// <summary>
        /// 获取日志
        /// </summary>
        [HttpGet("logs")]
        public IActionResult GetLogs([FromQuery] int count = 100)
        {
            var userId = CurrentUserId;
            var logs = _gameService.GetLogs(userId, count);
            return Ok(new { success = true, data = logs });
        }

        /// <summary>
        /// 清除日志
        /// </summary>
        [HttpPost("logs/clear")]
        public IActionResult ClearLogs()
        {
            var userId = CurrentUserId;
            _gameService.ClearLogs(userId);
            return Ok(new { success = true, message = "日志已清除" });
        }
    }

    public class ToggleModeRequest
    {
        public bool IsSimulation { get; set; }
    }
}
