using QuantumIdleDesktop.GameCore;
using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Models.DrawRules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantumIdleDesktop.Strategies.DrawRules
{
    /// <summary>
    /// 号码走势策略实现 (遗漏/连开)
    /// </summary>
    public class NumberTrendRule : IDrawRule
    {
        public List<string> GetNextBet(SchemeModel scheme, GroupGameContext context)
        {
            // 1. 基础类型校验
            if (scheme.DrawRuleConfig is not NumberTrendRuleConfig config)
                return new List<string>();

            var history = context.History;
            if (history == null || history.Count == 0) return new List<string>();

            // =========================================================
            // 逻辑 A: 处理连投状态 (IsBetting = true)
            // =========================================================
            // 如果处于触发后的连投阶段，直接使用锁定的目标进行投注，不再重复检测
            if (config.TriggerMode == TriggerMode.ContinueBet && config.IsBetting)
            {
                if (config.RemainingBetCount > 0)
                {
                    config.RemainingBetCount--;

                    // 获取锁定的目标 (如果是全匹配触发，可能是 "大,单"，如果是单匹配，可能是 "大")
                    // 防御性代码：如果为空，取配置的第一个
                    string targetStr = config.LockedTargetNumber ?? config.MonitorNumbers.Split(',')[0];
                    var targets = targetStr.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    var bets = new HashSet<string>();
                    foreach (var t in targets)
                    {
                        var generated = GenerateBet(scheme, t, config);
                        foreach (var b in generated) bets.Add(b);
                    }

                    if (bets.Count > 0) return bets.ToList();
                }

                // 次数耗尽，结束连投
                if (config.RemainingBetCount <= 0)
                {
                    config.IsBetting = false;
                    config.LockedTargetNumber = null;
                }

                // 连投结束当期不立即开启新检测，防止逻辑重叠
                return new List<string>();
            }

            // =========================================================
            // 逻辑 B: 监控状态 (检测遗漏或连开)
            // =========================================================

            var monitorTags = config.MonitorNumbers.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (monitorTags.Length == 0) return new List<string>();

            // 1. 计算所有监控号码的当前走势值 (遗漏值 或 连开值)
            var metTargets = new List<string>(); // 存储达标的号码
            bool allMet = true; // 标记是否全部达标 (用于全匹配)

            foreach (var tag in monitorTags)
            {
                int currentCount = GetCurrentTrendCount(scheme, history, tag, config.IsOmissionMode);

                if (currentCount >= config.ThresholdCount)
                {
                    metTargets.Add(tag);
                }
                else
                {
                    allMet = false;
                }
            }

            // 2. 根据匹配模式判断是否触发
            bool isTriggered = false;
            List<string> finalTargetsToBet = new List<string>();

            if (config.IsFullMatch)
            {
                // 【全匹配模式】: 必须所有 tag 都达标
                if (allMet)
                {
                    isTriggered = true;
                    finalTargetsToBet = monitorTags.ToList(); // 投所有监控的号码
                }
            }
            else
            {
                // 【任意匹配模式】: 只要有一个达标
                if (metTargets.Count > 0)
                {
                    isTriggered = true;
                    finalTargetsToBet = metTargets; // 只投达标的那几个
                }
            }

            // 3. 如果未触发，返回空
            if (!isTriggered) return new List<string>();

            // =========================================================
            // 逻辑 C: 生成注单 & 更新状态
            // =========================================================

            var finalBets = new HashSet<string>();
            foreach (var target in finalTargetsToBet)
            {
                var bets = GenerateBet(scheme, target, config);
                foreach (var b in bets) finalBets.Add(b);
            }

            // 如果开启了连投模式，更新状态
            if (config.TriggerMode == TriggerMode.ContinueBet)
            {
                config.IsBetting = true;
                config.RemainingBetCount = config.ContinueBetCount - 1; // 扣除本期

                // 锁定目标：
                // 如果是全匹配，锁定所有配置号码；如果是任意匹配，锁定触发的那几个
                config.LockedTargetNumber = string.Join(",", finalTargetsToBet);
            }

            return finalBets.ToList();
        }

        /// <summary>
        /// 核心算法：获取当前走势计数值
        /// </summary>
        /// <param name="tag">监控的目标 (如 "大", "1")</param>
        /// <param name="isOmission">True=算遗漏(没出的次数), False=算连开(连续出的次数)</param>
        private int GetCurrentTrendCount(SchemeModel scheme, List<LotteryRecord> history, string tag, bool isOmission)
        {
            int count = 0;
            // 从最新一期往前遍历
            foreach (var record in history)
            {
                bool hasTag = RecordHasTag(scheme, record, tag);

                if (isOmission)
                {
                    // === 遗漏模式 ===
                    if (!hasTag) count++; // 没出现，遗漏+1
                    else break;           // 出现了，遗漏中断
                }
                else
                {
                    // === 连开模式 ===
                    if (hasTag) count++;  // 出现了，连开+1
                    else break;           // 没出现，连开中断
                }
            }
            return count;
        }

        /// <summary>
        /// 判断单期结果是否包含目标 Tag
        /// </summary>
        private bool RecordHasTag(SchemeModel scheme, LotteryRecord record, string targetTag)
        {
            if (scheme.PlayMode == GamePlayMode.Digital)
            {
                // 数字玩法，直接比对结果字符串
                return record.Result == targetTag;
            }
            else
            {
                // 双面玩法，使用 Helper 解析属性

                var attributes = GameStrategyFactory.GetStrategy(scheme.GameType).ParseResult(scheme, record.Result);
                return attributes.Contains(targetTag);
            }
        }

        /// <summary>
        /// 生成注单
        /// </summary>
        private List<string> GenerateBet(SchemeModel scheme, string targetTag, NumberTrendRuleConfig config)
        {
            // 1. 固定号码
            if (config.BetMode == BetMode.Fixed)
            {
                if (string.IsNullOrWhiteSpace(config.FixedBetContent)) return new List<string>();
                return config.FixedBetContent.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            // 2. 智能跟/反
            // 正投(Follow): 
            //   - 遗漏模式下: 买它出 (追回补)
            //   - 连开模式下: 买它出 (追龙)
            // 反投(Reverse):
            //   - 遗漏模式下: 买它不出 (杀号)
            //   - 连开模式下: 买它不出 (斩龙)

            // 结论：无论哪种模式，BetMode.Follow 都是买 targetTag 对应的结果
            bool isFollow = (config.BetMode == BetMode.Follow);
            return GameStrategyFactory.GetStrategy(scheme.GameType).GetBetSuggestion(scheme, targetTag, isFollow);
        }
    }
}