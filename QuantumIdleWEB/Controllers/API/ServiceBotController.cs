using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuantumIdleModels.Entities;
using QuantumIdleWEB.Data;
using QuantumIdleWEB.Services;
using System.Collections.Concurrent;
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

        // 用户登录状态：chatId -> (userId, state, phoneNumber)
        private static readonly ConcurrentDictionary<long, TgLoginState> _loginStates = new();

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
                if (update.CallbackQuery != null)
                {
                    await HandleCallback(update.CallbackQuery);
                    return Ok();
                }

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
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId);

            // 检查是否在 TG 登录流程中
            if (_loginStates.TryGetValue(chatId, out var loginState) && user != null)
            {
                await HandleTgLoginInput(chatId, text, loginState, user);
                return;
            }

            // 常规消息处理
            switch (text)
            {
                case "/start":
                    await ShowWelcomeWithKeyboard(chatId, user);
                    break;
                case "📊 挂机状态":
                    if (user == null) { await PromptBindWithKeyboard(chatId); return; }
                    await ShowStatus(chatId, user, dbContext);
                    break;
                case "💳 购买卡密":
                    await ShowBuyMenu(chatId);
                    break;
                case "⚙️ 设置":
                    if (user == null) { await PromptBindWithKeyboard(chatId); return; }
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
                            await SendMessageWithReplyKeyboard(chatId, "⚠️ 格式: /bind 用户名 密码");
                        }
                    }
                    else if (user == null)
                    {
                        await PromptBindWithKeyboard(chatId);
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

            // 购买回调不需要绑定
            if (data.StartsWith("buy_"))
            {
                await HandleBuyCallback(chatId, data, dbContext);
                return;
            }

            if (user == null)
            {
                await PromptBindWithKeyboard(chatId);
                return;
            }

            switch (data)
            {
                case "status":
                    await ShowStatus(chatId, user, dbContext);
                    break;
                case "connect_tg":
                    await StartTgLogin(chatId, user);
                    break;
                case "start_bot":
                    await StartBot(chatId, user, dbContext);
                    break;
                case "stop_bot":
                    await StopBot(chatId, user);
                    break;
                case "mode_sim":
                    _gameService.IsSimulation = true;
                    await SendMessageWithReplyKeyboard(chatId, "✅ 已切换到 *模拟模式*", ParseMode.Markdown);
                    await ShowMainMenu(chatId, user, dbContext);
                    break;
                case "mode_real":
                    _gameService.IsSimulation = false;
                    await SendMessageWithReplyKeyboard(chatId, "✅ 已切换到 *真实模式*", ParseMode.Markdown);
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
                    _loginStates.TryRemove(chatId, out _); // 清除登录状态
                    await ShowMainMenu(chatId, user, dbContext);
                    break;
                case "unbind":
                    user.TelegramChatId = 0;
                    await dbContext.SaveChangesAsync();
                    await SendMessageWithReplyKeyboard(chatId, "✅ 已解绑账号\n\n发送 /start 重新开始");
                    break;
            }
        }

        #region TG 登录流程

        private async Task StartTgLogin(long chatId, AppUser user)
        {
            // 已连接则提示
            if (_telegramClientService.IsConnected(user.Id))
            {
                await SendMessageWithReplyKeyboard(chatId, "✅ Telegram 已连接，无需重新登录");
                return;
            }

            // 设置状态：等待手机号
            _loginStates[chatId] = new TgLoginState
            {
                UserId = user.Id,
                UserName = user.UserName ?? "",
                State = LoginStep.WaitingPhoneNumber
            };

            await SendMessageWithReplyKeyboard(chatId,
                "📱 *登录 Telegram*\n\n请输入您的手机号（带国际区号）\n\n例如: `+8613812345678`",
                ParseMode.Markdown);
        }

        private async Task HandleTgLoginInput(long chatId, string text, TgLoginState state, AppUser user)
        {
            switch (state.State)
            {
                case LoginStep.WaitingPhoneNumber:
                    await ProcessPhoneNumber(chatId, text, state, user);
                    break;
                case LoginStep.WaitingVerificationCode:
                    await ProcessVerificationCode(chatId, text, state, user);
                    break;
                case LoginStep.WaitingPassword:
                    await ProcessPassword(chatId, text, state, user);
                    break;
            }
        }

        private async Task ProcessPhoneNumber(long chatId, string phone, TgLoginState state, AppUser user)
        {
            if (!phone.StartsWith("+") || phone.Length < 10)
            {
                await SendMessageWithReplyKeyboard(chatId, "⚠️ 格式错误，请输入正确的手机号\n例如: `+8613812345678`", ParseMode.Markdown);
                return;
            }

            await SendMessageWithReplyKeyboard(chatId, "⏳ 正在发送验证码...");

            var result = await _telegramClientService.InitializeClientAsync(state.UserId, phone, state.UserName);

            if (result.Success)
            {
                _loginStates.TryRemove(chatId, out _);
                await SendMessageWithReplyKeyboard(chatId, "✅ *Telegram 登录成功！*", ParseMode.Markdown);
                
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await ShowMainMenu(chatId, user, dbContext);
            }
            else if (result.RequiresAuth)
            {
                state.PhoneNumber = phone;
                
                if (result.AuthType == "password")
                {
                    state.State = LoginStep.WaitingPassword;
                    await SendMessageWithReplyKeyboard(chatId, "🔐 请输入您的 Telegram 两步验证密码:");
                }
                else
                {
                    state.State = LoginStep.WaitingVerificationCode;
                    await SendMessageWithReplyKeyboard(chatId, "📨 验证码已发送到您的 Telegram 应用\n\n请输入收到的验证码:");
                }
            }
            else
            {
                _loginStates.TryRemove(chatId, out _);
                await SendMessageWithReplyKeyboard(chatId, $"❌ 登录失败: {result.Message}");
            }
        }

        private async Task ProcessVerificationCode(long chatId, string code, TgLoginState state, AppUser user)
        {
            await SendMessageWithReplyKeyboard(chatId, "⏳ 验证中...");

            var result = await _telegramClientService.SubmitAuthAsync(state.UserId, code);

            if (result.Success)
            {
                _loginStates.TryRemove(chatId, out _);
                await SendMessageWithReplyKeyboard(chatId, "✅ *Telegram 登录成功！*", ParseMode.Markdown);
                
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await ShowMainMenu(chatId, user, dbContext);
            }
            else if (result.RequiresAuth && result.AuthType == "password")
            {
                state.State = LoginStep.WaitingPassword;
                await SendMessageWithReplyKeyboard(chatId, "🔐 需要输入两步验证密码:");
            }
            else if (result.RequiresAuth)
            {
                await SendMessageWithReplyKeyboard(chatId, $"⚠️ {result.Message}\n请重新输入验证码:");
            }
            else
            {
                _loginStates.TryRemove(chatId, out _);
                await SendMessageWithReplyKeyboard(chatId, $"❌ 验证失败: {result.Message}");
            }
        }

        private async Task ProcessPassword(long chatId, string password, TgLoginState state, AppUser user)
        {
            await SendMessageWithReplyKeyboard(chatId, "⏳ 验证密码...");

            var result = await _telegramClientService.SubmitAuthAsync(state.UserId, password);

            if (result.Success)
            {
                _loginStates.TryRemove(chatId, out _);
                await SendMessageWithReplyKeyboard(chatId, "✅ *Telegram 登录成功！*", ParseMode.Markdown);
                
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await ShowMainMenu(chatId, user, dbContext);
            }
            else if (result.RequiresAuth)
            {
                await SendMessageWithReplyKeyboard(chatId, $"⚠️ {result.Message}\n请重新输入密码:");
            }
            else
            {
                _loginStates.TryRemove(chatId, out _);
                await SendMessageWithReplyKeyboard(chatId, $"❌ 验证失败: {result.Message}");
            }
        }

        #endregion

        #region 购买流程

        private async Task HandleBuyCallback(long chatId, string data, ApplicationDbContext dbContext)
        {
            int days = 0, amount = 0;

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

            var oldOrders = await dbContext.PaymentOrders
                .Where(o => o.TelegramId == chatId && o.Status == 0)
                .ToListAsync();

            foreach (var o in oldOrders) o.Status = -1;

            var rnd = new Random();
            decimal finalAmount = 0;
            bool foundUnique = false;

            for (int i = 0; i < 10; i++)
            {
                int randomMills = rnd.Next(1, 500);
                decimal discount = randomMills / 1000m;
                decimal tempAmount = baseAmount - discount;

                bool isOccupied = await dbContext.PaymentOrders.AnyAsync(o =>
                    o.Status == 0 && o.RealAmount == tempAmount && o.ExpireTime > DateTime.Now);

                if (!isOccupied)
                {
                    finalAmount = tempAmount;
                    foundUnique = true;
                    break;
                }
            }

            if (!foundUnique)
            {
                await SendMessageWithReplyKeyboard(chatId, "⚠️ 系统繁忙，请稍后再试。");
                return;
            }

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

            await SendMessageWithReplyKeyboard(chatId, text, ParseMode.Markdown);
        }

        #endregion

        #region 菜单和状态显示

        private async Task ShowWelcomeWithKeyboard(long chatId, AppUser? user)
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

            await SendMessageWithReplyKeyboard(chatId, text, ParseMode.Markdown);
        }

        private async Task PromptBindWithKeyboard(long chatId)
        {
            var text = @"⚠️ *请先绑定账号*

发送: `/bind 用户名 密码`

还没有账号？前往官网注册：
🌐 liangzi.love";

            await SendMessageWithReplyKeyboard(chatId, text, ParseMode.Markdown);
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

            var buttons = new List<InlineKeyboardButton[]>();

            // 如果 TG 未连接，显示连接按钮
            if (!isTgConnected)
            {
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("📱 连接 Telegram", "connect_tg") });
            }

            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("📊 详情", "status"),
                InlineKeyboardButton.WithCallbackData(_gameService.IsRunning ? "⏹ 停止" : "▶️ 开始", _gameService.IsRunning ? "stop_bot" : "start_bot")
            });
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("🎮 模拟", "mode_sim"),
                InlineKeyboardButton.WithCallbackData("💰 真实", "mode_real")
            });
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("📝 注单", "orders"),
                InlineKeyboardButton.WithCallbackData("⚙️ 设置", "settings")
            });
            // 小程序入口
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithUrl("🚀 进入小程序", "https://t.me/liangziweb_bot/liangzi")
            });

            var keyboard = new InlineKeyboardMarkup(buttons);
            await SendMessageWithBothKeyboards(chatId, text, ParseMode.Markdown, keyboard);
        }

        private async Task ShowStatus(long chatId, AppUser user, ApplicationDbContext dbContext)
        {
            var isTgConnected = _telegramClientService.IsConnected(user.Id);
            var tgStatus = isTgConnected ? "🟢 已连接" : "🔴 未连接";
            var runningStatus = _gameService.IsRunning ? "🟢 运行中" : "🔴 已停止";
            var modeStatus = _gameService.IsSimulation ? "🎮 模拟模式" : "💰 真实模式";
            var expireStatus = user.ExpireTime > DateTime.Now ? $"✅ {user.ExpireTime:yyyy-MM-dd}" : "❌ 已过期";
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

            await SendMessageWithInline(chatId, text, ParseMode.Markdown, keyboard);
        }

        private async Task StartBot(long chatId, AppUser user, ApplicationDbContext dbContext)
        {
            if (user.ExpireTime < DateTime.Now)
            {
                await SendMessageWithReplyKeyboard(chatId, "❌ 账户已过期，请先续费！");
                return;
            }

            if (!_telegramClientService.IsConnected(user.Id))
            {
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("📱 连接 Telegram", "connect_tg") },
                    new[] { InlineKeyboardButton.WithCallbackData("◀️ 返回", "menu") }
                });
                await SendMessageWithInline(chatId, "❌ Telegram 未连接！请先连接:", ParseMode.Html, keyboard);
                return;
            }

            var hasScheme = await dbContext.Schemes.AnyAsync(s => s.UserId == user.Id && s.IsEnabled);
            if (!hasScheme)
            {
                await SendMessageWithReplyKeyboard(chatId, "❌ 没有启用的方案！\n\n请先在网页端创建并启用方案。");
                return;
            }

            _gameService.IsRunning = true;
            var mode = _gameService.IsSimulation ? "模拟" : "真实";
            _gameService.AddLog($">>> [TG] 开始挂机 ({mode})", user.Id);

            await SendMessageWithReplyKeyboard(chatId, $"✅ 挂机已启动！\n当前模式: {mode}模式");
            await ShowMainMenu(chatId, user, dbContext);
        }

        private async Task StopBot(long chatId, AppUser user)
        {
            _gameService.IsRunning = false;
            _gameService.AddLog(">>> [TG] 挂机已停止", user.Id);
            await SendMessageWithReplyKeyboard(chatId, "⏹ 挂机已停止");

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

            string text = orders.Count == 0 ? "📝 暂无注单记录" : "📝 *最近5条注单*\n\n";
            foreach (var order in orders)
            {
                var status = order.Status == 1 ? (order.IsWin ? "✅" : "❌") : "⏳";
                var profit = order.Profit >= 0 ? $"+{order.Profit:F2}" : $"{order.Profit:F2}";
                text += $"{status} {order.BetContent} | ¥{order.Amount} | {profit}\n";
            }

            var keyboard = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("◀️ 返回", "menu") } });
            await SendMessageWithInline(chatId, text, ParseMode.Markdown, keyboard);
        }

        private async Task ShowSettings(long chatId, AppUser user)
        {
            var pushOrdersIcon = user.PushOrders ? "✅" : "❌";
            var pushAlertsIcon = user.PushAlerts ? "✅" : "❌";

            var text = $@"⚙️ *推送设置*

{pushOrdersIcon} 注单推送: {(user.PushOrders ? "开启" : "关闭")}
{pushAlertsIcon} 报警推送: {(user.PushAlerts ? "开启" : "关闭")}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData($"{pushOrdersIcon} 注单推送", "toggle_push_orders"), InlineKeyboardButton.WithCallbackData($"{pushAlertsIcon} 报警推送", "toggle_push_alerts") },
                new[] { InlineKeyboardButton.WithCallbackData("🔓 解绑账号", "unbind") },
                new[] { InlineKeyboardButton.WithCallbackData("◀️ 返回", "menu") }
            });

            await SendMessageWithInline(chatId, text, ParseMode.Markdown, keyboard);
        }

        private async Task ShowBuyMenu(long chatId)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("⚡️ 1天 (5 U)", "buy_1"), InlineKeyboardButton.WithCallbackData("📅 月卡 (99 U)", "buy_30") },
                new[] { InlineKeyboardButton.WithCallbackData("💎 季卡 (249 U) 🔥", "buy_90"), InlineKeyboardButton.WithCallbackData("👑 年卡 (599 U)", "buy_365") }
            });

            var text = @"💳 *VIP 授权套餐 (USDT-TRC20)*
━━━━━━━━━━━━━━
⚡️ *体验卡*：`5 U` /天
📅 *月卡*：`99 U` (日均 3.3 U)
💎 *季卡*：`249 U` (省 48 U) 🔥
👑 *年卡*：`599 U` (日均仅 1.6 U)
━━━━━━━━━━━━━━
✅ 自动发货 | 24小时无人值守";

            await SendMessageWithInline(chatId, text, ParseMode.Markdown, keyboard);
        }

        private async Task ShowSupport(long chatId)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithUrl("👩‍💻 在线客服", "https://t.me/Ao_8888888") },
                new[] { InlineKeyboardButton.WithUrl("👨‍🔧 技术支持", "https://t.me/Jeffrey31232") }
            });

            await SendMessageWithInline(chatId, "🆘 *官方支持*\n\n点击下方按钮直连人工服务", ParseMode.Markdown, keyboard);
        }

        #endregion

        #region 账号绑定

        private async Task HandleBind(long chatId, string username, string password, ApplicationDbContext dbContext)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                await SendMessageWithReplyKeyboard(chatId, "❌ 用户名不存在");
                return;
            }

            var inputHash = ComputeHash(password);
            if (user.PasswordHash != inputHash)
            {
                await SendMessageWithReplyKeyboard(chatId, "❌ 密码错误");
                return;
            }

            user.TelegramChatId = chatId;
            await dbContext.SaveChangesAsync();

            await SendMessageWithReplyKeyboard(chatId, $"✅ 绑定成功！\n\n欢迎回来，*{username}*", ParseMode.Markdown);
            await ShowMainMenu(chatId, user, dbContext);
        }

        #endregion

        #region 消息发送方法

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

        private async Task SendMessageWithReplyKeyboard(long chatId, string text, ParseMode parseMode = ParseMode.Html)
        {
            if (_serviceBot == null) return;
            try
            {
                await _serviceBot.SendMessage(chatId, text, parseMode: parseMode, replyMarkup: GetMainReplyKeyboard());
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

        private async Task SendMessageWithBothKeyboards(long chatId, string text, ParseMode parseMode, InlineKeyboardMarkup inlineKeyboard)
        {
            if (_serviceBot == null) return;
            try
            {
                // 先发一条消息设置底部键盘
                await _serviceBot.SendMessage(chatId, "📋", replyMarkup: GetMainReplyKeyboard());
                // 再发主要内容和内联键盘
                await _serviceBot.SendMessage(chatId, text, parseMode: parseMode, replyMarkup: inlineKeyboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送消息失败: chatId={chatId}");
            }
        }

        #endregion

        private string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }

        private class TgLoginState
        {
            public int UserId { get; set; }
            public string UserName { get; set; } = "";
            public string PhoneNumber { get; set; } = "";
            public LoginStep State { get; set; }
        }

        private enum LoginStep
        {
            WaitingPhoneNumber,
            WaitingVerificationCode,
            WaitingPassword
        }
    }
}
