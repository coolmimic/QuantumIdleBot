using TL;
using WTelegram;
using QuantumIdleWEB.Data;
using QuantumIdleWEB.GameCore;
using Microsoft.EntityFrameworkCore;

namespace QuantumIdleWEB.Services
{
    /// <summary>
    /// Telegram 客户端服务（供移动版使用，由后端维护）
    /// 使用 WTelegram 的 Login() 方法进行分步登录
    /// </summary>
    public class TelegramClientService : IDisposable
    {
        private readonly ILogger<TelegramClientService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        
        // 每个用户的 Telegram 客户端实例
        private readonly Dictionary<int, ClientInfo> _clients = new();
        // 存储待登录的客户端（正在进行登录流程）
        private readonly Dictionary<int, PendingLogin> _pendingLogins = new();
        private readonly object _lock = new();

        // API 配置
        private const int API_ID = 22497382;
        private const string API_HASH = "80d3f2e981c6cbb490579c9aa5db6005";

        public TelegramClientService(
            ILogger<TelegramClientService> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 为指定用户初始化 Telegram 客户端并开始登录流程
        /// </summary>
        public async Task<LoginResult> InitializeClientAsync(int userId, string phoneNumber, string userName)
        {
            // 检查是否已登录
            lock (_lock)
            {
                if (_clients.ContainsKey(userId) && _clients[userId].Client?.User != null)
                {
                    _logger.LogInformation($"用户 {userId} 的 Telegram 客户端已存在且已登录");
                    return new LoginResult { Success = true, RequiresAuth = false };
                }
            }

            try
            {
                // 清理旧的客户端和待登录状态
                lock (_lock)
                {
                    if (_clients.ContainsKey(userId))
                    {
                        _clients[userId].Client?.Dispose();
                        _clients.Remove(userId);
                    }
                    _pendingLogins.Remove(userId);
                }

                // 获取 Session 文件路径，使用用户名作为文件夹名
                var sessionPath = GetSessionPath(userId, phoneNumber, userName);
                
                // 使用简化的构造函数（不需要 Config 回调）
                var client = new Client(API_ID, API_HASH, sessionPath);
                
                // 开始登录流程，使用手机号
                var cleanPhone = CleanPhoneNumber(phoneNumber);
                var whatNext = await client.Login(cleanPhone);
                
                // 检查登录结果
                if (client.User != null)
                {
                    // 登录成功（可能是通过 session 文件自动登录）
                    return await CompleteLogin(userId, client, phoneNumber, userName);
                }
                
                // 需要进一步认证
                lock (_lock)
                {
                    _pendingLogins[userId] = new PendingLogin
                    {
                        Client = client,
                        PhoneNumber = phoneNumber,
                        UserName = userName,
                        WhatNext = whatNext
                    };
                }
                
                var authType = MapWhatNextToAuthType(whatNext);
                var message = GetAuthMessage(whatNext);
                
                _logger.LogInformation($"用户 {userId} 需要输入: {whatNext}");
                
                return new LoginResult
                {
                    Success = false,
                    RequiresAuth = true,
                    AuthType = authType,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"用户 {userId} 的 Telegram 客户端初始化失败");
                lock (_lock)
                {
                    _pendingLogins.Remove(userId);
                }
                return new LoginResult { Success = false, RequiresAuth = false, Message = ex.Message };
            }
        }

        /// <summary>
        /// 提交验证码或密码（由前端提供）
        /// </summary>
        public async Task<LoginResult> SubmitAuthAsync(int userId, string code)
        {
            PendingLogin pending;
            
            lock (_lock)
            {
                if (!_pendingLogins.TryGetValue(userId, out pending) || pending.Client == null)
                {
                    return new LoginResult 
                    { 
                        Success = false, 
                        RequiresAuth = false, 
                        Message = "没有待处理的认证请求，请先调用 initialize" 
                    };
                }
            }

            try
            {
                _logger.LogInformation($"用户 {userId} 提交认证: {pending.WhatNext}");
                
                // 继续登录流程，提交用户输入的值
                var whatNext = await pending.Client.Login(code);
                
                // 检查是否登录成功
                if (pending.Client.User != null)
                {
                    return await CompleteLogin(userId, pending.Client, pending.PhoneNumber, pending.UserName);
                }
                
                // 还需要进一步认证（例如验证码后还需要密码）
                lock (_lock)
                {
                    pending.WhatNext = whatNext;
                }
                
                var authType = MapWhatNextToAuthType(whatNext);
                var message = GetAuthMessage(whatNext);
                
                _logger.LogInformation($"用户 {userId} 还需要输入: {whatNext}");
                
                return new LoginResult
                {
                    Success = false,
                    RequiresAuth = true,
                    AuthType = authType,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"用户 {userId} 提交认证信息失败: {ex.Message}");
                
                // 判断是否是验证码/密码错误（可以重试）
                if (ex.Message.Contains("PHONE_CODE_INVALID") || 
                    ex.Message.Contains("PASSWORD_HASH_INVALID") ||
                    ex.Message.Contains("PHONE_CODE_EXPIRED"))
                {
                    var authType = pending.WhatNext == "password" ? "password" : "verification_code";
                    var errorMsg = ex.Message.Contains("EXPIRED") ? "验证码已过期，请重新获取" : 
                                   ex.Message.Contains("PASSWORD") ? "密码错误，请重试" : "验证码错误，请重试";
                    
                    return new LoginResult
                    {
                        Success = false,
                        RequiresAuth = true,
                        AuthType = authType,
                        Message = errorMsg
                    };
                }
                
                // 其他错误，清除状态
                lock (_lock)
                {
                    _pendingLogins.Remove(userId);
                }
                return new LoginResult { Success = false, RequiresAuth = false, Message = ex.Message };
            }
        }

        /// <summary>
        /// 完成登录流程
        /// </summary>
        private async Task<LoginResult> CompleteLogin(int userId, Client client, string phoneNumber, string userName)
        {
            var updateManager = client.WithUpdateManager(OnUpdateReceived, GetUpdateStatePath(userId, userName));
            
            lock (_lock)
            {
                _clients[userId] = new ClientInfo
                {
                    Client = client,
                    UpdateManager = updateManager,
                    User = client.User,
                    PhoneNumber = phoneNumber
                };
                _pendingLogins.Remove(userId);
            }

            _logger.LogInformation($"用户 {userId} 的 Telegram 登录成功: @{client.User?.username ?? client.User?.first_name}");
            return new LoginResult { Success = true, RequiresAuth = false };
        }

        /// <summary>
        /// 将 WTelegram 的 whatNext 映射到前端的 authType
        /// </summary>
        private string MapWhatNextToAuthType(string whatNext)
        {
            return whatNext switch
            {
                "verification_code" => "verification_code",
                "password" => "password",
                "name" => "name",
                _ => whatNext
            };
        }

        /// <summary>
        /// 获取认证提示消息
        /// </summary>
        private string GetAuthMessage(string whatNext)
        {
            return whatNext switch
            {
                "verification_code" => "请输入 Telegram 发送给您的验证码",
                "password" => "请输入您的两步验证密码",
                "name" => "请输入您的姓名（用于注册新账户）",
                _ => $"请输入 {whatNext}"
            };
        }

        /// <summary>
        /// 获取 Session 文件路径
        /// 路径格式: TelegramSessions/[用户名]/[手机号].session
        /// </summary>
        private string GetSessionPath(int userId, string phoneNumber, string? userName = null)
        {
            var basePath = _configuration["Telegram:SessionBasePath"] ?? "TelegramSessions";
            
            // 如果没有提供用户名，使用 userId
            var dirName = userName ?? userId.ToString();
            var sessionDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath, dirName);
            
            if (!Directory.Exists(sessionDir))
            {
                Directory.CreateDirectory(sessionDir);
            }
            
            var cleanPhone = CleanPhoneNumber(phoneNumber).Replace("+", "");
            return Path.Combine(sessionDir, $"{cleanPhone}.session");
        }

        /// <summary>
        /// 获取更新状态文件路径
        /// </summary>
        private string GetUpdateStatePath(int userId, string? userName = null)
        {
            var basePath = _configuration["Telegram:SessionBasePath"] ?? "TelegramSessions";
            var dirName = userName ?? userId.ToString();
            var stateDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath, dirName);
            return Path.Combine(stateDir, "updates.state");
        }

        /// <summary>
        /// 获取指定用户已存在的 session 手机号列表（用于前端预填充）
        /// </summary>
        public List<string> GetExistingSessionPhoneNumbers(string userName)
        {
            var result = new List<string>();
            var basePath = _configuration["Telegram:SessionBasePath"] ?? "TelegramSessions";
            var sessionDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath, userName);
            
            if (Directory.Exists(sessionDir))
            {
                var sessionFiles = Directory.GetFiles(sessionDir, "*.session");
                foreach (var file in sessionFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    if (!string.IsNullOrEmpty(fileName) && fileName != "updates")
                    {
                        // 将纯数字转换为带 + 号的格式
                        result.Add("+" + fileName);
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// 清理手机号格式
        /// </summary>
        private string CleanPhoneNumber(string phoneNumber)
        {
            return phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        }

        /// <summary>
        /// 获取用户的 Telegram 客户端
        /// </summary>
        public Client? GetClient(int userId)
        {
            lock (_lock)
            {
                return _clients.TryGetValue(userId, out var info) ? info.Client : null;
            }
        }

        /// <summary>
        /// 检查用户是否已连接 Telegram
        /// </summary>
        public bool IsConnected(int userId)
        {
            lock (_lock)
            {
                if (_clients.TryGetValue(userId, out var info))
                {
                    return info.Client?.User != null;
                }
                return false;
            }
        }

        /// <summary>
        /// 发送消息到指定群组
        /// </summary>
        public async Task<int> SendMessageAsync(int userId, long groupId, string message)
        {
            var client = GetClient(userId);
            if (client == null || client.User == null)
            {
                throw new InvalidOperationException("Telegram 客户端未初始化");
            }

            try
            {
                // 获取群组信息
                var chats = await client.Messages_GetAllChats();
                InputPeer? peer = null;

                foreach (var chat in chats.chats.Values)
                {
                    if (chat is Channel channel && channel.ID == groupId)
                    {
                        peer = new InputPeerChannel(channel.ID, channel.access_hash);
                        break;
                    }
                    else if (chat is Chat group && group.ID == groupId)
                    {
                        peer = new InputPeerChat(group.ID);
                        break;
                    }
                }

                if (peer == null)
                {
                    throw new InvalidOperationException($"未找到群组 {groupId}");
                }

                var result = await client.SendMessageAsync(peer, message);
                return result.ID;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送消息失败: 用户 {userId}, 群组 {groupId}");
                throw;
            }
        }

        /// <summary>
        /// 获取用户的所有群组
        /// </summary>
        public async Task<List<TelegramGroupInfo>> GetChatsAsync(int userId)
        {
            var client = GetClient(userId);
            if (client == null || client.User == null)
            {
                throw new InvalidOperationException("Telegram 客户端未初始化");
            }

            try
            {
                var allChats = await client.Messages_GetAllChats();
                var result = new List<TelegramGroupInfo>();

                foreach (var chat in allChats.chats.Values)
                {
                    if (chat is Channel channel)
                    {
                        if (channel.ID == 0) continue;
                        bool isChannel = (channel.flags & Channel.Flags.megagroup) == 0;
                        result.Add(new TelegramGroupInfo
                        {
                            Id = channel.ID,
                            Name = channel.title,
                            IsChannel = isChannel
                        });
                    }
                    else if (chat is Chat group)
                    {
                        result.Add(new TelegramGroupInfo
                        {
                            Id = group.ID,
                            Name = group.title,
                            IsChannel = false
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取群组列表失败: 用户 {userId}");
                throw;
            }
        }

        /// <summary>
        /// 断开指定用户的连接
        /// </summary>
        public void Disconnect(int userId)
        {
            lock (_lock)
            {
                if (_clients.TryGetValue(userId, out var info))
                {
                    info.UpdateManager = null;
                    info.Client?.Dispose();
                    _clients.Remove(userId);
                    _logger.LogInformation($"用户 {userId} 的 Telegram 客户端已断开");
                }
                _pendingLogins.Remove(userId);
            }
        }

        private async Task OnUpdateReceived(Update update)
        {
            // 处理接收到的更新
            if (update is UpdateNewMessage messageUpdate && messageUpdate.message is TL.Message msg)
            {
                // 忽略自己发送的消息
                if (msg.flags.HasFlag(TL.Message.Flags.out_)) 
                    return;

                // 获取群组ID
                long groupId = 0;
                if (msg.peer_id is PeerChat peerChat)
                {
                    groupId = peerChat.chat_id;
                }
                else if (msg.peer_id is PeerChannel peerChannel)
                {
                    groupId = peerChannel.channel_id;
                }
                else
                {
                    return; // 只处理群/频道消息
                }

                var messageText = msg.message;
                if (string.IsNullOrEmpty(messageText)) return;

                // 获取服务
                var gameService = _serviceProvider.GetRequiredService<GameContextService>();
                
                // 检查挂机状态
                if (!gameService.IsRunning)
                {
                    return;
                }

                try
                {
                    // 查找该群对应的所有用户的启用方案
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    
                    // 获取所有在该群启用方案的用户（按用户分组）
                    var userSchemes = await dbContext.Schemes
                        .Where(s => s.TgGroupId == groupId && s.IsEnabled)
                        .GroupBy(s => s.UserId)
                        .ToListAsync();
                    
                    if (userSchemes.Count == 0)
                    {
                        return; // 没有任何用户配置方案
                    }

                    // 使用第一个方案获取游戏类型（同一个群游戏类型应该相同）
                    var firstScheme = userSchemes.First().First();
                    var context = gameService.GetOrCreateContext(groupId, firstScheme.GameType);
                    var messageState = context.ProcessMessage(messageText);

                    if (messageState == GameMessageState.Unknown)
                    {
                        return;
                    }

                    // 消息类型日志已移除

                    // 根据消息类型执行对应逻辑，为每个用户独立处理
                    switch (messageState)
                    {
                        case GameMessageState.StartBetting:
                            // 为每个用户触发投注
                            var bettingService = scope.ServiceProvider.GetRequiredService<BettingService>();
                            foreach (var userGroup in userSchemes)
                            {
                                var userId = userGroup.Key;
                                gameService.AddLog($"[{groupId}] 开始销售 期号: {context.CurrentIssue}", userId);
                                // 获取方案名用于日志
                                var schemeNames = string.Join(",", userGroup.Select(s => s.Name));
                                gameService.AddLog($"[{groupId}] 处理方案: {schemeNames}", userId);
                                await bettingService.ProcessBetting(groupId, context, userId);
                            }
                            break;

                        case GameMessageState.LotteryResult:
                            // 触发结算
                            var lastRecord = context.History.FirstOrDefault();
                            if (lastRecord != null)
                            {
                                var settlementService = scope.ServiceProvider.GetRequiredService<SettlementService>();
                                // 为每个用户触发结算
                                foreach (var userGroup in userSchemes)
                                {
                                    var userId = userGroup.Key;
                                    gameService.AddLog($"[{groupId}] 开奖结果 期号: {lastRecord.IssueNumber} 结果: {lastRecord.Result}", userId);
                                    await settlementService.ProcessSettlement(groupId, lastRecord.IssueNumber, lastRecord.Result, userId);
                                }
                            }
                            else
                            {
                                // 无法确定用户，记录到所有相关用户
                                foreach (var userGroup in userSchemes)
                                {
                                    gameService.AddLog($"[{groupId}] 开奖消息解析失败，无法获取期号和结果", userGroup.Key);
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"处理消息时出错: 群组 {groupId}");
                    // 错误日志记录到所有相关用户
                    foreach (var userGroup in userSchemes)
                    {
                        gameService.AddLog($"[错误] 处理消息失败: {ex.Message}", userGroup.Key);
                    }
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var clientInfo in _clients.Values)
                {
                    clientInfo.UpdateManager = null;
                    clientInfo.Client?.Dispose();
                }
                _clients.Clear();
                
                foreach (var pending in _pendingLogins.Values)
                {
                    pending.Client?.Dispose();
                }
                _pendingLogins.Clear();
            }
        }

        private class ClientInfo
        {
            public Client Client { get; set; }
            public UpdateManager? UpdateManager { get; set; }
            public User? User { get; set; }
            public string? PhoneNumber { get; set; }
        }

        private class PendingLogin
        {
            public Client Client { get; set; }
            public string PhoneNumber { get; set; }
            public string UserName { get; set; }
            public string WhatNext { get; set; }  // 当前需要的认证类型
        }
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public bool RequiresAuth { get; set; }
        public string? AuthType { get; set; } // "verification_code" 或 "password"
        public string? Message { get; set; }
    }

    public class TelegramGroupInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsChannel { get; set; }
    }
}
