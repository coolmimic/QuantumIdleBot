using QuantumIdleWEB.GameCore;
using System.Text.Json;

namespace QuantumIdleWEB.Strategies.DrawRules
{
    /// <summary>
    /// 01形态/走势规则
    /// </summary>
    public class PatternTrendRule : IDrawRule
    {
        public List<string> GetNextBet(SchemeContext scheme, GroupGameContext context)
        {
            var config = GetConfig(scheme);
            if (config == null || config.StrategyList == null || config.StrategyList.Count == 0)
                return new List<string>();

            var history = context.History;
            if (history == null || history.Count == 0) return new List<string>();

            // =========================================================
            // 逻辑 A: 优先处理正在进行的策略
            // =========================================================
            var activeStrategy = config.StrategyList.FirstOrDefault(x => x.IsExecuting);

            if (activeStrategy != null)
            {
                return ProcessActiveStrategy(scheme, history, config, activeStrategy);
            }

            // =========================================================
            // 逻辑 B: 扫描所有策略看是否触发
            // =========================================================
            foreach (var strategy in config.StrategyList)
            {
                if (string.IsNullOrEmpty(strategy.MonitorPattern)) continue;

                if (CheckMonitorPattern(scheme, history, config, strategy.MonitorPattern))
                {
                    strategy.IsExecuting = true;
                    strategy.CurrentStepIndex = 0;

                    if (!string.IsNullOrEmpty(strategy.BetPattern))
                    {
                        char firstCode = strategy.BetPattern[0];
                        var betContent = GenerateBetContent(firstCode, config);
                        strategy.CurrentStepIndex++;
                        return betContent;
                    }
                    else
                    {
                        ResetState(strategy);
                    }

                    break;
                }
            }

            return new List<string>();
        }

        private List<string> ProcessActiveStrategy(SchemeContext scheme, List<LotteryRecord> history, PatternTrendConfig config, TrendStrategyItem strategy)
        {
            if (strategy.CurrentStepIndex > 0)
            {
                char lastBetCode = strategy.BetPattern[strategy.CurrentStepIndex - 1];
                char? lastResultCode = TranslateRecordToCode(scheme, history[0], config);
                bool isWin = lastResultCode.HasValue && lastResultCode.Value == lastBetCode;

                if (isWin && strategy.StopOnWin)
                {
                    ResetState(strategy);
                    return new List<string>();
                }
            }

            if (strategy.CurrentStepIndex >= strategy.BetPattern.Length)
            {
                ResetState(strategy);
                return new List<string>();
            }

            char codeToBet = strategy.BetPattern[strategy.CurrentStepIndex];
            var betContent = GenerateBetContent(codeToBet, config);
            strategy.CurrentStepIndex++;

            return betContent;
        }

        private bool CheckMonitorPattern(SchemeContext scheme, List<LotteryRecord> history, PatternTrendConfig config, string pattern)
        {
            int monitorLen = pattern.Length;
            if (history.Count < monitorLen) return false;

            char[] historyPatternArr = new char[monitorLen];

            for (int i = 0; i < monitorLen; i++)
            {
                var record = history[monitorLen - 1 - i];
                char? code = TranslateRecordToCode(scheme, record, config);

                if (code == null) return false;
                historyPatternArr[i] = code.Value;
            }

            string historyPatternStr = new string(historyPatternArr);
            return historyPatternStr == pattern;
        }

        private void ResetState(TrendStrategyItem strategy)
        {
            strategy.IsExecuting = false;
            strategy.CurrentStepIndex = 0;
        }

        private char? TranslateRecordToCode(SchemeContext scheme, LotteryRecord record, PatternTrendConfig config)
        {
            if (IsMatchDefinition(scheme, record, config.CodeZeroDefinition)) return '0';
            if (IsMatchDefinition(scheme, record, config.CodeOneDefinition)) return '1';
            return null;
        }

        private bool IsMatchDefinition(SchemeContext scheme, LotteryRecord record, string definition)
        {
            var targets = definition.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (scheme.PlayMode == 1)
            {
                return targets.Contains(record.Result);
            }
            else
            {
                var attributes = GameHelper.ParseResultToTags(record.Result, scheme.GameType, scheme.PlayMode);
                return targets.Any(t => attributes.Contains(t, StringComparer.OrdinalIgnoreCase));
            }
        }

        private List<string> GenerateBetContent(char code, PatternTrendConfig config)
        {
            string defStr = (code == '0') ? config.CodeZeroDefinition : config.CodeOneDefinition;
            if (string.IsNullOrWhiteSpace(defStr)) return new List<string>();
            return defStr.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private PatternTrendConfig? GetConfig(SchemeContext scheme)
        {
            if (scheme.DrawRuleConfig == null) return null;
            try
            {
                return JsonSerializer.Deserialize<PatternTrendConfig>(
                    JsonSerializer.Serialize(scheme.DrawRuleConfig),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { return null; }
        }
    }
}
