using QuantumIdleWEB.GameCore;
using System.Text.Json;

namespace QuantumIdleWEB.Strategies.DrawRules
{
    /// <summary>
    /// 开某投某规则（跟随上期结果）
    /// </summary>
    public class FollowLastRule : IDrawRule
    {
        public List<string> GetNextBet(SchemeContext scheme, GroupGameContext context)
        {
            // 1. 检查历史数据
            if (context.History == null || context.History.Count == 0)
            {
                return new List<string>();
            }

            // 2. 获取配置
            var config = GetConfig(scheme);
            if (config == null || config.DrawRuleDic == null || config.DrawRuleDic.Count == 0)
            {
                return new List<string>();
            }

            // 3. 获取上期结果
            var lastRecord = context.History.First();
            if (lastRecord == null || string.IsNullOrEmpty(lastRecord.Result))
            {
                return new List<string>();
            }

            // 4. 解析上期结果为标签（大/小/单/双等）
            var normalizedResult = GameHelper.ParseResultToTags(lastRecord.Result, scheme.GameType, scheme.PlayMode);

            // 5. 匹配配置规则
            foreach (var kvp in config.DrawRuleDic)
            {
                string ruleKey = kvp.Key.Trim();
                
                // 分割触发条件
                var triggers = ruleKey.Split(new[] { ',', '，', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToArray();

                // 任意结果都触发
                if (triggers.Length == 0 || triggers.Contains("*"))
                {
                    return kvp.Value;
                }

                // 检查所有条件是否匹配
                bool isMatch = triggers.All(trigger => 
                    normalizedResult.Contains(trigger, StringComparer.OrdinalIgnoreCase));

                if (isMatch)
                {
                    return kvp.Value;
                }
            }

            return new List<string>();
        }

        private FollowLastConfig? GetConfig(SchemeContext scheme)
        {
            if (scheme.DrawRuleConfig == null) return null;

            try
            {
                return JsonSerializer.Deserialize<FollowLastConfig>(
                    JsonSerializer.Serialize(scheme.DrawRuleConfig),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return null;
            }
        }
    }
}
