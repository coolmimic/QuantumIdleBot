using QuantumIdleDesktop.GameCore;
using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Models.Odds;
using QuantumIdleDesktop.Strategies.DrawRules;
using QuantumIdleDesktop.Strategies.OddsRules;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TL;

namespace QuantumIdleDesktop.Services
{
    /// <summary>
    /// 投注服务：只负责生成注单和发送消息
    /// </summary>
    public class BettingService
    {
        public event Action<string> OnLog;

        // 单例
        public static BettingService Instance { get; } = new BettingService();
        private readonly object _lockObj = new object();

        private BettingService() { }

        /// <summary>
        /// 核心入口：执行下注流程
        /// </summary>
        public async Task ProcessBetting(long groupId, GroupGameContext context)
        {
            // 1. 获取该群所有运行中的方案
            var schemes = CacheData.Schemes.FindAll(s => s.TgGroupId == groupId && s.IsEnabled);
            if (schemes.Count == 0) return;

            // 2. 生成注单 (纯策略逻辑)
            List<OrderModel> validOrders = GenerateOrders(schemes, context);
            if (validOrders.Count == 0) return;

            // 3. 执行下注与保存
            if (AppGlobal.IsSimulation)
            {
                // 模拟：直接保存
                SaveOrdersToCache(validOrders);
            }
            else
            {
                // 实盘：先发消息，成功了才保存
                bool isBetSuccess = await ExecuteRealBetting(context, validOrders);

                SaveOrdersToCache(validOrders);

            }
        }

        // --- 内部辅助方法 ---

        private async Task<bool> ExecuteRealBetting(GroupGameContext context, List<OrderModel> orders)
        {
            try
            {
                string msgContent = context.FormatOrderBets(orders); // 假设Context里有这个方法
                if (string.IsNullOrWhiteSpace(msgContent)) return false;


                await BotGuardService.WaitRandomDelayAsync();


                // 调用 TG 发送
                var result = await CacheData.tgService.PlaceBetAsync(msgContent, context.GroupId, context.GroupModel.Peer);

                if (result > 0)
                {
                    orders.ForEach(o => { o.TgMsgId = result; o.Status = OrderStatus.Confirmed; });
                    return true;
                }
                else
                {
                    // 发送失败，不保存订单，或者标记为失败
                    orders.ForEach(o => { o.TgMsgId = 0; o.Status = OrderStatus.BetFailed; o.Remark = "接口发送失败"; });
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"[下注异常] {ex.Message}");
                return false;
            }
        }

        private List<OrderModel> GenerateOrders(List<SchemeModel> schemes, GroupGameContext context)
        {
            List<OrderModel> orderList = new List<OrderModel>();
            string currentIssue = context.CurrentIssue;
            if (string.IsNullOrEmpty(currentIssue)) return orderList;

            foreach (var scheme in schemes)
            {
                try
                {
                    // A. 策略：买什么
                    var drawRule = DrawRuleFactory.GetRule(scheme.DrawRule);
                    var betTargets = drawRule.GetNextBet(scheme, context);
                    if (betTargets == null || betTargets.Count == 0) continue;

                    // B. 策略：买多少 (倍投)
                    var multiplier = CalculateStrategyMultiplier(scheme);
                    if (multiplier <= 0) continue;

                    // C. 生成对象
                    decimal baseAmount = 1m; // TODO: 这里以后应该从 Scheme 读取基础金额
                    decimal totalAmount = baseAmount * multiplier * betTargets.Count;

                    var order = new OrderModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        BetTime = DateTime.Now,
                        GroupId = scheme.TgGroupId,
                        GroupName = scheme.TgGroupName,
                        SchemeName = scheme.Name,
                        IssueNumber = currentIssue,
                        GameType = scheme.GameType,
                        PlayMode = scheme.PlayMode,
                        BetContent = string.Join(",", betTargets),
                        BetMultiplier = multiplier,
                        Amount = totalAmount, // 注：这里只记录金额，不扣款，等结算再扣
                        PayoutAmount = 0,
                        PositionLst= scheme.PositionLst,
                        Status = OrderStatus.PendingSettlement,
                        IsSimulation = AppGlobal.IsSimulation
                    };
                    orderList.Add(order);
                }
                catch (Exception ex)
                {
                    OnLog?.Invoke($"[生成出错] {scheme.Name}: {ex.Message}");
                }
            }
            return orderList;
        }



        /// <summary>
        /// 根据全局配置和当前盈亏状态，计算当前方案的倍数
        /// </summary>
        private int CalculateStrategyMultiplier(SchemeModel scheme)
        {
            var config = CacheData.Settings.MultiplyConfig;

            switch (config.Mode)
            {
                // 1. 默认模式：走原来的方案内部逻辑
                case Models.Odds.MultiplyMode.None:
                    var oddsRule = OddsRuleFactory.GetRule(scheme.OddsType);
                    return oddsRule.GetNextMultiplier(scheme);

                // 2. 全局盈亏接管模式：完全忽略 Scheme 内部倍率
                case Models.Odds.MultiplyMode.Profit:
                case Models.Odds.MultiplyMode.Loss:

                    decimal currentPnL = AppGlobal.IsSimulation
                        ? AppGlobal.SimProfit
                        : AppGlobal.Profit;

                    // 直接传入 PnL 和 全局配置，不再传入 scheme
                    return GetMultiplierByPnL(currentPnL, config);

                default:
                    return 0;
            }
        }


        /// <summary>
        /// 纯粹的查表逻辑：根据盈亏金额查全局配置
        /// </summary>
        private int GetMultiplierByPnL(decimal currentPnL, MultiplyConfig config)
        {
            // 1. 模式方向校验
            // 如果是亏损模式(Loss)，但当前是盈利的(>0)，不触发规则，直接返回默认
            if (config.Mode == Models.Odds.MultiplyMode.Loss && currentPnL > 0)
                return config.DefaultMultiplier;

            // 如果是盈利模式(Profit)，但当前是亏损的(<0)，不触发规则，直接返回默认
            if (config.Mode == Models.Odds.MultiplyMode.Profit && currentPnL < 0)
                return config.DefaultMultiplier;

            // 2. 取绝对值准备查表
            decimal absAmount = Math.Abs(currentPnL);

            // 3. 查表 (Items)
            // 逻辑：在配置列表中，找到第一个“触发金额 <= 当前盈亏”的项，取其中金额最大的那个
            // 假设 Items: [{100, 2倍}, {500, 5倍}]
            // 亏损 600 -> 命中 500 -> 5倍
            // 亏损 150 -> 命中 100 -> 2倍
            // 亏损 50  -> 未命中 -> 默认倍

            if (config.Items != null && config.Items.Count > 0)
            {
                var hitItem = config.Items
                    .OrderByDescending(x => x.TriggerAmount) // 倒序，优先匹配大金额
                    .FirstOrDefault(x => absAmount >= x.TriggerAmount);

                if (hitItem != null)
                {
                    OnLog?.Invoke($"[全局倍率] {config.Mode} >= {hitItem.TriggerAmount} 使用 {hitItem.Multiplier}");

                    return hitItem.Multiplier;
                }
            }

            // 4. 未命中任何规则，返回全局配置的默认倍率
            return config.DefaultMultiplier;
        }


        private void SaveOrdersToCache(List<OrderModel> orders)
        {
            lock (_lockObj)
            {
                CacheData.Orders.AddRange(orders);
            }
            // 仅记录日志，不更新财务
            foreach (var order in orders)
            {
                string modeStr = order.IsSimulation ? "[模拟] 模拟成功" : "[实盘] 订单待确认";
                OnLog?.Invoke($"{modeStr} | {order.SchemeName} | 期号:{order.IssueNumber} | 内容:{order.BetContent} | 金额:{order.Amount:F2}");
            }
        }
    }
}