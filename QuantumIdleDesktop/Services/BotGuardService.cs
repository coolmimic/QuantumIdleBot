using QuantumIdleDesktop.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuantumIdleDesktop.Services
{
    /// <summary>
    /// 挂机卫士服务
    /// 职责：全权负责挂机过程中的 风控检查、方案轮换、自动止盈止损 以及 日志记录
    /// </summary>
    public static class BotGuardService
    {
        // ==========================================
        // 0. 日志系统 (对外暴露事件)
        // ==========================================

        /// <summary>
        /// 当卫士产生日志时触发。
        /// 主窗体应该订阅此事件：BotGuardService.LogTriggered += (msg) => AppendLog(msg);
        /// </summary>
        public static event Action<string> LogTriggered;

        /// <summary>
        /// 内部日志辅助方法
        /// </summary>
        private static void Log(string message)
        {
            // 加上前缀，让日志看起来更清晰
            string formattedMsg = $"[卫士] {message}";
            LogTriggered?.Invoke(formattedMsg);
        }

        // ==========================================
        // 1. 挂机延迟 (包含日志)
        // ==========================================

        /// <summary>
        /// 执行随机延迟（内部会自动记录日志）
        /// </summary>
        public static async Task WaitRandomDelayAsync()
        {
            var s = CacheData.Settings;
            if (!s.EnableDelay) return;

            int min = Math.Min(s.DelayMinSeconds, s.DelayMaxSeconds);
            int max = Math.Max(s.DelayMinSeconds, s.DelayMaxSeconds);

            if (max <= 0) return;

            int delayMs = new Random().Next(min * 1000, max * 1000);

            // 只有延迟比较长的时候才打印日志，避免刷屏，或者根据需求决定是否打印
            if (delayMs > 1000)
            {
                Log($"随机延迟: 等待 {delayMs / 1000.0:F1} 秒...");
            }

            await Task.Delay(delayMs);
        }

        // ==========================================
        // 2. 投注前检查 (Pre-Bet Check)
        // ==========================================

        /// <summary>
        /// 检查是否允许下注
        /// 如果被拦截，Service 内部会自动 Log 原因，调用者无需处理日志。
        /// </summary>
        /// <returns>true=允许下注, false=被拦截(已记录日志)</returns>
        public static bool CanPlaceBet()
        {
            var s = CacheData.Settings;

            // --- A. 检查定时挂机 ---
            if (s.EnableSchedule && !IsInSchedule(s.ScheduleStartTime, s.ScheduleEndTime))
            {
                // 只有在第一次拦截时打印，或者每隔一段时间打印，防止日志刷屏？
                // 这里简单处理：每次询问都打印（如果调用频率不高），或者由调用者控制频率。
                // 假设调用者每次下注前调用一次，这里打印是合理的。
                Log($"当前时间不在运行时间段内 ({s.ScheduleStartTime:HH:mm} - {s.ScheduleEndTime:HH:mm})，暂停下注。");
                return false;
            }

            // --- B. 检查全局风控 ---
            // 逻辑：如果有轮换规则，则【全局止盈止损】失效，由轮换逻辑接管。
            bool hasRotationRules = s.SchemeRotations != null && s.SchemeRotations.Count > 0;

            if (!hasRotationRules && s.EnableRiskControl)
            {
                decimal totalProfit, totalTurnover;
                lock (CacheData.Schemes)
                {
                    totalProfit = CacheData.Schemes.Sum(item => item.RealProfit);
                    totalTurnover = CacheData.Schemes.Sum(item => item.RealTurnover);
                }

                // 1. 全局止盈
                if (totalProfit >= s.StopProfitAmount)
                {
                    Log($"触发全局止盈！当前总盈利: {totalProfit:F2} (目标: {s.StopProfitAmount:F2})，停止运行。");
                    return false;
                }

                // 2. 全局止损
                if (totalProfit <= -Math.Abs(s.StopLossAmount))
                {
                    Log($"触发全局止损！当前总亏损: {totalProfit:F2} (警戒线: -{s.StopLossAmount:F2})，停止运行。");
                    return false;
                }

                // 3. 全局流水
                if (totalTurnover >= s.StopTurnoverAmount)
                {
                    Log($"触发全局流水限制！当前总流水: {totalTurnover:F2}，停止运行。");
                    return false;
                }
            }

            return true;
        }
        // ==========================================
        // 3. 结算后逻辑 (Post-Settlement)
        // ==========================================

        /// <summary>
        /// 处理结算后的逻辑：方案轮换 OR 单方案止盈止损
        /// (内部自动判断并执行，且自动记录日志)
        /// </summary>
        /// <param name="scheme">刚刚结算的方案</param>
        public static void ProcessSettlementLogic(SchemeModel scheme)
        {
            if (scheme == null) return;

            var s = CacheData.Settings;
            bool hasRotationRules = s.SchemeRotations != null && s.SchemeRotations.Any();

            // 优先判断轮换
            if (hasRotationRules)
            {
                TryRotateScheme(scheme);
            }
            else
            {
                // 没有轮换规则时，执行单方案的风控
                CheckSingleSchemeStop(scheme);
            }
        }
        /// <summary>
        /// 尝试执行方案轮换
        /// </summary>
        private static void TryRotateScheme(SchemeModel currentScheme)
        {
            var rules = CacheData.Settings.SchemeRotations
                .Where(r => r.SourceSchemeId == currentScheme.Id)
                .ToList();

            if (!rules.Any()) return;

            foreach (var rule in rules)
            {
                bool trigger = false;
                string reason = "";

                switch (rule.ConditionType)
                {
                    case RotationConditionType.TakeProfit:
                        if (currentScheme.RealProfit >= currentScheme.StopProfitAmount)
                        {
                            trigger = true;
                            reason = $"盈利达标 ({currentScheme.RealProfit:F2} >= {currentScheme.StopProfitAmount})";
                        }
                        break;

                    case RotationConditionType.StopLoss:
                        if (currentScheme.RealProfit <= -Math.Abs(currentScheme.StopLossAmount))
                        {
                            trigger = true;
                            reason = $"触发止损 ({currentScheme.RealProfit:F2} <= -{currentScheme.StopLossAmount})";
                        }
                        break;
                }

                if (trigger)
                {
                    var targetScheme = CacheData.Schemes.FirstOrDefault(x => x.Id == rule.TargetSchemeId);
                    if (targetScheme != null)
                    {
                        // 执行切换动作

                        currentScheme.RealProfit = 0;
                        currentScheme.RealTurnover = 0;

                        currentScheme.IsEnabled = false;
                        targetScheme.IsEnabled = true;

                        // 【关键】直接在这里打印日志
                        Log($"方案轮换触发 [{currentScheme.Name}] -> [{targetScheme.Name}]。原因: {reason}");

                        // 既然已经切换了，就跳出循环，避免多重切换
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// 检查单方案止盈止损
        /// </summary>
        private static void CheckSingleSchemeStop(SchemeModel scheme)
        {
            if (!scheme.EnableStopProfitLoss || !scheme.IsEnabled) return;

            // 止盈
            if (scheme.RealProfit >= scheme.StopProfitAmount)
            {
                scheme.IsEnabled = false;
                Log($"方案 [{scheme.Name}] 盈利 {scheme.RealProfit:F2} 达到目标，已自动停止。");
            }
            // 止损
            else if (scheme.RealProfit <= -Math.Abs(scheme.StopLossAmount))
            {
                scheme.IsEnabled = false;
                Log($"方案 [{scheme.Name}] 亏损 {scheme.RealProfit:F2} 达到警戒线，已自动停止。");
            }
        }
        // ==========================================
        // 4. 私有工具
        // ==========================================
        private static bool IsInSchedule(DateTime startDt, DateTime endDt)
        {
            var now = DateTime.Now.TimeOfDay;
            var start = startDt.TimeOfDay;
            var end = endDt.TimeOfDay;

            if (start <= end)
                return now >= start && now <= end;
            else
                return now >= start || now <= end;
        }
    }
}