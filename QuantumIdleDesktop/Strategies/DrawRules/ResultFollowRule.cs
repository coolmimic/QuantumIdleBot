using QuantumIdleDesktop.GameCore;
using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Models.DrawRules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantumIdleDesktop.Strategies.DrawRules
{
    /// <summary>
    /// 结果跟随策略实现 (ResultFollow)
    /// 逻辑：
    /// 1. 监控上期开奖结果 (0或1)
    /// 2. 触发对应的投注序列 (SequenceOnZero / SequenceOnOne)
    /// 3. 中奖即停(重置)，挂了继续追
    /// </summary>
    public class ResultFollowRule : IDrawRule
    {
        public List<string> GetNextBet(SchemeModel scheme, GroupGameContext context)
        {
            // 1. 配置校验
            if (scheme.DrawRuleConfig is not ResultFollowRuleConfig config)
                return new List<string>();

            var history = context.History;
            if (history == null || history.Count == 0) return new List<string>();

            // =========================================================
            // 逻辑 A: 处于投注状态中 (IsBetting = true) -> 处理输赢与追号
            // =========================================================
            if (config.IsBetting)
            {
                // 1. 获取上一手我们投了什么 (用于判定胜负)
                string lastBetCodeStr = GetLastBetCode(config);

                // 异常保护：状态错乱
                if (string.IsNullOrEmpty(lastBetCodeStr))
                {
                    ResetState(config);
                    return new List<string>();
                }

                char lastBetCode = lastBetCodeStr[0];
                char? actualResultCode = TranslateRecordToCode(scheme, history[0], config);

                // 2. 判定胜负 (如果开奖结果无法解析，视为不中)
                bool isWin = actualResultCode.HasValue && actualResultCode.Value == lastBetCode;

                // 3. 逻辑分流
                if (isWin)
                {
                    // === 中奖 ===
                    if (config.StopOnWin)
                    {
                        // 策略要求：中了之后就继续重新检测
                        ResetState(config);
                        // 此时已重置为监控状态。
                        // 这里有两个选择：
                        // A. 直接 return，本期休息，下期再看
                        // B. 立即进入下方的监控逻辑 (Goto Logic B)，根据刚开出的结果马上开启新一轮
                        // 根据通常的“高频”需求，中奖后通常会立即根据最新结果开启新一轮，因此我们不 return，继续向下执行 Logic B
                    }
                    else
                    {
                        // 中奖不停止，继续跑完序列
                        config.CurrentStepIndex++;
                    }
                }
                else
                {
                    // === 没中 (挂了) ===
                    // 继续序列的下一步
                    config.CurrentStepIndex++;
                }

                // 4. 如果还在投注状态，检查序列是否跑完
                if (config.IsBetting)
                {
                    string activePattern = GetActivePattern(config);

                    // 序列跑完了?
                    if (string.IsNullOrEmpty(activePattern) || config.CurrentStepIndex >= activePattern.Length)
                    {
                        ResetState(config);
                        // 序列结束，同样继续向下执行 Logic B，进行新一轮监控
                    }
                    else
                    {
                        // 序列没跑完，返回当前步骤的号码
                        char nextCode = activePattern[config.CurrentStepIndex];
                        return GenerateBetContent(nextCode, config);
                    }
                }
            }

            // =========================================================
            // 逻辑 B: 监控状态 (IsBetting = false)
            // =========================================================

            // 只有当非投注状态时，才根据上期结果决定开启什么新序列
            if (!config.IsBetting)
            {
                // 解析上期结果 (history[0] 是最新的)
                char? lastResultCode = TranslateRecordToCode(scheme, history[0], config);

                if (lastResultCode.HasValue)
                {
                    // 命中 "0" (如开大) -> 触发 SequenceOnZero
                    if (lastResultCode.Value == '0' && !string.IsNullOrEmpty(config.SequenceOnZero))
                    {
                        StartSequence(config, ResultFollowState.FollowingZero);
                    }
                    // 命中 "1" (如开小) -> 触发 SequenceOnOne
                    else if (lastResultCode.Value == '1' && !string.IsNullOrEmpty(config.SequenceOnOne))
                    {
                        StartSequence(config, ResultFollowState.FollowingOne);
                    }
                }

                // 如果成功触发了新序列，返回第一口的号码
                if (config.IsBetting)
                {
                    string activePattern = GetActivePattern(config);
                    if (!string.IsNullOrEmpty(activePattern) && activePattern.Length > 0)
                    {
                        char firstCode = activePattern[0];
                        return GenerateBetContent(firstCode, config);
                    }
                }
            }

            return new List<string>();
        }

        // =========================================================
        // 辅助方法
        // =========================================================

        private void ResetState(ResultFollowRuleConfig config)
        {
            config.IsBetting = false;
            config.CurrentState = ResultFollowState.None;
            config.CurrentStepIndex = 0;
        }

        private void StartSequence(ResultFollowRuleConfig config, ResultFollowState state)
        {
            config.IsBetting = true;
            config.CurrentState = state;
            config.CurrentStepIndex = 0;
        }

        private string GetActivePattern(ResultFollowRuleConfig config)
        {
            if (config.CurrentState == ResultFollowState.FollowingZero) return config.SequenceOnZero;
            if (config.CurrentState == ResultFollowState.FollowingOne) return config.SequenceOnOne;
            return null;
        }

        private string GetLastBetCode(ResultFollowRuleConfig config)
        {
            string pattern = GetActivePattern(config);
            if (string.IsNullOrEmpty(pattern) || config.CurrentStepIndex >= pattern.Length) return null;
            return pattern[config.CurrentStepIndex].ToString();
        }

        /// <summary>
        /// 翻译开奖结果为 0 或 1
        /// </summary>
        private char? TranslateRecordToCode(SchemeModel scheme, LotteryRecord record, ResultFollowRuleConfig config)
        {
            if (IsMatchDefinition(scheme, record, config.CodeZeroDefinition)) return '0';
            if (IsMatchDefinition(scheme, record, config.CodeOneDefinition)) return '1';
            return null;
        }

        private bool IsMatchDefinition(SchemeModel scheme, LotteryRecord record, string definition)
        {
            if (string.IsNullOrWhiteSpace(definition)) return false;
            var targets = definition.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (scheme.PlayMode == GamePlayMode.Digital)
            {
                return targets.Contains(record.Result);
            }
            else
            {
                var attrs = GameStrategyFactory.GetStrategy(scheme.GameType).ParseResult(scheme, record.Result);
                return targets.Any(t => attrs.Contains(t));
            }
        }

        private List<string> GenerateBetContent(char code, ResultFollowRuleConfig config)
        {
            string defStr = (code == '0') ? config.CodeZeroDefinition : config.CodeOneDefinition;
            if (string.IsNullOrWhiteSpace(defStr)) return new List<string>();
            return defStr.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}