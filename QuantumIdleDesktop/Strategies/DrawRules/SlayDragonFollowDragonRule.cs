using QuantumIdleDesktop.GameCore;
using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Models.DrawRules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantumIdleDesktop.Strategies.DrawRules
{
    public class SlayDragonFollowDragonRule : IDrawRule
    {
        public List<string> GetNextBet(SchemeModel scheme, GroupGameContext context)
        {
            // 1. 基础校验
            if (scheme.DrawRuleConfig is not SlayDragonFollowDragonRuleConfig config)
                return new List<string>();

            var rule = config.MonitorRule;
            var history = context.History;

            // 如果历史数据不足，且不在连投状态，直接返回
            if (history.Count < rule.RequiredConsecutiveCount && !rule.IsBetting)
                return new List<string>();

            var finalBets = new HashSet<string>();
            var latestRecord = history.FirstOrDefault();
            if (latestRecord == null) return new List<string>();

            // =========================================================
            // 逻辑 A: 处理连投状态 (IsBetting = true)
            // =========================================================
            if (rule.TriggerMode == TriggerMode.ContinueBet && rule.IsBetting)
            {
                if (rule.RemainingBetCount > 0)
                {
                    rule.RemainingBetCount--;

                    // 连投期间，直接基于锁定的标签生成注单
                    // 如果 LockedTargetTag 为空 (防御性)，尝试用 MonitorTags 的第一个
                    string target = rule.LockedTargetTag ?? rule.MonitorTags.Split(',')[0];

                    var bets = GenerateBet(scheme, target, rule);
                    if (bets.Count > 0) return bets;
                }

                // 次数耗尽，结束连投
                if (rule.RemainingBetCount <= 0)
                {
                    rule.IsBetting = false;
                    rule.LockedTargetTag = null;
                }

                // 如果刚刚结束连投，本期是否允许重新检测？
                // 通常策略是：连投结束的当期不立即检测新龙，防止逻辑重叠。这里直接返回。
                return new List<string>();
            }

            // =========================================================
            // 逻辑 B: 监控状态 (检查是否满足长龙)
            // =========================================================

            // 拆分监控标签，例如 "大,单" -> ["大", "单"]
            var tags = rule.MonitorTags.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var tag in tags)
            {
                // 检查该 tag 是否形成长龙
                if (IsDragonStreak(scheme, history, tag, rule.RequiredConsecutiveCount))
                {
                    // 命中长龙！

                    // 1. 生成注单
                    var bets = GenerateBet(scheme, tag, rule);
                    foreach (var b in bets) finalBets.Add(b);

                    // 2. 如果是连投模式，开启连投状态
                    if (rule.TriggerMode == TriggerMode.ContinueBet)
                    {
                        rule.IsBetting = true;
                        rule.RemainingBetCount = rule.ContinueBetCount - 1; // 扣除本期
                        rule.LockedTargetTag = tag; // 锁定是因为这个 tag 触发的

                        // 一旦触发连投，通常只锁定这一个龙，跳出循环
                        // (如果不跳出，可能会同时触发 大龙 和 单龙，导致双倍注单，看你需求。这里选择跳出)
                        break;
                    }
                }
            }

            return finalBets.ToList();
        }
        /// <summary>
        /// 检查历史记录是否满足指定 Tag 的连开
        /// </summary>
        private bool IsDragonStreak(SchemeModel scheme, List<LotteryRecord> history, string targetTag, int count)
        {
            // 防御性检查
            if (history.Count < count) return false;

            for (int i = 0; i < count; i++)
            {
                // 只要有一期不包含该 Tag，就断龙
                if (!RecordHasTag(scheme, history[i], targetTag))
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 判断单期结果是否包含目标 Tag
        /// </summary>
        private bool RecordHasTag(SchemeModel scheme, LotteryRecord record, string targetTag)
        {
            if (scheme.PlayMode == GamePlayMode.Digital)
            {
                return record.Result == targetTag;
            }
            else
            {
                // 使用通用的结果标准化 helper
                var attributes = GameStrategyFactory.GetStrategy(scheme.GameType).ParseResult(scheme, record.Result);
                return attributes.Contains(targetTag);
            }
        }
        /// <summary>
        /// 生成注单
        /// </summary>
        private List<string> GenerateBet(SchemeModel scheme, string targetTag, SlayDragonMonitorRule rule)
        {
            // 1. 固定号码
            if (rule.BetMode == BetMode.Fixed)
            {
                if (string.IsNullOrWhiteSpace(rule.FixedBetContent)) return new List<string>();
                return rule.FixedBetContent.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            // 2. 智能跟/反
            bool isFollow = (rule.BetMode == BetMode.Follow);

            // 使用之前写好的通用策略 Helper
            return GameStrategyFactory.GetStrategy(scheme.GameType).GetBetSuggestion(scheme, targetTag, isFollow);
        }
    }
}