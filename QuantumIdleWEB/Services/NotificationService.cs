using Microsoft.EntityFrameworkCore;
using QuantumIdleModels.Entities;
using QuantumIdleWEB.Data;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace QuantumIdleWEB.Services
{
    /// <summary>
    /// 推送通知服务 - 向用户的 Telegram 发送注单和报警通知
    /// </summary>
    public class NotificationService
    {
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationService> _logger;
        private readonly ITelegramBotClient? _serviceBot;

        public NotificationService(
            IConfiguration config,
            IServiceProvider serviceProvider,
            ILogger<NotificationService> logger)
        {
            _config = config;
            _serviceProvider = serviceProvider;
            _logger = logger;

            var botToken = config["ServiceBot:BotToken"];
            if (!string.IsNullOrEmpty(botToken))
            {
                _serviceBot = new TelegramBotClient(botToken);
            }
        }

        /// <summary>
        /// 推送注单结果
        /// </summary>
        public async Task PushOrderResult(int userId, BetOrder order)
        {
            if (_serviceBot == null) return;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var user = await dbContext.Users.FindAsync(userId);
                if (user == null || user.TelegramChatId == 0 || !user.PushOrders)
                    return;

                var winIcon = order.IsWin ? "✅" : "❌";
                var profitSign = order.Profit >= 0 ? "+" : "";
                
                var text = $@"📝 *注单结果*

{winIcon} {order.BetContent}
💰 金额: {order.Amount:F2}
🎯 结果: {order.OpenResult}
📊 盈亏: {profitSign}{order.Profit:F2}";

                await _serviceBot.SendMessage(user.TelegramChatId, text, parseMode: ParseMode.Markdown);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"推送注单失败: userId={userId}");
            }
        }

        /// <summary>
        /// 推送报警信息（下注失败等）
        /// </summary>
        public async Task PushAlert(int userId, string title, string message)
        {
            if (_serviceBot == null) return;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var user = await dbContext.Users.FindAsync(userId);
                if (user == null || user.TelegramChatId == 0 || !user.PushAlerts)
                    return;

                var text = $@"⚠️ *{title}*

{message}";

                await _serviceBot.SendMessage(user.TelegramChatId, text, parseMode: ParseMode.Markdown);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"推送报警失败: userId={userId}");
            }
        }

        /// <summary>
        /// 推送方案状态变更（止盈止损触发等）
        /// </summary>
        public async Task PushSchemeStatus(int userId, string schemeName, string status, string reason)
        {
            if (_serviceBot == null) return;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var user = await dbContext.Users.FindAsync(userId);
                if (user == null || user.TelegramChatId == 0 || !user.PushAlerts)
                    return;

                var text = $@"🎯 *方案状态*

📋 方案: {schemeName}
📊 状态: {status}
📝 原因: {reason}";

                await _serviceBot.SendMessage(user.TelegramChatId, text, parseMode: ParseMode.Markdown);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"推送方案状态失败: userId={userId}");
            }
        }
    }
}
