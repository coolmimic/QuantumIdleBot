using Microsoft.EntityFrameworkCore;
using QuantumIdleModels.Entities;
using QuantumIdleWEB.Data;
using QuantumIdleWEB.GameCore;
using QuantumIdleWEB.Strategies.DrawRules;
using QuantumIdleWEB.Strategies.OddsRules;
using System.Text.Json;

namespace QuantumIdleWEB.Services
{
    /// <summary>
    /// 投注服务：负责生成注单和发送消息
    /// </summary>
    public class BettingService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly GameContextService _gameService;
        private readonly TelegramClientService _telegramService;
        private readonly ILogger<BettingService> _logger;
        private readonly Random _random = new();

        public BettingService(
            IServiceProvider serviceProvider,
            GameContextService gameService,
            TelegramClientService telegramService,
            ILogger<BettingService> logger)
        {
            _serviceProvider = serviceProvider;
            _gameService = gameService;
            _telegramService = telegramService;
            _logger = logger;
        }

        /// <summary>
        /// 核心入口：执行下注流程
        /// </summary>
        public async Task ProcessBetting(long groupId, GroupGameContext context, int userId)
        {
            GameContextService.CurrentUserId = userId;
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // 1. 获取该群所有运行中的方案
                var schemes = await dbContext.Schemes
                    .Where(s => s.UserId == userId && s.TgGroupId == groupId && s.IsEnabled)
                    .ToListAsync();

                if (schemes.Count == 0)
                {
                    _gameService.AddLog($"[{groupId}] 没有启用的方案", userId);
                    return;
                }

                // 2. 生成注单
                var orders = GenerateOrders(schemes, context, userId);
                if (orders.Count == 0)
                {
                    _gameService.AddLog($"[{groupId}] 没有生成注单", userId);
                    return;
                }

                // 3. 执行下注
                if (_gameService.GetIsSimulation(userId))
                {
                    // 模拟：直接保存
                    await SaveOrders(dbContext, orders);
                    
                    // 查询用户名用于日志显示
                    var user = await dbContext.Users.FindAsync(userId);
                    var userName = user?.UserName ?? $"用户#{userId}";
                    
                    foreach (var order in orders)
                    {
                        // 找到对应的方案名
                        var schemeName = schemes.FirstOrDefault(s => s.Id.ToString() == order.SchemeId)?.Name ?? order.SchemeId;
                        _gameService.AddLog($"[模拟投注] {userName}/{schemeName} | 内容:{order.BetContent} | 金额:{order.Amount}", userId);
                    }
                }
                else
                {
                    // 实盘：发送消息
                    await ExecuteRealBetting(context, orders, dbContext);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理投注时出错");
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                _gameService.AddLog($"[错误] 投注失败: {innerMsg}", userId);
            }
        }

        private List<BetOrder> GenerateOrders(List<Scheme> schemes, GroupGameContext context, int userId)
        {
            var orders = new List<BetOrder>();
            var currentIssue = context.CurrentIssue;

            if (string.IsNullOrEmpty(currentIssue)) return orders;

            foreach (var scheme in schemes)
            {
                try
                {
                    // A. 获取出号规则
                    var schemeContext = CreateSchemeContext(scheme);
                    var drawRule = DrawRuleFactory.GetRule(scheme.DrawRule);
                    var betTargets = drawRule.GetNextBet(schemeContext, context);

                    // 调试日志已移除

                    if (betTargets == null || betTargets.Count == 0)
                    {
                        _gameService.AddLog($"[{scheme.Name}] 出号规则未生成下注内容", scheme.UserId);
                        continue;
                    }

                    // B. 获取倍数
                    var oddsContext = new OddsContext
                    {
                        OddsConfig = string.IsNullOrEmpty(scheme.OddsConfig)
                            ? null
                            : JsonSerializer.Deserialize<object>(scheme.OddsConfig)
                    };
                    var oddsRule = OddsRuleFactory.GetRule(scheme.OddsType);
                    var multiplier = oddsRule.GetNextMultiplier(oddsContext);

                    if (multiplier <= 0) multiplier = 1;

                    // 调试日志已移除

                    // C. 生成订单
                    var issueNum = currentIssue.Length > 50 ? currentIssue.Substring(0, 50) : currentIssue;
                    var order = new BetOrder
                    {
                        AppUserId = userId,
                        SourceRefId = Guid.NewGuid().ToString("N").Substring(0, 16),
                        SchemeId = scheme.Id.ToString(),
                        IssueNumber = issueNum,
                        GameType = scheme.GameType,
                        PlayMode = scheme.PlayMode,
                        BetContent = string.Join(",", betTargets),
                        Amount = multiplier * betTargets.Count, // 基础金额 = 1
                        OpenResult = "",  // 待结算时填入
                        PayoutAmount = 0,
                        Profit = 0,
                        Status = (int)OrderStatus.PendingSettlement,
                        IsSimulation = _gameService.GetIsSimulation(userId),
                        BetTime = DateTime.Now
                    };

                    orders.Add(order);
                }
                catch (Exception ex)
                {
                    _gameService.AddLog($"[生成订单错误] {scheme.Name}: {ex.Message}", scheme.UserId);
                }
            }

            return orders;
        }

        private async Task ExecuteRealBetting(GroupGameContext context, List<BetOrder> orders, ApplicationDbContext dbContext)
        {
            try
            {
                // 格式化下注内容
                var bets = orders.Select(o => new OrderBet
                {
                    BetContent = o.BetContent,
                    BetMultiplier = (int)o.Amount,
                    PlayMode = o.PlayMode
                }).ToList();

                var msgContent = context.FormatOrderBets(bets);
                if (string.IsNullOrWhiteSpace(msgContent))
                {
                    _gameService.AddLog("[下注] 格式化内容为空");
                    return;
                }

                // 随机延迟（防止检测）
                var delay = _random.Next(500, 2000);
                await Task.Delay(delay);

                // 获取用户 ID（所有订单应属于同一用户）
                var userId = orders.First().AppUserId;

                // 发送消息到 Telegram 群组
                var messageId = await _telegramService.SendMessageAsync(userId, context.GroupId, msgContent);

                if (messageId > 0)
                {
                    _gameService.AddLog($"[实盘] 下注已发送 | MsgId:{messageId} | 内容:{msgContent}", userId);
                    
                    // 标记订单状态为已确认，等待机器人回复
                    foreach (var order in orders)
                    {
                        order.TgMsgId = messageId;
                        order.Status = (int)OrderStatus.Confirmed;
                    }
                }
                else
                {
                    _gameService.AddLog($"[实盘] 下注失败 | 内容:{msgContent}", userId);
                    
                    // 标记订单状态为失败
                    foreach (var order in orders)
                    {
                        order.Status = (int)OrderStatus.BetFailed;
                    }
                }

                await SaveOrders(dbContext, orders);
            }
            catch (Exception ex)
            {
                _gameService.AddLog($"[下注异常] {ex.Message}");
            }
        }

        private async Task SaveOrders(ApplicationDbContext dbContext, List<BetOrder> orders)
        {
            dbContext.BetOrders.AddRange(orders);
            await dbContext.SaveChangesAsync();

            // 更新统计
            foreach (var order in orders)
            {
                if (_gameService.IsSimulation)
                {
                    _gameService.SimTurnover += order.Amount;
                }
                else
                {
                    _gameService.Turnover += order.Amount;
                }
            }
        }

        private SchemeContext CreateSchemeContext(Scheme scheme)
        {
            return new SchemeContext
            {
                Id = scheme.Id,
                Name = scheme.Name,
                DrawRule = scheme.DrawRule,
                DrawRuleConfig = string.IsNullOrEmpty(scheme.DrawRuleConfig)
                    ? null
                    : JsonSerializer.Deserialize<object>(scheme.DrawRuleConfig),
                OddsType = scheme.OddsType,
                OddsConfig = string.IsNullOrEmpty(scheme.OddsConfig)
                    ? null
                    : JsonSerializer.Deserialize<object>(scheme.OddsConfig),
                GameType = scheme.GameType,
                PlayMode = scheme.PlayMode,
                TgGroupId = scheme.TgGroupId,
                TgGroupName = scheme.TgGroupName,
                IsEnabled = scheme.IsEnabled,
                EnableStopProfitLoss = scheme.EnableStopProfitLoss,
                StopProfitAmount = scheme.StopProfitAmount,
                StopLossAmount = scheme.StopLossAmount
            };
        }
    }

    public enum OrderStatus
    {
        PendingSettlement = 0,  // 待结算
        Settled = 1,            // 已结算
        BetFailed = 2,          // 下注失败
        Cancelled = 3,          // 已取消
        Confirmed = 4           // 已确认（实盘发送成功）
    }
}
