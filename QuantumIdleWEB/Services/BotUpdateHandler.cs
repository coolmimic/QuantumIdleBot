using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuantumIdleModels.Entities;
using QuantumIdleWEB.Data;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups; // 必须引用这个用于按钮

namespace QuantumIdleWEB.Services
{
    public class BotUpdateHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IServiceScopeFactory _serviceProvider;
        private readonly IConfiguration _config;

        public BotUpdateHandler(ITelegramBotClient botClient, IServiceScopeFactory serviceProvider, IConfiguration config)
        {
            _botClient = botClient;
            _serviceProvider = serviceProvider;
            _config = config;
        }

        // =========================================================
        // 核心入口
        // =========================================================
        public async Task HandleUpdateAsync(Update update)
        {
            try
            {
                // 1. 处理文本消息 (包括底部菜单的点击)
                if (update.Type == UpdateType.Message && update.Message!.Text != null)
                {
                    await HandleMessageAsync(update.Message);
                }
                // 2. 处理按钮点击 (Inline Keyboard 回调)
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    await HandleCallbackQueryAsync(update.CallbackQuery!);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bot Error: {ex.Message}");
            }
        }

        // =========================================================
        // 消息处理逻辑
        // =========================================================
        private async Task HandleMessageAsync(Message message)
        {
            var chatId = message.Chat.Id;
            var text = message.Text!;

            // 获取数据库上下文
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            switch (text) // 匹配文本内容
            {

                case "/start":
                    await SendMainMenuAsync(chatId, "👋 欢迎使用 **量子** 挂机系统！\n请选择下方功能：");
                    break;

                case "👤 我的账户":
                    await ShowUserProfileAsync(db, chatId, message.From!.Id);
                    break;

                case "💳 购买卡密":
                    await ShowBuyCardMenuAsync(chatId);
                    break;

                case "⬇️ 下载客户端":
                    await _botClient.SendMessage(chatId,
                        "🚀 **最新版本下载**\n\nWindows版: [点击下载](https://qt-a2l.pages.dev/)\n\n请使用电脑浏览器访问下载。",
                        parseMode: ParseMode.Markdown);
                    break;

                case "🆘 联系客服":


                    // 定义按钮：点击直接跳转到 Telegram 私聊
                    var supportKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        // 第一行：业务/充值客服
                        new [] { InlineKeyboardButton.WithUrl("👩‍💻 在线客服 (充值/业务)", "https://t.me/Ao_8888888") },
        
                        // 第二行：技术支持 (请把 Tech_User 换成你技术人员的真实 ID)
                        new [] { InlineKeyboardButton.WithUrl("👨‍🔧 技术支持 (故障/建议)", "https://t.me/Jeffrey31232") }
                    });

                    var sbSupport = new StringBuilder();
                    sbSupport.AppendLine("🆘 **官方支持中心**");
                    sbSupport.AppendLine("➖➖➖➖➖➖➖➖➖➖");
                    sbSupport.AppendLine("在使用过程中遇到任何问题？");
                    sbSupport.AppendLine("请根据您的问题类型，点击下方按钮直连人工服务。");
                    sbSupport.AppendLine("");
                    sbSupport.AppendLine("⏰ **在线时间**：全天候响应 (如未回复请留言)");
                    sbSupport.AppendLine("⚠️ _为了提高效率，请直接描述您遇到的问题_");

                    await _botClient.SendMessage(
                        chatId,
                        sbSupport.ToString(),
                        parseMode: ParseMode.Markdown,
                        replyMarkup: supportKeyboard // 挂载跳转按钮
                    );

                    break;

                default:



                    // 1. 尝试处理生成卡密指令
                    if (text.StartsWith("/card/"))
                    {
                        await HandleGenerateCardCommandAsync(db, message);
                        return; // 处理完直接返回
                    }

                    // 2. 处理绑定指令
                    if (text.StartsWith("/bind"))
                    {
                        await HandleBindCommandAsync(db, message);
                    }
                    // 3. 未知指令
                    else
                    {
                        await _botClient.SendMessage(chatId, "🤔 未知指令，请点击下方菜单。", replyMarkup: GetMainMenuKeyboard());
                    }
                    break;
            }
        }




        /// <summary>
        /// 处理管理员生成卡密指令 (/card/天数)
        /// </summary>
        private async Task HandleGenerateCardCommandAsync(ApplicationDbContext db, Message message)
        {
            var chatId = message.Chat.Id;
            var userId = message.From!.Id;
            var text = message.Text!;

            // --- 1. 权限验证 ---
            // 只有指定的 ID 可以执行此操作
            if (userId != 108234485 && userId != 143263312)
            {
                // 无权限时不回复，或者回复一个装傻的消息
                return;
            }

            // --- 2. 参数解析 ---
            // 截取 "/card/" (6个字符) 后面的部分
            string daysStr = text.Substring(6);

            if (!int.TryParse(daysStr, out int days) || days <= 0)
            {
                await _botClient.SendMessage(chatId, "⚠️ 格式错误。\n用法: `/card/天数`\n示例: `/card/1` (生成1天卡密)", parseMode: ParseMode.Markdown);
                return;
            }

            try
            {
                // --- 3. 业务逻辑 ---

                // 生成 16 位随机大写字符串作为卡密
                string key = $"{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper()}";

                var card = new CardKey
                {
                    KeyCode = key,
                    DurationDays = days,
                    // 记录是谁生成的，方便查账
                    BatchName = $"AdminGen-{userId}",
                    CreateTime = DateTime.Now,
                    IsRedeemed = false
                };

                db.CardKeys.Add(card);
                await db.SaveChangesAsync();

                // --- 4. 结果反馈 ---
                // 使用 Monospace 格式 (`key`) 方便手机端点击复制
                string replyText = $"✅ **生成成功**\n\n" +
                                   $"⏱️ 时长: `{days}` 天\n" +
                                   $"🔑 卡密: `{key}`";

                await _botClient.SendMessage(chatId, replyText, parseMode: ParseMode.Markdown);
            }
            catch (Exception ex)
            {
                // 记录日志并通知错误
                // _logger.LogError(ex, "生成卡密失败");
                await _botClient.SendMessage(chatId, $"❌ 生成失败: 数据库错误 ({ex.Message})");
            }
        }


        // =========================================================
        // 按钮回调处理 (Inline Button)
        // =========================================================
        private async Task HandleCallbackQueryAsync(CallbackQuery callback)
        {
            var chatId = callback.Message!.Chat.Id;
            var data = callback.Data; // 按钮背后藏的数据

            // 告诉 TG 我们收到了点击 (消除按钮上的转圈动画)
            await _botClient.AnswerCallbackQuery(callback.Id);
            switch (data)
            {
                case "buy_1":
                    // 1天，5 U
                    await SendPaymentInfoAsync(chatId, 1, 5);
                    break;

                case "buy_30":
                    // 30天，99 U
                    await SendPaymentInfoAsync(chatId, 30, 99);
                    break;

                case "buy_90":
                    // 90天，199 U (爆款)
                    await SendPaymentInfoAsync(chatId, 90, 249);
                    break;

                case "buy_365":
                    // 365天，599 U (超值)
                    await SendPaymentInfoAsync(chatId, 365, 599);
                    break;

                case "check_payment":
                    await _botClient.SendMessage(chatId, "🔍 正在连接 TRON 节点查询交易...");
                    // TODO: 查账
                    break;
            }
        }

        // =========================================================
        // 辅助方法：发送主菜单 (底部按钮)
        // =========================================================
        private async Task SendMainMenuAsync(long chatId, string text)
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: text,
                parseMode: ParseMode.Markdown,
                replyMarkup: GetMainMenuKeyboard() // 挂载底部键盘
            );
        }

        private ReplyKeyboardMarkup GetMainMenuKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("👤 我的账户"), new KeyboardButton("💳 购买卡密") }, // 第一行
                new[] { new KeyboardButton("⬇️ 下载客户端"), new KeyboardButton("🆘 联系客服") }  // 第二行
            })
            {
                ResizeKeyboard = true // 自动调整按钮大小
            };
        }

        // =========================================================
        // 业务逻辑：显示个人资料
        // =========================================================
        private async Task ShowUserProfileAsync(ApplicationDbContext db, long chatId, long tgUserId)
        {
            // 查找绑定了该 TG ID 的用户
            var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramId == tgUserId);

            if (user == null)
            {
                await _botClient.SendMessage(chatId,
                    "⚠️ **您尚未绑定软件账号**\n\n请在输入框发送：\n`/bind 您的账号 您的密码`\n\n(注意中间有空格)",
                    parseMode: ParseMode.Markdown);
            }
            else
            {
                var sb = new StringBuilder();
                sb.AppendLine("👤 **账户信息**");
                sb.AppendLine($"----------------");
                sb.AppendLine($"🆔 账号：`{user.UserName}`");
                sb.AppendLine($"📅 到期：`{user.ExpireTime:yyyy-MM-dd HH:mm}`");
                sb.AppendLine($"💎 状态：{(user.IsActive == 1 ? "✅ 正常" : "❌ 未激活/封禁")}");

                // 这里加一个“解绑”按钮的例子
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("刷新数据", "refresh_profile")
                });

                await _botClient.SendMessage(chatId, sb.ToString(), parseMode: ParseMode.Markdown, replyMarkup: inlineKeyboard);
            }
        }

        // =========================================================
        // 业务逻辑：处理绑定
        // =========================================================
        private async Task HandleBindCommandAsync(ApplicationDbContext db, Message message)
        {
            var parts = message.Text!.Split(' ');
            if (parts.Length != 3)
            {
                await _botClient.SendMessage(message.Chat.Id, "❌ 格式错误。示例：`/bind user123 123456`", parseMode: ParseMode.Markdown);
                return;
            }

            var username = parts[1];
            // 实际项目请校验哈希，这里演示直接匹配
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                await _botClient.SendMessage(message.Chat.Id, "❌ 账号不存在，请先在软件上注册。");
                return;
            }

            // 绑定 TG
            user.TelegramId = message.From!.Id;
            await db.SaveChangesAsync();

            await _botClient.SendMessage(message.Chat.Id, "✅ **绑定成功！**\n点击【👤 我的账户】查看详情。", parseMode: ParseMode.Markdown);
        }

        // =========================================================
        // 业务逻辑：购买菜单 (Inline Button 示例)
        // =========================================================
        private async Task ShowBuyCardMenuAsync(long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
             {
                // 第一行
                new []
                {
                    InlineKeyboardButton.WithCallbackData("⚡️ 1天 (5 U)", "buy_1"),
                    InlineKeyboardButton.WithCallbackData("📅 月卡 (99 U)", "buy_30")
                },
        
                // 第二行
                new []
                {
                    InlineKeyboardButton.WithCallbackData("💎 季卡 (249 U) 🔥", "buy_90"),
                    InlineKeyboardButton.WithCallbackData("👑 年卡 (599 U) 🩸", "buy_365")
                },
            });

            var sb = new StringBuilder();
            sb.AppendLine("💳 **VIP 授权套餐 (USDT)**");
            sb.AppendLine("➖➖➖➖➖➖➖➖➖➖");
            sb.AppendLine("⚡️ **体验卡**：`5 U` /天");
            sb.AppendLine("📅 **月卡**：`99 U` (日均 3.3 U)");
            sb.AppendLine("💎 **季卡**：`249 U` (省 `48 U`)");
            sb.AppendLine("👑 **年卡**：`599 U` (🔥**日均仅 1.6 U**)");
            sb.AppendLine("➖➖➖➖➖➖➖➖➖➖");
            sb.AppendLine("💡 _购买年卡仅需 599，比买三次季卡立省 150+_");
            sb.AppendLine("✅ 自动发货 | 24小时无人值守");



            await _botClient.SendMessage(
                chatId,
                "💳 **请选择购买套餐**\n自动发货，支持 USDT-TRC20\n" + sb.ToString(),
                parseMode: ParseMode.Markdown,
                replyMarkup: inlineKeyboard);
        }

        private async Task SendPaymentInfoAsync(long chatId, int days, int baseAmount)
        {
           string address = _config["Tron:WalletAddress"];


            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. 先把该用户之前的未支付订单全部标记为过期 (避免一个人占好几个坑)
            var oldOrders = await db.PaymentOrders
                .Where(o => o.TelegramId == chatId && o.Status == 0)
                .ToListAsync();

            foreach (var o in oldOrders)
            {
                o.Status = -1; // 标记已过期/取消
            }

            // 2. 【核心】生成随机数，且支持回收利用过期的金额
            Random rnd = new Random();
            decimal finalAmount = 0;
            bool foundUnique = false;

            // 尝试 10 次，防止极其罕见的碰撞
            for (int i = 0; i < 10; i++)
            {
                int randomMills = rnd.Next(1, 500);
                decimal discount = randomMills / 1000m;
                decimal tempAmount = baseAmount - discount; // 算出 4.923

                // 查重条件：
                // 是否有订单同时满足：
                // 1. 待支付 (Status == 0)
                // 2. 金额占用 (RealAmount == tempAmount)
                // 3. 还没过期 (ExpireTime > Now)  <--- 关键！如果过期了，就可以复用！
                bool isOccupied = await db.PaymentOrders.AnyAsync(o =>
                    o.Status == 0 &&
                    o.RealAmount == tempAmount &&
                    o.ExpireTime > DateTime.Now
                );

                if (!isOccupied)
                {
                    finalAmount = tempAmount;
                    foundUnique = true;
                    break;
                }
            }

            if (!foundUnique)
            {
                await _botClient.SendMessage(chatId, "⚠️ 系统繁忙(金额池已满)，请稍后再试。");
                return;
            }

            // 3. 创建新订单 (设置20分钟有效期)
            var newOrder = new PaymentOrder
            {
                TelegramId = chatId,
                DurationDays = days,
                BaseAmount = baseAmount,
                RealAmount = finalAmount,
                Status = 0,
                CreateTime = DateTime.Now,
                ExpireTime = DateTime.Now.AddMinutes(20) // 👇 设置 20 分钟后过期
            };

            db.PaymentOrders.Add(newOrder);
            await db.SaveChangesAsync();


            var text = $"💎 **订单确认** (20分钟内有效)\n" +
                       $"------------------------------\n" +
                       $"商品：{days}天 授权\n" +
                       $"原价：~~{baseAmount} U~~\n" +
                       $"实付：`{finalAmount:0.000}` (👈点击复制)\n" +
                       $"(已随机立减 `{baseAmount - finalAmount:0.000}` U)\n" +
                       $"------------------------------\n" +
                       $"地址：`{address}` (👈点击复制)\n" +
                       $"------------------------------\n" +
                       $"⚠️ **请在 20 分钟内完成支付，超时后金额将失效！**\n" +
                       $"✅ **转账 `{finalAmount:0.000}` 后自动发货**";

            await _botClient.SendMessage(chatId, text, parseMode: ParseMode.Markdown);

        }
    }
}