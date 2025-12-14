using QuantumIdleWEB.GameCore;
using System.Text.Json;

namespace QuantumIdleWEB.Strategies.DrawRules
{
    /// <summary>
    /// 结果跟随规则
    /// 逻辑：根据上期开奖结果(0或1)，触发对应的投注序列
    /// </summary>
    public class ResultFollowRule : IDrawRule
    {
        public List<string> GetNextBet(SchemeContext scheme, GroupGameContext context)
        {
            var config = GetConfig(scheme);
            if (config == null) return new List<string>();

            var history = context.History;
            if (history == null || history.Count == 0) return new List<string>();

            // =========================================================
            // 逻辑 A: 处于投注状态中 -> 处理输赢与追号
            // =========================================================
            if (config.IsBetting)
            {
                string? lastBetCodeStr = GetLastBetCode(config);
                if (string.IsNullOrEmpty(lastBetCodeStr))
                {
                    ResetState(config);
                    return new List<string>();
                }

                char lastBetCode = lastBetCodeStr[0];
                char? actualResultCode = TranslateRecordToCode(scheme, history[0], config);
                bool isWin = actualResultCode.HasValue && actualResultCode.Value == lastBetCode;

                if (isWin)
                {
                    if (config.StopOnWin)
                    {
                        ResetState(config);
                        // 继续向下执行监控逻辑
                    }
                    else
                    {
                        config.CurrentStepIndex++;
                    }
                }
                else
                {
                    config.CurrentStepIndex++;
                }

                // 如果还在投注状态，检查序列是否跑完
                if (config.IsBetting)
                {
                    string? activePattern = GetActivePattern(config);

                    if (string.IsNullOrEmpty(activePattern) || config.CurrentStepIndex >= activePattern.Length)
                    {
                        ResetState(config);
                    }
                    else
                    {
                        char nextCode = activePattern[config.CurrentStepIndex];
                        return GenerateBetContent(nextCode, config);
                    }
                }
            }

            // =========================================================
            // 逻辑 B: 监控状态
            // =========================================================
            if (!config.IsBetting)
            {
                char? lastResultCode = TranslateRecordToCode(scheme, history[0], config);

                if (lastResultCode.HasValue)
                {
                    if (lastResultCode.Value == '0' && !string.IsNullOrEmpty(config.SequenceOnZero))
                    {
                        StartSequence(config, ResultFollowState.FollowingZero);
                    }
                    else if (lastResultCode.Value == '1' && !string.IsNullOrEmpty(config.SequenceOnOne))
                    {
                        StartSequence(config, ResultFollowState.FollowingOne);
                    }
                }

                // 如果成功触发了新序列，返回第一口的号码
                if (config.IsBetting)
                {
                    string? activePattern = GetActivePattern(config);
                    if (!string.IsNullOrEmpty(activePattern) && activePattern.Length > 0)
                    {
                        char firstCode = activePattern[0];
                        return GenerateBetContent(firstCode, config);
                    }
                }
            }

            return new List<string>();
        }

        private void ResetState(ResultFollowConfig config)
        {
            config.IsBetting = false;
            config.CurrentState = ResultFollowState.None;
            config.CurrentStepIndex = 0;
        }

        private void StartSequence(ResultFollowConfig config, ResultFollowState state)
        {
            config.IsBetting = true;
            config.CurrentState = state;
            config.CurrentStepIndex = 0;
        }

        private string? GetActivePattern(ResultFollowConfig config)
        {
            if (config.CurrentState == ResultFollowState.FollowingZero) return config.SequenceOnZero;
            if (config.CurrentState == ResultFollowState.FollowingOne) return config.SequenceOnOne;
            return null;
        }

        private string? GetLastBetCode(ResultFollowConfig config)
        {
            string? pattern = GetActivePattern(config);
            if (string.IsNullOrEmpty(pattern) || config.CurrentStepIndex >= pattern.Length) return null;
            return pattern[config.CurrentStepIndex].ToString();
        }

        private char? TranslateRecordToCode(SchemeContext scheme, LotteryRecord record, ResultFollowConfig config)
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

        private List<string> GenerateBetContent(char code, ResultFollowConfig config)
        {
            string defStr = (code == '0') ? config.CodeZeroDefinition : config.CodeOneDefinition;
            if (string.IsNullOrWhiteSpace(defStr)) return new List<string>();
            return defStr.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private ResultFollowConfig? GetConfig(SchemeContext scheme)
        {
            if (scheme.DrawRuleConfig == null) return null;
            try
            {
                return JsonSerializer.Deserialize<ResultFollowConfig>(
                    JsonSerializer.Serialize(scheme.DrawRuleConfig),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { return null; }
        }
    }
}
