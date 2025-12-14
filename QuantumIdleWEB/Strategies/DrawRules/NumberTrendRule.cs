using QuantumIdleWEB.GameCore;
using System.Text.Json;

namespace QuantumIdleWEB.Strategies.DrawRules
{
    /// <summary>
    /// 号码走势规则 (遗漏/连开)
    /// </summary>
    public class NumberTrendRule : IDrawRule
    {
        public List<string> GetNextBet(SchemeContext scheme, GroupGameContext context)
        {
            var config = GetConfig(scheme);
            if (config == null) return new List<string>();

            var history = context.History;
            if (history == null || history.Count == 0) return new List<string>();

            // =========================================================
            // 逻辑 A: 处理连投状态
            // =========================================================
            if (config.TriggerMode == TriggerMode.ContinueBet && config.IsBetting)
            {
                if (config.RemainingBetCount > 0)
                {
                    config.RemainingBetCount--;
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

                if (config.RemainingBetCount <= 0)
                {
                    config.IsBetting = false;
                    config.LockedTargetNumber = null;
                }

                return new List<string>();
            }

            // =========================================================
            // 逻辑 B: 监控状态 (检测遗漏或连开)
            // =========================================================
            var monitorTags = config.MonitorNumbers.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (monitorTags.Length == 0) return new List<string>();

            var metTargets = new List<string>();
            bool allMet = true;

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

            // 根据匹配模式判断是否触发
            bool isTriggered = false;
            List<string> finalTargetsToBet = new();

            if (config.IsFullMatch)
            {
                if (allMet)
                {
                    isTriggered = true;
                    finalTargetsToBet = monitorTags.ToList();
                }
            }
            else
            {
                if (metTargets.Count > 0)
                {
                    isTriggered = true;
                    finalTargetsToBet = metTargets;
                }
            }

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

            if (config.TriggerMode == TriggerMode.ContinueBet)
            {
                config.IsBetting = true;
                config.RemainingBetCount = config.ContinueBetCount - 1;
                config.LockedTargetNumber = string.Join(",", finalTargetsToBet);
            }

            return finalBets.ToList();
        }

        private int GetCurrentTrendCount(SchemeContext scheme, List<LotteryRecord> history, string tag, bool isOmission)
        {
            int count = 0;
            foreach (var record in history)
            {
                bool hasTag = RecordHasTag(scheme, record, tag);

                if (isOmission)
                {
                    if (!hasTag) count++;
                    else break;
                }
                else
                {
                    if (hasTag) count++;
                    else break;
                }
            }
            return count;
        }

        private bool RecordHasTag(SchemeContext scheme, LotteryRecord record, string targetTag)
        {
            if (scheme.PlayMode == 1)
            {
                return record.Result == targetTag;
            }
            else
            {
                var attributes = GameHelper.ParseResultToTags(record.Result, scheme.GameType, scheme.PlayMode);
                return attributes.Contains(targetTag, StringComparer.OrdinalIgnoreCase);
            }
        }

        private List<string> GenerateBet(SchemeContext scheme, string targetTag, NumberTrendConfig config)
        {
            if (config.BetMode == BetMode.Fixed)
            {
                if (string.IsNullOrWhiteSpace(config.FixedBetContent)) return new List<string>();
                return config.FixedBetContent.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            bool isFollow = (config.BetMode == BetMode.Follow);
            return GameHelper.GetBetSuggestion(scheme.GameType, scheme.PlayMode, targetTag, isFollow);
        }

        private NumberTrendConfig? GetConfig(SchemeContext scheme)
        {
            if (scheme.DrawRuleConfig == null) return null;
            try
            {
                return JsonSerializer.Deserialize<NumberTrendConfig>(
                    JsonSerializer.Serialize(scheme.DrawRuleConfig),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { return null; }
        }
    }
}
