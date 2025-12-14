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
        private readonly TelegramClientService _telegramClientService;
        private readonly ILogger<ServiceBotController> _logger;
        private readonly ITelegramBotClient? _serviceBot;

        public ServiceBotController(
            IConfiguration config,
            IServiceProvider serviceProvider,
            GameContextService gameService,
            TelegramClientService telegramClientService,
            ILogger<ServiceBotController> logger)
        {
            _config = config;
            _serviceProvider = serviceProvider;
            _gameService = gameService;
            _telegramClientService = telegramClientService;
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

            // 处理底部键盘按钮
            switch (text)
            {
                case "/start":
                    await ShowWelcome(chatId, user);
                    break;
                case "📊 挂机状态":
                    if (user == null) { await PromptBind(chatId); return; }
                    await ShowStatus(chatId, user, dbContext);
                    break;
                case "💳 购买卡密":
                    await ShowBuyMenu(chatId);
                    break;
                case "⚙️ 设置":
                    if (user == null) { await PromptBind(chatId); return; }
                    await ShowSettings(chatId, user);
                    break;
                case "🆘 联系客服":
                    await ShowSupport(chatId);
                    break;
                default:
                    if (text.StartsWith("/bind "))
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
                        await PromptBind(chatId);
                    }
                    else
                    {
                        await ShowMainMenu(chatId, user, dbContext);
                    }
                    break;
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

            await _serviceBot.AnswerCallbackQuery(callback.Id);

            // 购买相关回调不需要绑定
            if (data.StartsWith("buy_"))
            {
                await HandleBuyCallback(chatId, data, dbContext);
                return;
            }

            if (user == null)
            {
                await PromptBind(chatId);
                return;
            }

            switch (data)
            {
                case "status":
                    await ShowStatus(chatId, user, dbContext);
                    break;
                case "start_bot":
                    await StartBot(chatId, user, dbContext);
                    break;
                case "stop_bot":
                    await StopBot(chatId, user);
                    break;
                case "mode_sim":
                    _gameService.IsSimulation = true;
                    await SendMessage(chatId, "✅ 已切换到 *模拟模式*", ParseMode.Markdown);
                    await ShowMainMenu(chatId, user, dbContext);
                    break;
                case "mode_real":
                    _gameService.IsSimulation = false;
                    await SendMessage(chatId, "✅ 已切换到 *真实模式*", ParseMode.Markdown);
                    await ShowMainMenu(chatId, user, dbContext);
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
                case "menu":
                    await ShowMainMenu(chatId, user, dbContext);
                    break;
                case "unbind":
                    user.TelegramChatId = 0;
                    await dbContext.SaveChangesAsync();
                    await SendMessage(chatId, "✅ 已解绑账号\n\n发送 /start 重新开始");
                    break;
            }
        }

        private async Task HandleBuyCallback(long chatId, string data, ApplicationDbContext dbContext)
        {
            int days = 0;
            int amount = 0;

            switch (data)
            {
                case "buy_1": days = 1; amount = 5; break;
                case "buy_30": days = 30; amount = 99; break;
                case "buy_90": days = 90; amount = 249; break;
                case "buy_365": days = 365; amount = 599; break;
                default: return;
            }

            await SendPaymentInfo(chatId, days, amount, dbContext);
        }

        private async Task SendPaymentInfo(long chatId, int days, int baseAmount, ApplicationDbContext dbContext)
        {
            string address = _config["Tron:WalletAddress"] ?? "";

            // 1. 把之前的未支付订单标记过期
            var oldOrders = await dbContext.PaymentOrders
                .Where(o => o.TelegramId == chatId && o.Status == 0)
                .ToListAsync();

            foreach (var o in oldOrders)
            {
                o.Status = -1;
            }

            // 2. 生成随机金额
            var rnd = new Random();
            decimal finalAmount = 0;
            bool foundUnique = false;

            for (int i = 0; i < 10; i++)
            {
                int randomMills = rnd.Next(1, 500);
                decimal discount = randomMills / 1000m;
                decimal tempAmount = baseAmount - discount;

                bool isOccupied = await dbContext.PaymentOrders.AnyAsync(o =>
                    o.Status == 0 &&
                    o.RealAmount == tempAmount &&
                    o.ExpireTime > DateTime.Now);

                if (!isOccupied)
                {
                    finalAmount = tempAmount;
                    foundUnique = true;
                    break;
                }
            }

            if (!foundUnique)
            {
                await SendMessage(chatId, "⚠️ 系统繁忙，请稍后再试。");
                return;
            }

            // 3. 创建订单
            var newOrder = new PaymentOrder
            {
                TelegramId = chatId,
                DurationDays = days,
                BaseAmount = baseAmount,
                RealAmount = finalAmount,
                Status = 0,
                CreateTime = DateTime.Now,
                ExpireTime = DateTime.Now.AddMinutes(20)
            };

            dbContext.PaymentOrders.Add(newOrder);
            await dbContext.SaveChangesAsync();

            var text = $@"💎 *订单确认* (20分钟内有效)
━━━━━━━━━━━━━━
📦 商品：{days}天 授权
💰 原价：~~{baseAmount} U~~
✅ 实付：`{finalAmount:0.000}` (👈点击复制)
🎁 已随机立减 `{baseAmount - finalAmount:0.000}` U
━━━━━━━━━━━━━━
📍 地址：`{address}` (👈点击复制)
━━━━━━━━━━━━━━
⚠️ *请在 20 分钟内完成支付*
✅ *转账后自动发货卡密*";

            await SendMessage(chatId, text, ParseMode.Markdown);
        }

        private async Task ShowWelcome(long chatId, AppUser? user)
        {
            if (user != null)
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await ShowMainMenu(chatId, user, dbContext);
                return;
            }

            var text = @"⚡ *量子挂机机器人*

欢迎使用！请先绑定您的账号。

*绑定方式:*
发送: `/bind 用户名 密码`

━━━━━━━━━━━━━━
🌐 官网注册: liangzi.love";

            await SendMessage(chatId, text, ParseMode.Markdown, null, GetMainReplyKeyboard());
        }

        private async Task PromptBind(long chatId)
        {
            var text = @"⚠️ *请先绑定账号*

发送: `/bind 用户名 密码`

还没有账号？前往官网注册：
🌐 liangzi.love";

            await SendMessage(chatId, text, ParseMode.Markdown, null, GetMainReplyKeyboard());
        }

        private async Task ShowMainMenu(long chatId, AppUser user, ApplicationDbContext dbContext)
        {
            var isTgConnected = _telegramClientService.IsConnected(user.Id);
            var tgStatus = isTgConnected ? "🟢 已连接" : "🔴 未连接";

            var runningIcon = _gameService.IsRunning ? "🟢" : "🔴";
            var modeIcon = _gameService.IsSimulation ? "🎮" : "💰";
            var status = _gameService.IsRunning ? "运行中" : "已停止";
            var mode = _gameService.IsSimulation ? "模拟" : "真实";

            var text = $@"⚡ *量子挂机*

👤 用户: {user.UserName}
📅 到期: {user.ExpireTime:yyyy-MM-dd}
📡 Telegram: {tgStatus}

{runningIcon} 挂机: {status}
{modeIcon} 模式: {mode}模式";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("📊 详情", "status"),
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
                }
            });

            await SendMessageWithInline(chatId, text, ParseMode.Markdown, keyboard);
        }

        private async Task ShowStatus(long chatId, AppUser user, ApplicationDbContext dbContext)
        {
            var isTgConnected = _telegramClientService.IsConnected(user.Id);
            var tgStatus = isTgConnected ? "🟢 已连接" : "🔴 未连接";
            var runningStatus = _gameService.IsRunning ? "🟢 运行中" : "🔴 已停止";
            var modeStatus = _gameService.IsSimulation ? "🎮 模拟模式" : "💰 真实模式";
            var expireStatus = user.ExpireTime > DateTime.Now
                ? $"✅ {user.ExpireTime:yyyy-MM-dd}"
                : "❌ 已过期";

            // 获取方案数量
            var schemeCount = await dbContext.Schemes.CountAsync(s => s.UserId == user.Id && s.IsEnabled);

            var text = $@"📊 *详细状态*

👤 用户: {user.UserName}
📅 到期: {expireStatus}
📡 Telegram: {tgStatus}
📋 启用方案: {schemeCount} 个

{runningStatus}
{modeStatus}

*盈亏统计*
💰 实盘: {(user.Profit >= 0 ? "+" : "")}{user.Profit:F2} / 流水 {user.Turnover:F2}
🎮 模拟: {(user.SimProfit >= 0 ? "+" : "")}{user.SimProfit:F2} / 流水 {user.SimTurnover:F2}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("◀️ 返回", "menu") }
            });

            await SendMessage(chatId, text, ParseMode.Markdown, keyboard);
        }

        private async Task StartBot(long chatId, AppUser user, ApplicationDbContext dbContext)
        {
            // 检查账户是否过期
            if (user.ExpireTime < DateTime.Now)
            {
                await SendMessage(chatId, "❌ 账户已过期，请先续费！");
                return;
            }

            // 检查 TG 是否连接
            var isTgConnected = _telegramClientService.IsConnected(user.Id);
            if (!isTgConnected)
            {
                await SendMessage(chatId, "❌ Telegram 未连接！\n\n请先在网页端登录您的 Telegram 账号：\n🌐 liangzi.love");
                return;
            }

            // 检查是否有启用的方案
            var hasScheme = await dbContext.Schemes.AnyAsync(s => s.UserId == user.Id && s.IsEnabled);
            if (!hasScheme)
            {
                await SendMessage(chatId, "❌ 没有启用的方案！\n\n请先在网页端创建并启用方案。");
                return;
            }

            _gameService.IsRunning = true;
            var mode = _gameService.IsSimulation ? "模拟" : "真实";
            _gameService.AddLog($">>> [TG] 开始挂机 ({mode})", user.Id);

            await SendMessage(chatId, $"✅ 挂机已启动！\n当前模式: {mode}模式");
            await ShowMainMenu(chatId, user, dbContext);
        }

        private async Task StopBot(long chatId, AppUser user)
        {
            _gameService.IsRunning = false;
            _gameService.AddLog(">>> [TG] 挂机已停止", user.Id);

            await SendMessage(chatId, "⏹ 挂机已停止");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await ShowMainMenu(chatId, user, dbContext);
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

        private async Task ShowBuyMenu(long chatId)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("⚡️ 1天 (5 U)", "buy_1"),
                    InlineKeyboardButton.WithCallbackData("📅 月卡 (99 U)", "buy_30")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("💎 季卡 (249 U) 🔥", "buy_90"),
                    InlineKeyboardButton.WithCallbackData("👑 年卡 (599 U)", "buy_365")
                }
            });

            var text = @"💳 *VIP 授权套餐 (USDT-TRC20)*
━━━━━━━━━━━━━━
⚡️ *体验卡*：`5 U` /天
📅 *月卡*：`99 U` (日均 3.3 U)
💎 *季卡*：`249 U` (省 48 U) 🔥
👑 *年卡*：`599 U` (日均仅 1.6 U)
━━━━━━━━━━━━━━
✅ 自动发货 | 24小时无人值守
💡 点击下方按钮选择套餐";

            await SendMessage(chatId, text, ParseMode.Markdown, keyboard);
        }

        private async Task ShowSupport(long chatId)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithUrl("👩‍💻 在线客服 (充值/业务)", "https://t.me/Ao_8888888") },
                new[] { InlineKeyboardButton.WithUrl("👨‍🔧 技术支持 (故障/建议)", "https://t.me/Jeffrey31232") }
            });

            var text = @"🆘 *官方支持中心*
━━━━━━━━━━━━━━
遇到问题？请点击下方按钮直连人工服务。

⏰ 在线时间：全天候响应
⚠️ 请直接描述您遇到的问题";

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
            await ShowMainMenu(chatId, user, dbContext);
        }

        // 底部固定键盘
        private ReplyKeyboardMarkup GetMainReplyKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("📊 挂机状态"), new KeyboardButton("💳 购买卡密") },
                new[] { new KeyboardButton("⚙️ 设置"), new KeyboardButton("🆘 联系客服") }
            })
            {
                ResizeKeyboard = true
            };
        }

        private async Task SendMessage(long chatId, string text, ParseMode parseMode = ParseMode.Html,
            InlineKeyboardMarkup? inlineKeyboard = null, ReplyKeyboardMarkup? replyKeyboard = null)
        {
            if (_serviceBot == null) return;

            try
            {
                if (inlineKeyboard != null)
                {
                    await _serviceBot.SendMessage(chatId, text, parseMode: parseMode, replyMarkup: inlineKeyboard);
                }
                else if (replyKeyboard != null)
                {
                    await _serviceBot.SendMessage(chatId, text, parseMode: parseMode, replyMarkup: replyKeyboard);
                }
                else
                {
                    await _serviceBot.SendMessage(chatId, text, parseMode: parseMode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送消息失败: chatId={chatId}");
            }
        }

        private async Task SendMessageWithInline(long chatId, string text, ParseMode parseMode, InlineKeyboardMarkup keyboard)
        {
            if (_serviceBot == null) return;

            try
            {
                await _serviceBot.SendMessage(chatId, text, parseMode: parseMode, replyMarkup: keyboard);
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
