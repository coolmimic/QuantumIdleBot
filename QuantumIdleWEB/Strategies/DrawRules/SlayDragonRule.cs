using QuantumIdleWEB.GameCore;
using System.Text.Json;

namespace QuantumIdleWEB.Strategies.DrawRules
{
    /// <summary>
    /// 斩龙跟龙规则
    /// </summary>
    public class SlayDragonRule : IDrawRule
    {
        public List<string> GetNextBet(SchemeContext scheme, GroupGameContext context)
        {
            var config = GetConfig(scheme);
            if (config?.MonitorRule == null) return new List<string>();

            var rule = config.MonitorRule;
            var history = context.History;

            // 如果历史数据不足，且不在连投状态，返回
            if (history.Count < rule.RequiredConsecutiveCount && !rule.IsBetting)
                return new List<string>();

            var finalBets = new HashSet<string>();
            var latestRecord = history.FirstOrDefault();
            if (latestRecord == null) return new List<string>();

            // =========================================================
            // 逻辑 A: 处理连投状态
            // =========================================================
            if (rule.TriggerMode == TriggerMode.ContinueBet && rule.IsBetting)
            {
                if (rule.RemainingBetCount > 0)
                {
                    rule.RemainingBetCount--;
                    string target = rule.LockedTargetTag ?? rule.MonitorTags.Split(',')[0];
                    var bets = GenerateBet(scheme, target, rule);
                    if (bets.Count > 0) return bets;
                }

                if (rule.RemainingBetCount <= 0)
                {
                    rule.IsBetting = false;
                    rule.LockedTargetTag = null;
                }

                return new List<string>();
            }

            // =========================================================
            // 逻辑 B: 监控状态 (检查是否满足长龙)
            // =========================================================
            var tags = rule.MonitorTags.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var tag in tags)
            {
                if (IsDragonStreak(scheme, history, tag, rule.RequiredConsecutiveCount))
                {
                    var bets = GenerateBet(scheme, tag, rule);
                    foreach (var b in bets) finalBets.Add(b);

                    if (rule.TriggerMode == TriggerMode.ContinueBet)
                    {
                        rule.IsBetting = true;
                        rule.RemainingBetCount = rule.ContinueBetCount - 1;
                        rule.LockedTargetTag = tag;
                        break;
                    }
                }
            }

            return finalBets.ToList();
        }

        private bool IsDragonStreak(SchemeContext scheme, List<LotteryRecord> history, string targetTag, int count)
        {
            if (history.Count < count) return false;

            for (int i = 0; i < count; i++)
            {
                if (!RecordHasTag(scheme, history[i], targetTag))
                {
                    return false;
                }
            }
            return true;
        }

        private bool RecordHasTag(SchemeContext scheme, LotteryRecord record, string targetTag)
        {
            if (scheme.PlayMode == 1) // Digital
            {
                return record.Result == targetTag;
            }
            else
            {
                var attributes = GameHelper.ParseResultToTags(record.Result, scheme.GameType, scheme.PlayMode);
                return attributes.Contains(targetTag, StringComparer.OrdinalIgnoreCase);
            }
        }

        private List<string> GenerateBet(SchemeContext scheme, string targetTag, SlayDragonMonitorRule rule)
        {
            if (rule.BetMode == BetMode.Fixed)
            {
                if (string.IsNullOrWhiteSpace(rule.FixedBetContent)) return new List<string>();
                return rule.FixedBetContent.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            bool isFollow = (rule.BetMode == BetMode.Follow);
            return GameHelper.GetBetSuggestion(scheme.GameType, scheme.PlayMode, targetTag, isFollow);
        }

        private SlayDragonConfig? GetConfig(SchemeContext scheme)
        {
            if (scheme.DrawRuleConfig == null) return null;
            try
            {
                return JsonSerializer.Deserialize<SlayDragonConfig>(
                    JsonSerializer.Serialize(scheme.DrawRuleConfig),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { return null; }
        }
    }
}
