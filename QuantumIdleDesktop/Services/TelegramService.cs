using QuantumIdleDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TL;
using WTelegram;

namespace QuantumIdleDesktop.Services
{
    public class TelegramService : IDisposable
    {
        private Client _client;
        private UpdateManager _updateManager;



        private string _tempPhoneNumber;
        // --- 状态属性 ---

        /// <summary>
        /// 【新增】判断是否已经登录且连接成功。
        /// 只有当 Client 实例存在且 User 对象不为空时，才视为在线。
        /// </summary>
        public bool IsOnline => _client != null && _client.User != null;

        /// <summary>
        /// 【新增】获取当前登录的用户信息（如果未登录则为 null）
        /// </summary>
        public User CurrentUser => _client?.User;

        // --- 事件定义 ---

        /// <summary>
        /// 日志回调
        /// </summary>
        public event Action<string> OnLogMessage;

        /// <summary>
        /// 收到新消息的回调
        /// </summary>
        public event Action<Update> onNewMessage;

        /// <summary>
        /// 连接状态改变回调
        /// </summary>
        public event Action<ConnectionStatus> OnStatusChanged;

        /// <summary>
        /// 需要用户输入验证码或密码时的回调
        /// </summary>
        public event Func<string, Task<string>> OnLoginRequired;

        /// <summary>
        /// 登录成功回调
        /// </summary>
        public event Action<User> OnLoggedIn;

        public enum ConnectionStatus { Disconnected, Connecting, WaitingForAuth, Connected, Error }

        /// <summary>
        /// 构造函数
        /// </summary>
        public TelegramService(string phone)
        {
            _tempPhoneNumber = phone.Replace(" ", "")
                            .Replace("-", "")
                            .Replace("(", "")
                            .Replace(")", "");
            _client = new Client(Config);
        }

        /// <summary>
        /// WTelegramClient 的配置回调
        /// </summary>
        private string Config(string what)
        {
            try
            {
                switch (what)
                {
                    case "api_id": return "22497382";
                    case "api_hash": return "80d3f2e981c6cbb490579c9aa5db6005";
                    case "session_pathname":

                        // 1. 定义文件夹名称
                        string folderName = "phone";

                        if (!Directory.Exists(folderName))
                        {
                            Directory.CreateDirectory(folderName);
                        }

                        return Path.Combine(folderName, $"{_tempPhoneNumber}.session");
                    case "phone_number":return _tempPhoneNumber;
                    case "verification_code":
                    case "password":
                        // 通知 UI 状态变化
                        OnStatusChanged?.Invoke(ConnectionStatus.WaitingForAuth);

                        if (OnLoginRequired != null)
                        {
                            // 同步等待 Task 完成 (WTelegram 的 Config 必须同步返回)
                            return OnLoginRequired.Invoke(what).GetAwaiter().GetResult();
                        }
                        throw new InvalidOperationException($"登录需要验证 '{what}'，但未注册处理事件。");

                    default:
                        return null; // 使用默认值
                }
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"[Config Error] {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 异步登录方法
        /// </summary>
        public async Task LoginAsync()
        {
            // 1. 如果已经在线，直接返回
            if (IsOnline)
            {
                OnStatusChanged?.Invoke(ConnectionStatus.Connected);
                OnLoggedIn?.Invoke(CurrentUser);
                OnLogMessage?.Invoke($"[系统] 已登录: @{CurrentUser.username}");

         
                EnsureUpdateManager();
                return;
            }

            OnStatusChanged?.Invoke(ConnectionStatus.Connecting);
            OnLogMessage?.Invoke("[系统] 正在连接 Telegram API...");

            try
            {
       

                // 2. 执行登录逻辑 (如果 session 文件存在且有效，会自动跳过验证)
                User user = await _client.LoginUserIfNeeded();

                if (user != null)
                {
                    CacheData.GroupLst = await GetAllChats();
                    OnLogMessage?.Invoke($"[系统] 获取群组{CacheData.GroupLst.Count}个");

                    // 3. 登录成功处理
                    OnLoggedIn?.Invoke(user);
                    OnStatusChanged?.Invoke(ConnectionStatus.Connected);
                    OnLogMessage?.Invoke($"[系统] 登录成功: @{user.username} (ID: {user.id})");

                    // 4. 启动消息监听
                    EnsureUpdateManager();
                }
                else
                {
                    OnLogMessage?.Invoke("[系统] 登录流程未完成，可能在等待验证码...");
                }
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(ConnectionStatus.Error);
                OnLogMessage?.Invoke($"[登录失败] {ex.Message}");
            }
        }







        /// <summary>
        /// 【新增】注销/切换账号
        /// </summary>
        public void Logout()
        {
            // 1. 断开连接
            _client?.Dispose();
            _client = null;
            _updateManager = null;

            // 2. 删除 Session 文件 (关键：否则下次 new Client 又会自动登录旧账号)
            if (File.Exists("tg_session.session"))
            {
                File.Delete("tg_session.session");
            }

            // 3. 重新初始化 Client 以便下次登录
            _client = new Client(Config);

            OnLogMessage?.Invoke("[系统] 已注销，请重新登录");
            OnStatusChanged?.Invoke(ConnectionStatus.Disconnected);
        }

        /// <summary>
        /// 确保 UpdateManager 已启动
        /// </summary>
        private void EnsureUpdateManager()
        {
            if (_updateManager == null && _client != null)
            {
                _updateManager = _client.WithUpdateManager(OnSingleUpdateReceivedAsync, "tg_updates.state");
                OnLogMessage?.Invoke("[系统] 消息监听服务已启动");
            }
        }

        /// <summary>
        /// 处理接收到的单个 Update
        /// </summary>
        private Task OnSingleUpdateReceivedAsync(Update update)
        {
            // 过滤出新消息
            if (update is UpdateNewMessage messageUpdate && messageUpdate.message is TL.Message msg)
            {
                // 忽略自己发送的消息 (flags 包含 out_)
                if (msg.flags.HasFlag(TL.Message.Flags.out_)) return Task.CompletedTask;
                // 触发事件
                onNewMessage?.Invoke(update);

              
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 执行下注操作：发送指令并返回消息 ID
        /// </summary>
        /// <param name="betCommand">下注内容 (如: "大 100")</param>
        /// <param name="groupId">目标群组 ID (仅用于日志)</param>
        /// <param name="groupPeer">Telegram 通信对象</param>
        /// <returns>发送成功的消息 ID，失败返回 0</returns>
        public async Task<int> PlaceBetAsync(string betCommand, long groupId, InputPeer groupPeer)
        {
            if (!IsOnline)
            {
                OnLogMessage?.Invoke("[错误] 未连接，无法执行下注");
                return 0;
            }

            try
            {
                // 发送下注指令
                var sendResult = await _client.SendMessageAsync(groupPeer, betCommand);

                // 获取消息 ID (用于后续追踪机器人的回复引用)
                int messageId = sendResult.ID;

                //OnLogMessage?.Invoke($"[下注已发送] Group: {groupId}, ID: {messageId}, Cmd: {betCommand}");

                return messageId;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"[下注发送失败] {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 获取所有加入的群组和超级群
        /// </summary>
        public async Task<List<TelegramGroupModel>> GetAllChats()
        {

            var result = new List<TelegramGroupModel>(); // 存放优先群组

            // 2. 获取所有对话
            var allChats = await _client.Messages_GetAllChats();

            foreach (var chat in allChats.chats.Values)
            {
                TelegramGroupModel model = null;

                // 构建模型
                switch (chat)
                {
                    case Chat g: // 普通群
                        model = new TelegramGroupModel
                        {
                            Id = g.ID,
                            Name = g.title,
                            IsChannel = false
                        };
                        result.Add(model);
                        break;

                    case Channel c: // 频道/超级群
                        if (c.ID == 0) continue;
                        bool isChannel = (c.flags & Channel.Flags.megagroup) == 0;
                        var inputPeer = new InputPeerChannel(c.ID, c.access_hash);

                        model = new TelegramGroupModel
                        {
                            Id = c.ID,
                            Name = c.title,
                            IsChannel = isChannel,
                            Peer = inputPeer
                        };
                        result.Add(model);
                        break;
                }
            }
            return result;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _client?.Dispose();
            _client = null;
            _updateManager = null;
            OnStatusChanged?.Invoke(ConnectionStatus.Disconnected);
            OnLogMessage?.Invoke("[系统] 服务已停止");
        }
    }
}