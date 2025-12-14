using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Strategies;
using QuantumIdleDesktop.Strategies.OddsRules;
using QuantumIdleDesktop.Utils; // 引用 GlobalOddsManager
using System;
using System.Collections.Generic;

namespace QuantumIdleDesktop.Services
{
    /// <summary>
    /// 结算服务：只负责开奖、算账、更新财务数据和风控
    /// </summary>
    public class SettlementService
    {

        public event Action<string> OnLog;
        public event Action<bool> OnIsRunning;

        // 单例
        public static SettlementService Instance { get; } = new SettlementService();
        private readonly object _lockObj = new object();

        private SettlementService() { }

        /// <summary>
        /// 核心入口：处理开奖
        /// </summary>
        public void ProcessSettlement(long groupId, string issueNumber, string openResult)
        {
            // 1. 查询待结算订单
            var targetOrders = GetPendingOrders(groupId, issueNumber);
            if (targetOrders.Count == 0) return;

            foreach (var order in targetOrders)
            {
                var scheme = CacheData.Schemes.Find(s => s.Name == order.SchemeName);
                if (scheme == null) continue;

                // 2. 处理单个订单
                ProcessSingleOrder(order, scheme, openResult, issueNumber);
            }
        }

        private void ProcessSingleOrder(OrderModel order, SchemeModel scheme, string openResult, string issueNumber)
        {
            // A. 算钱 (Calculate)
            decimal payout = CalculatePayout(order, scheme, openResult);

            // B. 改单 (Status)
            order.OpenResult = openResult;
            order.PayoutAmount = payout;
            order.Status = OrderStatus.Settled;

            // 净盈亏 = 派彩 - 本金
            decimal netProfit = payout - order.Amount;

            // C. 记账 (Statistics) - 关键：流水和盈亏都在这里统一更新！
            UpdateStatistics(scheme, order, netProfit);

            // D. 策略流转 (Strategy)
            var oddsRule = OddsRuleFactory.GetRule(scheme.OddsType);
            oddsRule.UpdateState(scheme, order);

            // E. 日志
            string winLose = netProfit > 0 ? "赢" : (netProfit == 0 ? "平" : "输");
            OnLog?.Invoke($"[结算] 方案:{order.SchemeName} | 结果:{openResult} | {winLose} | 盈亏:{netProfit:F2}");

            // F. 风控卫士 (Guard) - 结算完立刻检查是否需要停机
            CheckRiskControl(scheme);
        }

        private List<OrderModel> GetPendingOrders(long groupId, string issueNumber)
        {
            lock (_lockObj)
            {
                return CacheData.Orders.FindAll(o =>
                    o.GroupId == groupId &&
                    o.IssueNumber == issueNumber &&
                    o.Status == OrderStatus.PendingSettlement
                );
            }
        }

        private decimal CalculatePayout(OrderModel order, SchemeModel scheme, string openResult)
        {
            order.OpenResult = openResult; // 临时赋值供 Judge 使用
            var judge = GameStrategyFactory.GetStrategy(scheme.GameType);
            int winCount = judge.Judge(order);

            if (winCount > 0)
            {
                // 获取赔率
                decimal odds = GlobalOddsManager.Instance.GetOdds(scheme.GameType, scheme.PlayMode);
                return winCount * order.BetMultiplier * odds; // 假设这里 baseAmount 已包含在 multiplier 或 amount 逻辑中
            }
            return 0;
        }

        /// <summary>
        /// 更新财务数据 (流水在这里加，盈亏也在这里加)
        /// </summary>
        private void UpdateStatistics(SchemeModel scheme, OrderModel order, decimal netProfit)
        {
            if (order.IsSimulation)
            {
                // 模拟盘
                scheme.SimulatedProfit += netProfit;
                scheme.SimulatedTurnover += order.Amount; // 流水在这里增加

                AppGlobal.SimProfit += netProfit;
                AppGlobal.SimTurnover += order.Amount;
            }
            else
            {
                // 实盘
                scheme.RealProfit += netProfit;
                scheme.RealTurnover += order.Amount; // 流水在这里增加

                AppGlobal.Profit += netProfit;
                AppGlobal.Turnover += order.Amount;
            }
        }

        private void CheckRiskControl(SchemeModel scheme)
        {
            // 1. 检查方案单独止盈止损
            BotGuardService.ProcessSettlementLogic(scheme);




            // 2. 检查全局风控
            bool IsRunning = BotGuardService.CanPlaceBet();

            if (IsRunning != AppGlobal.IsRunning)
            {
                OnIsRunning?.Invoke(IsRunning);
            }
        }
    }
}