using System.Text.Json;

namespace QuantumIdleWEB.Strategies.OddsRules
{
    /// <summary>
    /// 直线倍率规则
    /// </summary>
    public class LinearOddsRule : IOddsRule
    {
        private static readonly JsonSerializerOptions _jsonOptions = new() 
        { 
            PropertyNameCaseInsensitive = true 
        };

        public int GetNextMultiplier(OddsContext context)
        {
            var config = GetConfig(context);
            if (config == null || config.Sequence == null || config.Sequence.Count == 0) return 1;
            
            var index = config.CurrentIndex;
            if (index >= config.Sequence.Count) index = 0;
            
            return config.Sequence[index];
        }

        public void UpdateState(OddsContext context, bool isWin)
        {
            var config = GetConfig(context);
            if (config == null || config.Sequence == null || config.Sequence.Count == 0) return;
            
            // ProgressMode: 0=挂了加倍(输后递增), 1=中了加倍(赢后递增)
            bool shouldProgress = config.ProgressMode == 1 ? isWin : !isWin;
            
            if (shouldProgress)
            {
                config.CurrentIndex++;
                if (config.CurrentIndex >= config.Sequence.Count)
                {
                    config.CurrentIndex = 0;
                }
            }
            else
            {
                config.CurrentIndex = 0;
            }
        }

        private LinearOddsConfig? GetConfig(OddsContext context)
        {
            if (context.OddsConfig == null) return null;
            
            try
            {
                return JsonSerializer.Deserialize<LinearOddsConfig>(
                    JsonSerializer.Serialize(context.OddsConfig), _jsonOptions);
            }
            catch
            {
                return null;
            }
        }
    }

    public class LinearOddsConfig
    {
        // 不设置默认值，避免覆盖用户配置
        public List<int> Sequence { get; set; } = new();
        public int CurrentIndex { get; set; }
        
        /// <summary>
        /// 递进模式：0=挂了加倍(输后递增，默认)，1=中了加倍(赢后递增)
        /// </summary>
        public int ProgressMode { get; set; } = 0;
    }

    /// <summary>
    /// 倍率规则工厂
    /// </summary>
    public static class OddsRuleFactory
    {
        private static readonly LinearOddsRule _linearRule = new();

        public static IOddsRule GetRule(int type)
        {
            // 目前只有直线倍率
            return _linearRule;
        }
    }
}
