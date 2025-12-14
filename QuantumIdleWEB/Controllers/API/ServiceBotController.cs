using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuantumIdleWEB.Data;
using QuantumIdleWEB.Services;
using System.Security.Cryptography;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace QuantumIdleWeb.Controllers.Api
{
    /// <summary>
    /// 服务机器人控制器 - 处理 @liangziweb_bot 的命令
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceBotController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly GameContextService _gameService;
        private readonly ILogger<ServiceBotController> _logger;
        private readonly ITelegramBotClient _serviceBot;

        public ServiceBotController(
            IConfiguration config,
            IServiceProvider serviceProvider,
            GameContextService gameService,
            ILogger<ServiceBotController> logger)
        {
            _config = config;
            _serviceProvider = serviceProvider;
            _gameService = gameService;
            _logger = logger;
            
            // 创建服务机器人客户端
            var botToken = config["ServiceBot:BotToken"];
            if (!string.IsNullOrEmpty(botToken))
            {
                _serviceBot = new TelegramBotClient(botToken);
            }
        }

        /// <summary>
        /// 接收服务机器人 Webhook 更新
        /// </summary>
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] Update update)
        {
            if (update?.Message?.Text == null) return Ok();

            var message = update.Message;
            var chatId = message.Chat.Id;
            var text = message.Text.Trim();
            var userName = message.From?.Username ?? message.From?.FirstName ?? "用户";

            try
            {
                await ProcessCommand(chatId, text, userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理服务机器人命令失败: {text}");
                await SendMessage(chatId, $"❌ 处理命令时出错: {ex.Message}");
            }

            return Ok();
        }

        /// <summary>
        /// 设置 Webhook
        /// </summary>
        [HttpGet("set-webhook")]
        public async Task<IActionResult> SetWebhook()
        {
            var webhookUrl = _config["ServiceBot:WebhookUrl"];
            if (string.IsNullOrEmpty(webhookUrl))
            {
                return BadRequest(new { success = false, message = "WebhookUrl 未配置" });
            }

            await _serviceBot.SetWebhook(webhookUrl);
            var info = await _serviceBot.GetWebhookInfo();

            return Ok(new
            {
                success = true,
                message = "Webhook 设置成功",
                url = info.Url,
                pending_updates = info.PendingUpdateCount
            });
        }

        private async Task ProcessCommand(long chatId, string text, string tgUserName)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 解析命令
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLower().Replace("@liangziweb_bot", "");

            switch (command)
            {
                case "/start":
                    await HandleStart(chatId);
                    break;

                case "/bind":
                    if (parts.Length >= 3)
                    {
                        await HandleBind(chatId, parts[1], parts[2], dbContext);
                    }
                    else
                    {
                        await SendMessage(chatId, "⚠️ 使用方法: /bind <用户名> <密码>");
                    }
                    break;

                case "/status":
                    await HandleStatus(chatId, dbContext);
                    break;

                case "/start_bot":
                    await HandleStartBot(chatId, dbContext);
                    break;

                case "/stop_bot":
                    await HandleStopBot(chatId, dbContext);
                    break;

                case "/sim":
                    await HandleSwitchMode(chatId, true, dbContext);
                    break;

                case "/real":
                    await HandleSwitchMode(chatId, false, dbContext);
                    break;

                case "/orders":
                    await HandleOrders(chatId, dbContext);
                    break;

                case "/buy":
                    await HandleBuy(chatId);
                    break;

                default:
                    await SendMessage(chatId, "❓ 未知命令，发送 /start 查看帮助");
                    break;
            }
        }

        private async Task HandleStart(long chatId)
        {
            var message = @"⚡ *量子挂机机器人*

欢迎使用量子挂机！以下是可用命令：

🔗 *绑定账号*
`/bind <用户名> <密码>` - 绑定您的账户

📊 *挂机控制*
`/status` - 查看挂机状态
`/start_bot` - 开始挂机
`/stop_bot` - 停止挂机
`/sim` - 切换到模拟模式
`/real` - 切换到真实模式

📝 *其他*
`/orders` - 查看最近5条注单
`/buy` - 购买/续费

━━━━━━━━━━━━━━
🌐 官网: https://liangzi.love";

            await SendMessage(chatId, message, ParseMode.Markdown);
        }

        private async Task HandleBind(long chatId, string username, string password, ApplicationDbContext dbContext)
        {
            // 验证用户名密码
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                await SendMessage(chatId, "❌ 用户名不存在");
                return;
            }

            // 使用 SHA256 验证密码
            var inputHash = ComputeHash(password);
            if (user.PasswordHash != inputHash)
            {
                await SendMessage(chatId, "❌ 密码错误");
                return;
            }

            // 绑定 TG Chat ID
            user.TelegramChatId = chatId;
            await dbContext.SaveChangesAsync();

            await SendMessage(chatId, $"✅ 绑定成功！\n\n欢迎回来，*{username}*\n\n现在您可以使用机器人控制挂机了。", ParseMode.Markdown);
        }

        private async Task HandleStatus(long chatId, ApplicationDbContext dbContext)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId);
            if (user == null)
            {
                await SendMessage(chatId, "⚠️ 请先使用 /bind 绑定账号");
                return;
            }

            var runningStatus = _gameService.IsRunning ? "🟢 运行中" : "🔴 已停止";
            var modeStatus = _gameService.IsSimulation ? "🎮 模拟模式" : "💰 真实模式";
            
            // 检查账户到期
            var expireStatus = user.ExpireTime > DateTime.Now 
                ? $"✅ {user.ExpireTime:yyyy-MM-dd HH:mm}" 
                : "❌ 已过期";

            var message = $@"📊 *挂机状态*

👤 用户: {user.UserName}
📅 到期: {expireStatus}

*当前状态*
{runningStatus}
{modeStatus}

*盈亏统计*
💰 实盘: {(user.Profit >= 0 ? "+" : "")}{user.Profit:F2} / 流水 {user.Turnover:F2}
🎮 模拟: {(user.SimProfit >= 0 ? "+" : "")}{user.SimProfit:F2} / 流水 {user.SimTurnover:F2}";

            await SendMessage(chatId, message, ParseMode.Markdown);
        }

        private async Task HandleStartBot(long chatId, ApplicationDbContext dbContext)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId);
            if (user == null)
            {
                await SendMessage(chatId, "⚠️ 请先使用 /bind 绑定账号");
                return;
            }

            if (user.ExpireTime < DateTime.Now)
            {
                await SendMessage(chatId, "❌ 账户已过期，请使用 /buy 续费");
                return;
            }

            _gameService.IsRunning = true;
            var mode = _gameService.IsSimulation ? "模拟" : "真实";
            _gameService.AddLog($">>> [TG] 开始挂机 ({mode})", user.Id);

            await SendMessage(chatId, $"✅ 挂机已启动！\n\n当前模式: {mode}模式");
        }

        private async Task HandleStopBot(long chatId, ApplicationDbContext dbContext)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId);
            if (user == null)
            {
                await SendMessage(chatId, "⚠️ 请先使用 /bind 绑定账号");
                return;
            }

            _gameService.IsRunning = false;
            _gameService.AddLog(">>> [TG] 挂机已停止", user.Id);

            await SendMessage(chatId, "⏹ 挂机已停止");
        }

        private async Task HandleSwitchMode(long chatId, bool simulation, ApplicationDbContext dbContext)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId);
            if (user == null)
            {
                await SendMessage(chatId, "⚠️ 请先使用 /bind 绑定账号");
                return;
            }

            _gameService.IsSimulation = simulation;
            var mode = simulation ? "模拟" : "真实";
            _gameService.AddLog($">>> [TG] 切换到{mode}模式", user.Id);

            await SendMessage(chatId, $"✅ 已切换到 *{mode}模式*", ParseMode.Markdown);
        }

        private async Task HandleOrders(long chatId, ApplicationDbContext dbContext)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId);
            if (user == null)
            {
                await SendMessage(chatId, "⚠️ 请先使用 /bind 绑定账号");
                return;
            }

            var orders = await dbContext.BetOrders
                .Where(o => o.AppUserId == user.Id)
                .OrderByDescending(o => o.BetTime)
                .Take(5)
                .ToListAsync();

            if (orders.Count == 0)
            {
                await SendMessage(chatId, "📝 暂无注单记录");
                return;
            }

            var message = "📝 *最近5条注单*\n\n";
            foreach (var order in orders)
            {
                var status = order.Status == 1 ? (order.IsWin ? "✅" : "❌") : "⏳";
                var profit = order.Profit >= 0 ? $"+{order.Profit:F2}" : $"{order.Profit:F2}";
                message += $"{status} {order.BetContent} | ¥{order.Amount} | {profit}\n";
            }

            await SendMessage(chatId, message, ParseMode.Markdown);
        }

        private async Task HandleBuy(long chatId)
        {
            var message = @"💳 *购买/续费*

请选择套餐：

📦 *月卡* - ¥99 (30天)
📦 *季卡* - ¥249 (90天) 推荐
📦 *年卡* - ¥799 (365天)

━━━━━━━━━━━━━━
联系客服购买: @your_customer_service

或访问官网自助购买:
https://liangzi.love";

            await SendMessage(chatId, message, ParseMode.Markdown);
        }

        private async Task SendMessage(long chatId, string text, ParseMode parseMode = ParseMode.Html)
        {
            if (_serviceBot == null) return;
            
            try
            {
                await _serviceBot.SendMessage(chatId, text, parseMode: parseMode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送消息失败: chatId={chatId}");
            }
        }

        private string ComputeHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
