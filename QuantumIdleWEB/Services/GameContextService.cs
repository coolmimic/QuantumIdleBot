using QuantumIdleWEB.GameCore;
using System.Collections.Concurrent;

namespace QuantumIdleWEB.Services
{
    /// <summary>
    /// 游戏上下文管理服务
    /// </summary>
    public class GameContextService
    {
        private readonly Dictionary<long, GroupGameContext> _contexts = new();
        // 按用户隔离日志，key = userId
        private readonly ConcurrentDictionary<int, List<string>> _userLogs = new();
        private readonly object _lockObj = new();

        // 当前操作的用户ID（用于日志记录）
        private static readonly AsyncLocal<int> _currentUserId = new();
        public static int CurrentUserId 
        { 
            get => _currentUserId.Value;
            set => _currentUserId.Value = value;
        }

        // ========== 按用户隔离的状态 ==========
        private readonly ConcurrentDictionary<int, bool> _userRunningState = new();
        private readonly ConcurrentDictionary<int, bool> _userSimulationState = new();

        /// <summary>
        /// 获取/设置指定用户的运行状态
        /// </summary>
        public bool IsRunning
        {
            get => _userRunningState.GetValueOrDefault(CurrentUserId, false);
            set => _userRunningState[CurrentUserId] = value;
        }

        /// <summary>
        /// 获取/设置指定用户的模拟模式状态
        /// </summary>
        public bool IsSimulation
        {
            get => _userSimulationState.GetValueOrDefault(CurrentUserId, false);
            set => _userSimulationState[CurrentUserId] = value;
        }

        /// <summary>
        /// 为指定用户ID获取运行状态（不依赖CurrentUserId）
        /// </summary>
        public bool GetIsRunning(int userId) => _userRunningState.GetValueOrDefault(userId, false);
        public void SetIsRunning(int userId, bool value) => _userRunningState[userId] = value;
        
        public bool GetIsSimulation(int userId) => _userSimulationState.GetValueOrDefault(userId, false);
        public void SetIsSimulation(int userId, bool value) => _userSimulationState[userId] = value;

        // 全局统计（向后兼容）
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
        /// 添加日志（使用当前用户ID）
        /// </summary>
        public void AddLog(string message)
        {
            AddLog(message, CurrentUserId);
        }

        /// <summary>
        /// 添加日志（指定用户ID）
        /// </summary>
        public void AddLog(string message, int userId)
        {
            var logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            
            if (userId > 0)
            {
                var userLogs = _userLogs.GetOrAdd(userId, _ => new List<string>());
                lock (userLogs)
                {
                    userLogs.Insert(0, logEntry);
                    if (userLogs.Count > 500)
                    {
                        userLogs.RemoveAt(userLogs.Count - 1);
                    }
                }
            }
            
            OnLog?.Invoke(logEntry);
        }

        /// <summary>
        /// 获取日志（指定用户ID）
        /// </summary>
        public List<string> GetLogs(int userId, int count = 100)
        {
            if (_userLogs.TryGetValue(userId, out var logs))
            {
                lock (logs)
                {
                    return logs.Take(count).ToList();
                }
            }
            return new List<string>();
        }

        /// <summary>
        /// 获取日志（使用当前用户ID，向后兼容）
        /// </summary>
        public List<string> GetLogs(int count = 100)
        {
            return GetLogs(CurrentUserId, count);
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
        /// 清除日志（指定用户ID）
        /// </summary>
        public void ClearLogs(int userId)
        {
            if (_userLogs.TryGetValue(userId, out var logs))
            {
                lock (logs)
                {
                    logs.Clear();
                }
            }
        }

        /// <summary>
        /// 清除日志（使用当前用户ID，向后兼容）
        /// </summary>
        public void ClearLogs()
        {
            ClearLogs(CurrentUserId);
        }
    }
}
