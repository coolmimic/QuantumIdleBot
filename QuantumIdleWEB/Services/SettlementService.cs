using Microsoft.EntityFrameworkCore;
using QuantumIdleModels.Entities;
using QuantumIdleWEB.Data;
using QuantumIdleWEB.Strategies.OddsRules;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuantumIdleWEB.Services
{
    /// <summary>
    /// 结算服务：负责开奖结算
    /// </summary>
    public class SettlementService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly GameContextService _gameService;
        private readonly NotificationService _notificationService;
        private readonly ILogger<SettlementService> _logger;

        public SettlementService(
            IServiceProvider serviceProvider,
            GameContextService gameService,
            NotificationService notificationService,
            ILogger<SettlementService> logger)
        {
            _serviceProvider = serviceProvider;
            _gameService = gameService;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// 核心入口：处理开奖结算
        /// </summary>
        public async Task ProcessSettlement(long groupId, string issueNumber, string openResult, int userId)
        {
            GameContextService.CurrentUserId = userId;
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // 1. 查询待结算订单
                var pendingOrders = await dbContext.BetOrders
                    .Where(o => o.AppUserId == userId &&
                                o.IssueNumber == issueNumber &&
                                (o.Status == (int)OrderStatus.PendingSettlement ||
                                 o.Status == (int)OrderStatus.Confirmed))
                    .ToListAsync();

                if (pendingOrders.Count == 0) return;

                // 2. 处理每个订单
                foreach (var order in pendingOrders)
                {
                    await ProcessSingleOrder(order, openResult, dbContext);
                }

                await dbContext.SaveChangesAsync();

                // 3. 从缓存中移除已结算的订单
                foreach (var order in pendingOrders)
                {
                    _gameService.RemoveOrder(order.Id, order.TgGroupId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "结算时出错");
                _gameService.AddLog($"[错误] 结算失败: {ex.Message}", userId);
            }
        }

        private async Task ProcessSingleOrder(BetOrder order, string openResult, ApplicationDbContext dbContext)
        {
            // A. 计算派彩
            decimal payout = CalculatePayout(order, openResult);

            // B. 更新订单
            order.OpenResult = openResult;
            order.PayoutAmount = payout;
            order.Status = (int)OrderStatus.Settled;
            order.SettleTime = DateTime.Now;
            order.IsWin = payout > 0;

            // 净盈亏 = 派彩 - 本金
            decimal netProfit = payout - order.Amount;
            order.Profit = netProfit;

            // C. 更新用户数据库记录
            var user = await dbContext.Users.FindAsync(order.AppUserId);
            if (user != null)
            {
                if (order.IsSimulation)
                {
                    user.SimProfit += netProfit;
                    user.SimTurnover += order.Amount;
                }
                else
                {
                    user.Profit += netProfit;
                    user.Turnover += order.Amount;
                }
            }

            // D. 更新方案盈亏记录
            string schemeName = "未知方案";
            string userName = user?.UserName ?? $"用户#{order.AppUserId}";
            if (int.TryParse(order.SchemeId, out int schemeIdInt))
            {
                var scheme = await dbContext.Schemes.FindAsync(schemeIdInt);
                if (scheme != null)
                {
                    schemeName = scheme.Name;
                    if (order.IsSimulation)
                    {
                        scheme.SimProfit += netProfit;
                        scheme.SimTurnover += order.Amount;
                    }
                    else
                    {
                        scheme.Profit += netProfit;
                        scheme.Turnover += order.Amount;
                    }
                }
            }

            // E. 更新方案倍率状态
            await UpdateSchemeOddsState(order, payout > 0, dbContext);

            // F. 日志
            string winLose = netProfit > 0 ? "赢" : (netProfit == 0 ? "平" : "输");
            _gameService.AddLog($"[结算] {userName}/{schemeName} | 结果:{openResult} | {winLose} | 盈亏:{netProfit:F2}", order.AppUserId);

            // G. 推送注单结果到用户 Telegram
            _ = _notificationService.PushOrderResult(order.AppUserId, order);

            // H. 风控检查
            await CheckRiskControl(order, dbContext);
        }

        private decimal CalculatePayout(BetOrder order, string openResult)
        {
            // 简化的判断逻辑
            var betContents = order.BetContent.Split(',');
            int winCount = 0;

            foreach (var bet in betContents)
            {
                if (JudgeWin(bet, openResult, order.GameType, order.PlayMode))
                {
                    winCount++;
                }
            }

            if (winCount > 0)
            {
                // 默认赔率 1.95
                decimal odds = 1.95m;
                return winCount * order.Amount / betContents.Length * odds;
            }

            return 0;
        }

        private bool JudgeWin(string bet, string openResult, int gameType, int playMode)
        {
            // 简化的判断：大小单双
            bet = bet.Trim().ToLower();
            
            // 尝试从结果中提取数字
            if (!TryParseResult(openResult, gameType, out int resultNum))
            {
                return false;
            }

            // 根据游戏类型判断
            switch (gameType)
            {
                case 0: // 扫雷 - 1-6, 规则: 1,2,3小 / 4,5,6大
                    return JudgeBigSmallOddEven(bet, resultNum, 4); // >=4 算大
                    
                case 4: // 哈希彩 - 结果是5个数字，取和，23及以上算大
                    return JudgeBigSmallOddEven(bet, resultNum, 23); // >=23 算大
                    
                default:
                    return JudgeBigSmallOddEven(bet, resultNum, 4);
            }
        }

        private bool JudgeBigSmallOddEven(string bet, int num, int threshold)
        {
            return bet switch
            {
                "大" or "da" => num >= threshold,
                "小" or "x" => num < threshold,
                "单" or "dan" => num % 2 == 1,
                "双" or "s" => num % 2 == 0,
                _ => int.TryParse(bet, out int n) && n == num
            };
        }

        private bool TryParseResult(string result, int gameType, out int num)
        {
            num = 0;
            if (string.IsNullOrEmpty(result)) return false;

            // 如果是逗号分隔
            if (result.Contains(','))
            {
                var parts = result.Split(',');
                num = parts.Select(s => int.TryParse(s.Trim(), out int n) ? n : 0).Sum();
                return true;
            }

            return int.TryParse(result, out num);
        }

        private async Task UpdateSchemeOddsState(BetOrder order, bool isWin, ApplicationDbContext dbContext)
        {
            if (!int.TryParse(order.SchemeId, out int schemeId)) return;
            
            var scheme = await dbContext.Schemes.FindAsync(schemeId);
            if (scheme == null) return;

            // 更新倍率状态
            if (!string.IsNullOrEmpty(scheme.OddsConfig))
            {
                try
                {
                    // 使用不区分大小写的反序列化，避免前端 'sequence' 无法匹配到 'Sequence'
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var config = JsonSerializer.Deserialize<LinearOddsConfig>(scheme.OddsConfig, options);
                    if (config != null && config.Sequence != null && config.Sequence.Count > 0)
                    {
                        // ProgressMode: 0=挂了加倍(输后递增), 1=中了加倍(赢后递增)
                        bool shouldProgress = config.ProgressMode == 1 ? isWin : !isWin;
                        
                        if (shouldProgress)
                        {
                            config.CurrentIndex++;
                            if (config.CurrentIndex >= config.Sequence.Count)
                            {
                                config.CurrentIndex = 0;
                            }
                        }
                        else
                        {
                            config.CurrentIndex = 0;
                        }
                        scheme.OddsConfig = JsonSerializer.Serialize(config);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "更新方案倍率状态失败: {OddsConfig}", scheme.OddsConfig);
                }
            }
        }

        private async Task CheckRiskControl(BetOrder order, ApplicationDbContext dbContext)
        {
            if (!int.TryParse(order.SchemeId, out int schemeId)) return;
            
            var scheme = await dbContext.Schemes.FindAsync(schemeId);
            if (scheme == null || !scheme.EnableStopProfitLoss) return;

            // 计算方案总盈亏
            var totalProfit = await dbContext.BetOrders
                .Where(o => o.SchemeId == order.SchemeId && o.Status == (int)OrderStatus.Settled)
                .SumAsync(o => o.Profit);

            // 止盈检查
            if (scheme.StopProfitAmount > 0 && totalProfit >= scheme.StopProfitAmount)
            {
                scheme.IsEnabled = false;
                _gameService.AddLog($"[风控] 方案 {scheme.Name} 盈利 {totalProfit:F2} 达到目标，已停止", scheme.UserId);
            }

            // 止损检查
            if (scheme.StopLossAmount > 0 && totalProfit <= -scheme.StopLossAmount)
            {
                scheme.IsEnabled = false;
                _gameService.AddLog($"[风控] 方案 {scheme.Name} 亏损 {totalProfit:F2} 达到止损，已停止", scheme.UserId);
            }
        }
    }
}
