using QuantumIdleWEB.GameCore;
using System.Text.Json;

namespace QuantumIdleWEB.Strategies.DrawRules
{
    /// <summary>
    /// 分支走势规则
    /// 逻辑：监控 -> 首投 -> 根据结果进入 赢/输 分支序列
    /// </summary>
    public class BranchTrendRule : IDrawRule
    {
        public List<string> GetNextBet(SchemeContext scheme, GroupGameContext context)
        {
            var config = GetConfig(scheme);
            if (config == null || string.IsNullOrEmpty(config.MonitorPattern) || string.IsNullOrEmpty(config.InitialBet))
                return new List<string>();

            var history = context.History;
            if (history == null || history.Count == 0) return new List<string>();

            // =========================================================
            // 逻辑 A: 处于投注状态中
            // =========================================================
            if (config.IsBetting)
            {
                string lastBetCodeStr = GetLastBetCode(config);
                if (string.IsNullOrEmpty(lastBetCodeStr))
                {
                    ResetState(config);
                    return new List<string>();
                }

                char lastBetCode = lastBetCodeStr[0];
                char? actualResultCode = TranslateRecordToCode(scheme, history[0], config);
                bool isWin = actualResultCode.HasValue && actualResultCode.Value == lastBetCode;

                // 中奖即停
                if (isWin && config.StopOnWin)
                {
                    ResetState(config);
                    return new List<string>();
                }

                // 状态流转
                if (config.CurrentState == BranchBettingState.Initial)
                {
                    if (isWin)
                    {
                        config.CurrentState = BranchBettingState.WinSequence;
                        config.CurrentStepIndex = 0;
                    }
                    else
                    {
                        config.CurrentState = BranchBettingState.LossSequence;
                        config.CurrentStepIndex = 0;
                    }
                }
                else
                {
                    config.CurrentStepIndex++;
                }

                // 执行新状态下的投注
                string activePattern = GetActivePattern(config);
                if (string.IsNullOrEmpty(activePattern) || config.CurrentStepIndex >= activePattern.Length)
                {
                    ResetState(config);
                    return new List<string>();
                }

                char nextCode = activePattern[config.CurrentStepIndex];
                return GenerateBetContent(nextCode, config);
            }

            // =========================================================
            // 逻辑 B: 监控状态
            // =========================================================
            int monitorLen = config.MonitorPattern.Length;
            if (history.Count < monitorLen) return new List<string>();

            char[] historyArr = new char[monitorLen];
            bool isValid = true;

            for (int i = 0; i < monitorLen; i++)
            {
                var record = history[monitorLen - 1 - i];
                char? code = TranslateRecordToCode(scheme, record, config);

                if (code == null)
                {
                    isValid = false;
                    break;
                }
                historyArr[i] = code.Value;
            }

            if (!isValid) return new List<string>();

            string historyStr = new string(historyArr);

            if (historyStr == config.MonitorPattern)
            {
                config.IsBetting = true;
                config.CurrentState = BranchBettingState.Initial;
                config.CurrentStepIndex = 0;

                if (!string.IsNullOrEmpty(config.InitialBet))
                {
                    return GenerateBetContent(config.InitialBet[0], config);
                }
            }

            return new List<string>();
        }

        private void ResetState(BranchTrendConfig config)
        {
            config.IsBetting = false;
            config.CurrentState = BranchBettingState.None;
            config.CurrentStepIndex = 0;
        }

        private string? GetActivePattern(BranchTrendConfig config)
        {
            if (config.CurrentState == BranchBettingState.WinSequence) return config.WinPattern;
            if (config.CurrentState == BranchBettingState.LossSequence) return config.LossPattern;
            return null;
        }

        private string? GetLastBetCode(BranchTrendConfig config)
        {
            if (config.CurrentState == BranchBettingState.Initial)
            {
                return config.InitialBet;
            }
            else if (config.CurrentState == BranchBettingState.WinSequence || config.CurrentState == BranchBettingState.LossSequence)
            {
                string? pattern = GetActivePattern(config);
                if (string.IsNullOrEmpty(pattern) || config.CurrentStepIndex >= pattern.Length) return null;
                return pattern[config.CurrentStepIndex].ToString();
            }
            return null;
        }

        private char? TranslateRecordToCode(SchemeContext scheme, LotteryRecord record, BranchTrendConfig config)
        {
            if (IsMatchDefinition(scheme, record, config.CodeZeroDefinition)) return '0';
            if (IsMatchDefinition(scheme, record, config.CodeOneDefinition)) return '1';
            return null;
        }

        private bool IsMatchDefinition(SchemeContext scheme, LotteryRecord record, string definition)
        {
            if (string.IsNullOrWhiteSpace(definition)) return false;
            var targets = definition.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (scheme.PlayMode == 1)
            {
                return targets.Contains(record.Result);
            }
            else
            {
                var attrs = GameHelper.ParseResultToTags(record.Result, scheme.GameType, scheme.PlayMode);
                return targets.Any(t => attrs.Contains(t, StringComparer.OrdinalIgnoreCase));
            }
        }

        private List<string> GenerateBetContent(char code, BranchTrendConfig config)
        {
            string defStr = (code == '0') ? config.CodeZeroDefinition : config.CodeOneDefinition;
            if (string.IsNullOrWhiteSpace(defStr)) return new List<string>();
            return defStr.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private BranchTrendConfig? GetConfig(SchemeContext scheme)
        {
            if (scheme.DrawRuleConfig == null) return null;
            try
            {
                return JsonSerializer.Deserialize<BranchTrendConfig>(
                    JsonSerializer.Serialize(scheme.DrawRuleConfig),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { return null; }
        }
    }
}
