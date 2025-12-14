using QuantumIdleDesktop.GameCore;
using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Models.DrawRules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantumIdleDesktop.Strategies.DrawRules
{
    /// <summary>
    /// 01形态/走势策略实现 (支持多策略并发/优先级)
    /// </summary>
    public class PatternTrendRule : IDrawRule
    {
        public List<string> GetNextBet(SchemeModel scheme, GroupGameContext context)
        {
            // 1. 基础配置校验
            if (scheme.DrawRuleConfig is not PatternTrendRuleConfig config)
                return new List<string>();

            if (config.StrategyList == null || config.StrategyList.Count == 0)
                return new List<string>();

            var history = context.History;
            if (history == null || history.Count == 0) return new List<string>();

            // =========================================================
            // 逻辑 A: 优先处理正在进行的策略 (Run Active Strategy)
            // =========================================================
            // 查找当前是否有任意一个子策略正在运行
            // 假设同一时间只允许跑一个策略 (单线程逻辑)，找到第一个正在跑的即可
            var activeStrategy = config.StrategyList.FirstOrDefault(x => x.IsExecuting);

            if (activeStrategy != null)
            {
                return ProcessActiveStrategy(scheme, history, config, activeStrategy);
            }

            // =========================================================
            // 逻辑 B: 没有策略在跑，扫描所有策略看是否触发 (Scan Triggers)
            // =========================================================

            // 遍历所有策略
            foreach (var strategy in config.StrategyList)
            {
                if (string.IsNullOrEmpty(strategy.MonitorPattern)) continue;

                // 检查该策略是否匹配历史
                if (CheckMonitorPattern(scheme, history, config, strategy.MonitorPattern))
                {
                    // --- 触发成功！---

                    // 1. 激活该策略状态
                    strategy.IsExecuting = true;
                    strategy.CurrentStepIndex = 0;

                    // 2. 立即生成第一期的投注内容
                    if (!string.IsNullOrEmpty(strategy.BetPattern))
                    {
                        char firstCode = strategy.BetPattern[0];
                        var betContent = GenerateBetContent(firstCode, config);

                        // 指针移到下一步，标记第一步已完成
                        strategy.CurrentStepIndex++;

                        return betContent;
                    }
                    else
                    {
                        // 如果配置了监控但没配置投注内容，直接重置
                        ResetState(strategy);
                    }

                    // 既然已经触发了一个策略，通常本期就只执行这一个，直接返回
                    // (如果你希望同时触发多个策略并合并投注，逻辑会更复杂，这里假设互斥)
                    break;
                }
            }

            return new List<string>();
        }

        /// <summary>
        /// 处理正在运行的策略逻辑 (检查中奖、推进步骤)
        /// </summary>
        private List<string> ProcessActiveStrategy(SchemeModel scheme, List<LotteryRecord> history, PatternTrendRuleConfig config, TrendStrategyItem strategy)
        {
            // 1. 检查上一期是否中奖 (用于处理 StopOnWin)
            if (strategy.CurrentStepIndex > 0)
            {
                // 获取上一期我们要投的形态 (0或1)
                // 注意：CurrentStepIndex 已经指向了"下一期"，所以上一期是 Index - 1
                char lastBetCode = strategy.BetPattern[strategy.CurrentStepIndex - 1];

                // 获取上一期实际开出的形态 (History[0] 是最新开奖)
                char? lastResultCode = TranslateRecordToCode(scheme, history[0], config);

                // 判断是否中奖
                bool isWin = lastResultCode.HasValue && lastResultCode.Value == lastBetCode;

                if (isWin && strategy.StopOnWin)
                {
                    ResetState(strategy);
                    // 中奖停止，本期暂停，等待下期重新扫描
                    return new List<string>();
                }
            }

            // 2. 检查是否所有步骤都投完了
            if (strategy.CurrentStepIndex >= strategy.BetPattern.Length)
            {
                ResetState(strategy);
                // 投完结束
                return new List<string>();
            }

            // 3. 继续执行连投的下一步
            char codeToBet = strategy.BetPattern[strategy.CurrentStepIndex];
            var betContent = GenerateBetContent(codeToBet, config);

            // 指针后移
            strategy.CurrentStepIndex++;

            return betContent;
        }

        /// <summary>
        /// 检查历史记录是否匹配指定的 MonitorPattern
        /// </summary>
        private bool CheckMonitorPattern(SchemeModel scheme, List<LotteryRecord> history, PatternTrendRuleConfig config, string pattern)
        {
            int monitorLen = pattern.Length;
            if (history.Count < monitorLen) return false;

            // 构建历史形态字符串 (从旧到新)
            // History[0] 是最新，History[monitorLen-1] 是最远
            char[] historyPatternArr = new char[monitorLen];

            for (int i = 0; i < monitorLen; i++)
            {
                // 倒序读取：monitorLen - 1 - i
                var record = history[monitorLen - 1 - i];
                char? code = TranslateRecordToCode(scheme, record, config);

                if (code == null) return false; // 遇到无法识别的数据，匹配失败

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

        // --- 以下辅助方法逻辑不变，主要是参数传递 ---

        private char? TranslateRecordToCode(SchemeModel scheme, LotteryRecord record, PatternTrendRuleConfig config)
        {
            if (IsMatchDefinition(scheme, record, config.CodeZeroDefinition)) return '0';
            if (IsMatchDefinition(scheme, record, config.CodeOneDefinition)) return '1';
            return null;
        }

        private bool IsMatchDefinition(SchemeModel scheme, LotteryRecord record, string definition)
        {
            var targets = definition.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (scheme.PlayMode == GamePlayMode.Digital)
            {
                return targets.Contains(record.Result);
            }
            else
            {
                var attributes = GameStrategyFactory.GetStrategy(scheme.GameType).ParseResult(scheme, record.Result);
                return targets.Any(t => attributes.Contains(t));
            }
        }

        private List<string> GenerateBetContent(char code, PatternTrendRuleConfig config)
        {
            string defStr = (code == '0') ? config.CodeZeroDefinition : config.CodeOneDefinition;
            if (string.IsNullOrWhiteSpace(defStr)) return new List<string>();
            return defStr.Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}