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
        
        // 定义内部状态类
        public class UserGameState
        {
            public bool IsRunning { get; set; }
            public bool IsSimulation { get; set; }
            public decimal Balance { get; set; }
            public decimal Profit { get; set; }
            public decimal Turnover { get; set; }
            public decimal SimProfit { get; set; }
            public decimal SimTurnover { get; set; }
        }

        private readonly ConcurrentDictionary<int, UserGameState> _userStates = new();

        // ========== 订单缓存 ==========
        // Key: GroupId, Value: 该群组下的待结算订单列表
        private readonly ConcurrentDictionary<long, List<BetOrder>> _activeOrders = new();

        /// <summary>
        /// 添加订单到缓存
        /// </summary>
        public void AddOrder(BetOrder order)
        {
            var groupOrders = _activeOrders.GetOrAdd(order.TgGroupId, _ => new List<BetOrder>());
            lock (groupOrders)
            {
                groupOrders.Add(order);
            }
        }

        /// <summary>
        /// 获取待结算订单（从缓存）
        /// </summary>
        public List<BetOrder> GetPendingOrders(long groupId, string issueNumber)
        {
            if (_activeOrders.TryGetValue(groupId, out var groupOrders))
            {
                lock (groupOrders)
                {
                    // 筛选符合期号且状态为待结算或已确认的订单
                    return groupOrders
                        .Where(o => o.IssueNumber == issueNumber && 
                                   (o.Status == 0 || o.Status == 4))
                        .ToList();
                }
            }
            return new List<BetOrder>();
        }

        /// <summary>
        /// 从缓存中移除订单
        /// </summary>
        public void RemoveOrder(long orderId, long groupId)
        {
            if (_activeOrders.TryGetValue(groupId, out var groupOrders))
            {
                lock (groupOrders)
                {
                    var order = groupOrders.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        groupOrders.Remove(order);
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定用户的状态对象（不存在则创建）
        /// </summary>
        private UserGameState GetUserState(int userId)
        {
            return _userStates.GetOrAdd(userId, _ => new UserGameState());
        }

        /// <summary>
        /// 获取/设置指定用户的运行状态
        /// </summary>
        public bool IsRunning
        {
            get => GetUserState(CurrentUserId).IsRunning;
            set => GetUserState(CurrentUserId).IsRunning = value;
        }

        /// <summary>
        /// 获取/设置指定用户的模拟模式状态
        /// </summary>
        public bool IsSimulation
        {
            get => GetUserState(CurrentUserId).IsSimulation;
            set => GetUserState(CurrentUserId).IsSimulation = value;
        }

        // ========== 按用户访问的方法 ==========

        public bool GetIsRunning(int userId) => GetUserState(userId).IsRunning;
        public void SetIsRunning(int userId, bool value) => GetUserState(userId).IsRunning = value;
        
        public bool GetIsSimulation(int userId) => GetUserState(userId).IsSimulation;
        public void SetIsSimulation(int userId, bool value) => GetUserState(userId).IsSimulation = value;

        // ========== 统计数据访问（按当前用户） ==========
        
        public decimal Balance 
        { 
            get => GetUserState(CurrentUserId).Balance;
            set => GetUserState(CurrentUserId).Balance = value;
        }
        
        public decimal Profit 
        { 
            get => GetUserState(CurrentUserId).Profit;
            set => GetUserState(CurrentUserId).Profit = value;
        }
        
        public decimal Turnover 
        { 
            get => GetUserState(CurrentUserId).Turnover;
            set => GetUserState(CurrentUserId).Turnover = value;
        }
        
        public decimal SimProfit 
        { 
            get => GetUserState(CurrentUserId).SimProfit;
            set => GetUserState(CurrentUserId).SimProfit = value;
        }
        
        public decimal SimTurnover 
        { 
            get => GetUserState(CurrentUserId).SimTurnover;
            set => GetUserState(CurrentUserId).SimTurnover = value;
        }

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
