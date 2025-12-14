using QuantumIdleDesktop.GameCore;
using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Models.DrawRules;
using System;
using System.Collections.Generic;
using System.Text;
using TL;

namespace QuantumIdleDesktop.Strategies.DrawRules
{
    internal class FollowLastRule : IDrawRule
    {
        /// <summary>
        /// 获取下注内容 - 跟随上期策略
        /// </summary>
        public List<string> GetNextBet(SchemeModel scheme, GroupGameContext context)
        {
            // ---------------------------------------------------------
            // 1. 基础数据校验
            // ---------------------------------------------------------

            // 如果没有历史数据，无法判断“上期”是什么，直接跳过
            if (context.History == null || context.History.Count == 0)
            {
                return null;
            }

            // 获取配置对象，如果类型不对或配置为空，直接跳过
            var config = scheme.DrawRuleConfig as FollowLastDrawRuleConfig;
            if (config == null || config.DrawRuleDic == null || config.DrawRuleDic.Count == 0)
            {
                return null;
            }

            // ---------------------------------------------------------
            // 2. 获取并标准化上期结果
            // ---------------------------------------------------------

            // 获取最新一期的原始结果 (例如 "5" 或 "1,2,3")
            // 假设 context.History[0] 或 First() 是最新的一期
            string lastRawResult = context.History.First()?.Result;

            if (string.IsNullOrEmpty(lastRawResult))
            {
                return null;
            }

            // 【核心调用】使用工具类将原始结果转换为通用形态 (如 "大", "单", "14")
            // 这一步屏蔽了扫雷、快3、PC28 之间的规则差异
            List<string> normalizedResult = GameStrategyFactory.GetStrategy(scheme.GameType).ParseResult(scheme, lastRawResult);

            // ---------------------------------------------------------
            // 3. 匹配配置规则
            // ---------------------------------------------------------

            // 遍历字典查找匹配项
            // Key 可能是单个条件 "大"，也可能是组合条件 "大,单"
            foreach (var kvp in config.DrawRuleDic)
            {
                string ruleKey = kvp.Key.Trim();

                // 支持多种分隔符：逗号、中文逗号、空格、分号等
                string[] triggers = ruleKey.Split(new[] { ',', '，', ';', ' ' },
                    StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToArray();

                // 特殊情况：配置为空或"*", 表示任意结果都触发
                if (triggers.Length == 0 || triggers.Contains("*"))
                {
                    return kvp.Value;
                }

                // 【核心判断】配置中的“每一个”条件，都必须在上期结果标签中出现
                bool isMatch = triggers.All(trigger =>
                    normalizedResult.Contains(trigger, StringComparer.OrdinalIgnoreCase));

                if (isMatch)
                {
                    return kvp.Value;  // 找到第一个完全匹配的规则就返回
                }
            }

            // 如果没有匹配到任何规则，不下注
            return null;
        }

    }


}
