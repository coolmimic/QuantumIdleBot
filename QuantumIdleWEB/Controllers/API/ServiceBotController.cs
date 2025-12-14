using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuantumIdleModels.Entities;
using QuantumIdleWEB.Data;
using QuantumIdleWEB.Services;
using System.Security.Cryptography;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuantumIdleWeb.Controllers.Api
{
    /// <summary>
    /// 服务机器人控制器 - 处理 @liangziweb_bot 的命令（按钮式交互）
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceBotController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly GameContextService _gameService;
        private readonly ILogger<ServiceBotController> _logger;
        private readonly ITelegramBotClient? _serviceBot;

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
            
            var botToken = config["ServiceBot:BotToken"];
            if (!string.IsNullOrEmpty(botToken))
            {
                _serviceBot = new TelegramBotClient(botToken);
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] Update update)
        {
            if (_serviceBot == null) return Ok();

            try
            {
                // 处理回调查询（按钮点击）
                if (update.CallbackQuery != null)
                {
                    await HandleCallback(update.CallbackQuery);
                    return Ok();
                }

                // 处理消息
                if (update.Message?.Text != null)
                {
                    await HandleMessage(update.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理服务机器人更新失败");
            }

            return Ok();
        }

        [HttpGet("set-webhook")]
        public async Task<IActionResult> SetWebhook()
        {
            if (_serviceBot == null) return BadRequest(new { success = false, message = "Bot未配置" });

            var webhookUrl = _config["ServiceBot:WebhookUrl"];
            if (string.IsNullOrEmpty(webhookUrl))
            {
                return BadRequest(new { success = false, message = "WebhookUrl 未配置" });
            }

            await _serviceBot.SetWebhook(webhookUrl);
            var info = await _serviceBot.GetWebhookInfo();

            return Ok(new { success = true, url = info.Url, pending_updates = info.PendingUpdateCount });
        }

        private async Task HandleMessage(Message message)
        {
            var chatId = message.Chat.Id;
            var text = message.Text?.Trim() ?? "";

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 检查是否已绑定
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId);

            if (text.StartsWith("/start"))
            {
                await ShowWelcome(chatId, user);
            }
            else if (text.StartsWith("/bind "))
            {
                var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    await HandleBind(chatId, parts[1], parts[2], dbContext);
                }
                else
                {
                    await SendMessage(chatId, "⚠️ 格式: /bind 用户名 密码");
                }
            }
            else if (user == null)
            {
                await SendMessage(chatId, "⚠️ 请先绑定账号！\n\n发送: /bind 用户名 密码");
            }
            else
            {
                await ShowMainMenu(chatId, user);
            }
        }

        private async Task HandleCallback(CallbackQuery callback)
        {
            if (_serviceBot == null || callback.Message == null) return;

            var chatId = callback.Message.Chat.Id;
            var data = callback.Data ?? "";

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId);

            if (user == null)
            {
                await _serviceBot.AnswerCallbackQuery(callback.Id, "请先绑定账号！");
                return;
            }

            await _serviceBot.AnswerCallbackQuery(callback.Id);

            switch (data)
            {
                case "status":
                    await ShowStatus(chatId, user);
                    break;
                case "start_bot":
                    await StartBot(chatId, user);
                    break;
                case "stop_bot":
                    await StopBot(chatId, user);
                    break;
                case "mode_sim":
                    _gameService.IsSimulation = true;
                    await SendMessage(chatId, "✅ 已切换到 *模拟模式*", ParseMode.Markdown);
                    break;
                case "mode_real":
                    _gameService.IsSimulation = false;
                    await SendMessage(chatId, "✅ 已切换到 *真实模式*", ParseMode.Markdown);
                    break;
                case "orders":
                    await ShowOrders(chatId, user, dbContext);
                    break;
                case "settings":
                    await ShowSettings(chatId, user);
                    break;
                case "toggle_push_orders":
                    user.PushOrders = !user.PushOrders;
                    await dbContext.SaveChangesAsync();
                    await ShowSettings(chatId, user);
                    break;
                case "toggle_push_alerts":
                    user.PushAlerts = !user.PushAlerts;
                    await dbContext.SaveChangesAsync();
                    await ShowSettings(chatId, user);
                    break;
                case "buy":
                    await ShowBuy(chatId);
                    break;
                case "menu":
                    await ShowMainMenu(chatId, user);
                    break;
                case "unbind":
                    user.TelegramChatId = 0;
                    await dbContext.SaveChangesAsync();
                    await SendMessage(chatId, "✅ 已解绑账号\n\n发送 /start 重新开始");
                    break;
            }
        }

        private async Task ShowWelcome(long chatId, AppUser? user)
        {
            if (user != null)
            {
                await ShowMainMenu(chatId, user);
                return;
            }

            var text = @"⚡ *量子挂机机器人*

欢迎使用！请先绑定您的账号。

*绑定方式:*
发送: `/bind 用户名 密码`

━━━━━━━━━━━━━━
🌐 官网注册: liangzi.love";

            await SendMessage(chatId, text, ParseMode.Markdown);
        }

        private async Task ShowMainMenu(long chatId, AppUser user)
        {
            var runningIcon = _gameService.IsRunning ? "🟢" : "🔴";
            var modeIcon = _gameService.IsSimulation ? "🎮" : "💰";
            var status = _gameService.IsRunning ? "运行中" : "已停止";
            var mode = _gameService.IsSimulation ? "模拟" : "真实";

            var text = $@"⚡ *量子挂机*

👤 用户: {user.UserName}
{runningIcon} 状态: {status}
{modeIcon} 模式: {mode}模式";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] 
                {
                    InlineKeyboardButton.WithCallbackData("📊 状态", "status"),
                    InlineKeyboardButton.WithCallbackData(_gameService.IsRunning ? "⏹ 停止" : "▶️ 开始", _gameService.IsRunning ? "stop_bot" : "start_bot")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🎮 模拟", "mode_sim"),
                    InlineKeyboardButton.WithCallbackData("💰 真实", "mode_real")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("📝 注单", "orders"),
                    InlineKeyboardButton.WithCallbackData("⚙️ 设置", "settings")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("💳 购买续费", "buy")
                }
            });

            await SendMessage(chatId, text, ParseMode.Markdown, keyboard);
        }

        private async Task ShowStatus(long chatId, AppUser user)
        {
            var runningStatus = _gameService.IsRunning ? "🟢 运行中" : "🔴 已停止";
            var modeStatus = _gameService.IsSimulation ? "🎮 模拟模式" : "💰 真实模式";
            var expireStatus = user.ExpireTime > DateTime.Now 
                ? $"✅ {user.ExpireTime:yyyy-MM-dd}" 
                : "❌ 已过期";

            var text = $@"📊 *详细状态*

👤 用户: {user.UserName}
📅 到期: {expireStatus}
{runningStatus}
{modeStatus}

*盈亏统计*
💰 实盘: {(user.Profit >= 0 ? "+" : "")}{user.Profit:F2}
🎮 模拟: {(user.SimProfit >= 0 ? "+" : "")}{user.SimProfit:F2}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("◀️ 返回", "menu") }
            });

            await SendMessage(chatId, text, ParseMode.Markdown, keyboard);
        }

        private async Task StartBot(long chatId, AppUser user)
        {
            if (user.ExpireTime < DateTime.Now)
            {
                await SendMessage(chatId, "❌ 账户已过期，请先续费！");
                return;
            }

            _gameService.IsRunning = true;
            var mode = _gameService.IsSimulation ? "模拟" : "真实";
            _gameService.AddLog($">>> [TG] 开始挂机 ({mode})", user.Id);

            await SendMessage(chatId, $"✅ 挂机已启动！\n当前模式: {mode}模式");
            await ShowMainMenu(chatId, user);
        }

        private async Task StopBot(long chatId, AppUser user)
        {
            _gameService.IsRunning = false;
            _gameService.AddLog(">>> [TG] 挂机已停止", user.Id);

            await SendMessage(chatId, "⏹ 挂机已停止");
            await ShowMainMenu(chatId, user);
        }

        private async Task ShowOrders(long chatId, AppUser user, ApplicationDbContext dbContext)
        {
            var orders = await dbContext.BetOrders
                .Where(o => o.AppUserId == user.Id)
                .OrderByDescending(o => o.BetTime)
                .Take(5)
                .ToListAsync();

            string text;
            if (orders.Count == 0)
            {
                text = "📝 暂无注单记录";
            }
            else
            {
                text = "📝 *最近5条注单*\n\n";
                foreach (var order in orders)
                {
                    var status = order.Status == 1 ? (order.IsWin ? "✅" : "❌") : "⏳";
                    var profit = order.Profit >= 0 ? $"+{order.Profit:F2}" : $"{order.Profit:F2}";
                    text += $"{status} {order.BetContent} | ¥{order.Amount} | {profit}\n";
                }
            }

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("◀️ 返回", "menu") }
            });

            await SendMessage(chatId, text, ParseMode.Markdown, keyboard);
        }

        private async Task ShowSettings(long chatId, AppUser user)
        {
            var pushOrdersIcon = user.PushOrders ? "✅" : "❌";
            var pushAlertsIcon = user.PushAlerts ? "✅" : "❌";

            var text = $@"⚙️ *推送设置*

{pushOrdersIcon} 注单推送: {(user.PushOrders ? "开启" : "关闭")}
{pushAlertsIcon} 报警推送: {(user.PushAlerts ? "开启" : "关闭")}

点击下方按钮切换设置";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{pushOrdersIcon} 注单推送", "toggle_push_orders"),
                    InlineKeyboardButton.WithCallbackData($"{pushAlertsIcon} 报警推送", "toggle_push_alerts")
                },
                new[] { InlineKeyboardButton.WithCallbackData("🔓 解绑账号", "unbind") },
                new[] { InlineKeyboardButton.WithCallbackData("◀️ 返回", "menu") }
            });

            await SendMessage(chatId, text, ParseMode.Markdown, keyboard);
        }

        private async Task ShowBuy(long chatId)
        {
            var text = @"💳 *购买/续费*

📦 月卡 - ¥99 (30天)
📦 季卡 - ¥249 (90天) *推荐*
📦 年卡 - ¥799 (365天)

━━━━━━━━━━━━━━
联系客服购买";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithUrl("📱 联系客服", "https://t.me/your_support") },
                new[] { InlineKeyboardButton.WithUrl("🌐 官网购买", "https://liangzi.love") },
                new[] { InlineKeyboardButton.WithCallbackData("◀️ 返回", "menu") }
            });

            await SendMessage(chatId, text, ParseMode.Markdown, keyboard);
        }

        private async Task HandleBind(long chatId, string username, string password, ApplicationDbContext dbContext)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                await SendMessage(chatId, "❌ 用户名不存在");
                return;
            }

            var inputHash = ComputeHash(password);
            if (user.PasswordHash != inputHash)
            {
                await SendMessage(chatId, "❌ 密码错误");
                return;
            }

            user.TelegramChatId = chatId;
            await dbContext.SaveChangesAsync();

            await SendMessage(chatId, $"✅ 绑定成功！\n\n欢迎回来，*{username}*", ParseMode.Markdown);
            await ShowMainMenu(chatId, user);
        }

        private async Task SendMessage(long chatId, string text, ParseMode parseMode = ParseMode.Html, InlineKeyboardMarkup? replyMarkup = null)
        {
            if (_serviceBot == null) return;
            
            try
            {
                await _serviceBot.SendMessage(chatId, text, parseMode: parseMode, replyMarkup: replyMarkup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送消息失败: chatId={chatId}");
            }
        }

        private string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
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
