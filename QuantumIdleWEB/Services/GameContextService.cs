using QuantumIdleWEB.GameCore;

namespace QuantumIdleWEB.Services
{
    /// <summary>
    /// 游戏上下文管理服务
    /// </summary>
    public class GameContextService
    {
        private readonly Dictionary<long, GroupGameContext> _contexts = new();
        private readonly List<string> _logs = new();
        private readonly object _lockObj = new();

        // 全局状态
        public bool IsRunning { get; set; } = false;
        public bool IsSimulation { get; set; } = false;
        public decimal Balance { get; set; } = 0;
        public decimal Profit { get; set; } = 0;
        public decimal Turnover { get; set; } = 0;
        public decimal SimProfit { get; set; } = 0;
        public decimal SimTurnover { get; set; } = 0;

        public event Action<string>? OnLog;

        /// <summary>
        /// 获取或创建游戏上下文
        /// </summary>
        public GroupGameContext GetOrCreateContext(long groupId, int gameType)
        {
            lock (_lockObj)
            {
                if (_contexts.TryGetValue(groupId, out var context))
                {
                    return context;
                }

                // 根据游戏类型创建不同的上下文
                GroupGameContext newContext = gameType switch
                {
                    0 => new MinesweeperContext(groupId),    // 扫雷
                    4 => new HashLotteryContext(groupId),    // 哈希彩
                    _ => new MinesweeperContext(groupId)     // 默认
                };

                _contexts[groupId] = newContext;
                return newContext;
            }
        }

        /// <summary>
        /// 处理群消息
        /// </summary>
        public GameMessageState ProcessGroupMessage(long groupId, int gameType, string message)
        {
            if (!IsRunning) return GameMessageState.Unknown;

            var context = GetOrCreateContext(groupId, gameType);
            var result = context.ProcessMessage(message);

            if (result != GameMessageState.Unknown)
            {
                // 消息类型日志已移除
            }

            return result;
        }

        /// <summary>
        /// 添加日志
        /// </summary>
        public void AddLog(string message)
        {
            var logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            lock (_logs)
            {
                _logs.Insert(0, logEntry);
                if (_logs.Count > 500)
                {
                    _logs.RemoveAt(_logs.Count - 1);
                }
            }
            OnLog?.Invoke(logEntry);
        }

        /// <summary>
        /// 获取日志
        /// </summary>
        public List<string> GetLogs(int count = 100)
        {
            lock (_logs)
            {
                return _logs.Take(count).ToList();
            }
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        public object GetStatus()
        {
            return new
            {
                isRunning = IsRunning,
                isSimulation = IsSimulation,
                balance = Balance,
                profit = Profit,
                turnover = Turnover,
                simProfit = SimProfit,
                simTurnover = SimTurnover
            };
        }

        /// <summary>
        /// 清除日志
        /// </summary>
        public void ClearLogs()
        {
            lock (_logs)
            {
                _logs.Clear();
            }
        }
    }
}
