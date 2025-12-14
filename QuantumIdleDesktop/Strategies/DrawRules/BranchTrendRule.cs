using QuantumIdleDesktop.GameCore;
using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Models.DrawRules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantumIdleDesktop.Strategies.DrawRules
{
    /// <summary>
    /// 分支走势策略实现 (BranchTrend)
    /// 逻辑：监控 -> 首投 -> 根据结果进入 赢/输 分支序列
    /// </summary>
    public class BranchTrendRule : IDrawRule
    {
        public List<string> GetNextBet(SchemeModel scheme, GroupGameContext context)
        {
            // 1. 配置校验
            if (scheme.DrawRuleConfig is not BranchTrendRuleConfig config)
                return new List<string>();

            if (string.IsNullOrEmpty(config.MonitorPattern) || string.IsNullOrEmpty(config.InitialBet))
                return new List<string>();

            var history = context.History;
            if (history == null || history.Count == 0) return new List<string>();

            // =========================================================
            // 逻辑 A: 处于投注状态中 (IsBetting = true)
            // =========================================================
            if (config.IsBetting)
            {
                // 1. 获取上一期我们投了什么 (用于判定胜负)
                // 注意：history[0] 是刚刚开出的结果，我们需要对比 上一次的下注内容 和 history[0]
                string lastBetCodeStr = GetLastBetCode(config);

                // 异常保护：如果状态错乱找不到上一期的下注代码，重置
                if (string.IsNullOrEmpty(lastBetCodeStr))
                {
                    ResetState(config);
                    return new List<string>();
                }

                char lastBetCode = lastBetCodeStr[0];
                char? actualResultCode = TranslateRecordToCode(scheme, history[0], config);

                // 2. 判定胜负
                bool isWin = actualResultCode.HasValue && actualResultCode.Value == lastBetCode;

                // 3. 全局风控：中奖即停 (StopOnWin)
                // 无论是在首投阶段，还是在序列阶段，只要勾选了中奖即停且中奖了，立即重置
                if (isWin && config.StopOnWin)
                {
                    ResetState(config);
                    return new List<string>(); // 本期休息，下期重新监控
                }

                // 4. 状态流转 (根据当前处于哪个阶段决定下一步去哪)
                if (config.CurrentState == BranchBettingState.Initial)
                {
                    // === 刚刚结束的是【首投】 ===

                    // 根据胜负决定进入哪个分支
                    if (isWin)
                    {
                        config.CurrentState = BranchBettingState.WinSequence;
                        config.CurrentStepIndex = 0; // 新序列从头开始
                    }
                    else
                    {
                        config.CurrentState = BranchBettingState.LossSequence;
                        config.CurrentStepIndex = 0; // 新序列从头开始
                    }
                }
                else
                {
                    // === 刚刚结束的是【分支序列】中的一步 ===

                    // 只是简单的移动到下一步
                    config.CurrentStepIndex++;
                }

                // 5. 执行新状态下的投注
                string activePattern = GetActivePattern(config);

                // 检查序列是否为空，或者是否已经跑完了
                if (string.IsNullOrEmpty(activePattern) || config.CurrentStepIndex >= activePattern.Length)
                {
                    // 序列执行完毕，重置回监控状态
                    ResetState(config);
                    // 可以在这里选择 return new List<string>() 休息一期，或者直接 fall through 到监控逻辑
                    return new List<string>();
                }

                // 获取当前步骤的代码 (0或1)
                char nextCode = activePattern[config.CurrentStepIndex];
                return GenerateBetContent(nextCode, config);
            }

            // =========================================================
            // 逻辑 B: 监控状态 (IsBetting = false)
            // =========================================================

            int monitorLen = config.MonitorPattern.Length;
            if (history.Count < monitorLen) return new List<string>();

            // 1. 构建历史形态字符串
            char[] historyArr = new char[monitorLen];
            bool isValid = true;

            for (int i = 0; i < monitorLen; i++)
            {
                // 倒序读取：monitorLen-1-i 是历史记录索引
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

            // 2. 比对
            if (historyStr == config.MonitorPattern)
            {
                // 触发！进入首投状态
                config.IsBetting = true;
                config.CurrentState = BranchBettingState.Initial;
                config.CurrentStepIndex = 0; // 虽然Initial不依赖Index，但保持数据整洁

                // 生成首投内容
                if (!string.IsNullOrEmpty(config.InitialBet))
                {
                    return GenerateBetContent(config.InitialBet[0], config);
                }
            }

            return new List<string>();
        }

        // =========================================================
        // 辅助方法
        // =========================================================

        private void ResetState(BranchTrendRuleConfig config)
        {
            config.IsBetting = false;
            config.CurrentState = BranchBettingState.None;
            config.CurrentStepIndex = 0;
        }

        /// <summary>
        /// 获取当前状态下应该执行的形态字符串 (WinPattern 或 LossPattern)
        /// </summary>
        private string GetActivePattern(BranchTrendRuleConfig config)
        {
            if (config.CurrentState == BranchBettingState.WinSequence) return config.WinPattern;
            if (config.CurrentState == BranchBettingState.LossSequence) return config.LossPattern;
            return null;
        }

        /// <summary>
        /// 回溯获取“上一手”我们投了什么代码，用于判定输赢
        /// </summary>
        private string GetLastBetCode(BranchTrendRuleConfig config)
        {
            // 如果当前状态是 Initial，说明上一手就是 InitialBet (因为状态是在判定完胜负后才切换的，但这里我们是在判定前)
            // 等等，逻辑顺序是：
            // 1. IsBetting = true
            // 2. 检查 History[0] (最新开奖) vs "我们上一次投的"
            // 3. 这里的 "上一次" 对应的是 CurrentState 和 CurrentStepIndex 指向的内容
            //    因为我们投完后，Index 没有动，直到现在 Check 完才会动。

            if (config.CurrentState == BranchBettingState.Initial)
            {
                return config.InitialBet;
            }
            else if (config.CurrentState == BranchBettingState.WinSequence || config.CurrentState == BranchBettingState.LossSequence)
            {
                string pattern = GetActivePattern(config);
                if (string.IsNullOrEmpty(pattern) || config.CurrentStepIndex >= pattern.Length) return null;
                return pattern[config.CurrentStepIndex].ToString();
            }

            return null;
        }

        /// <summary>
        /// 翻译开奖结果为 0 或 1
        /// </summary>
        private char? TranslateRecordToCode(SchemeModel scheme, LotteryRecord record, BranchTrendRuleConfig config)
        {
            if (IsMatchDefinition(scheme, record, config.CodeZeroDefinition)) return '0';
            if (IsMatchDefinition(scheme, record, config.CodeOneDefinition)) return '1';
            return null;
        }

        /// <summary>
        /// 匹配定义 (支持 "大,单" 这种多值定义)
        /// </summary>
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

        /// <summary>
        /// 生成具体的下注号码列表
        /// </summary>
        private List<string> GenerateBetContent(char code, BranchTrendRuleConfig config)
        {
            string defStr = (code == '0') ? config.CodeZeroDefinition : config.CodeOneDefinition;
            if (string.IsNullOrWhiteSpace(defStr)) return new List<string>();
            return defStr.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}