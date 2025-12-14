using QuantumIdleWEB.GameCore;
using System.Text.Json;

namespace QuantumIdleWEB.Strategies.DrawRules
{
    /// <summary>
    /// 固定号码规则
    /// </summary>
    public class FixedNumberRule : IDrawRule
    {
        public List<string> GetNextBet(SchemeContext scheme, GroupGameContext context)
        {
            if (scheme.DrawRuleConfig == null) return new List<string>();
            
            try
            {
                var config = JsonSerializer.Deserialize<FixedNumberConfig>(
                    JsonSerializer.Serialize(scheme.DrawRuleConfig),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                return config?.BetNumbers ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
