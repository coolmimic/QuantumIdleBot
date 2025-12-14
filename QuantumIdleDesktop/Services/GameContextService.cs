using QuantumIdleDesktop.GameCore;
using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Strategies.DrawRules;
using QuantumIdleDesktop.Strategies.OddsRules;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace QuantumIdleDesktop.Services
{
    public class GameContextService
    {
        // 单例模式 (可选，如果你想全局只用一个实例)
        public static GameContextService Instance { get; } = new GameContextService();
        // 缓存所有的群上下文
        public readonly Dictionary<long, GroupGameContext> _groupContexts = new Dictionary<long, GroupGameContext>();
        // === 事件：通知 UI 更新日志或界面 ===
        // UI 窗体只需订阅这个事件即可收到消息
        public event Action<string> OnLog;
        public event Action<GameMessageState> OnStateChanged; // 例如: (groupId, "第123期开始")
        private GameContextService() { } // 私有构造函数(配合单例)
        public void ProcessGroupMessage(TL.Message msg, string message)
        {
            long groupId = msg.Peer.ID;
      

            // 1. 获取上下文 (逻辑保持不变)
            if (!_groupContexts.TryGetValue(groupId, out var context))
            {
                context = TryCreateContext(groupId);
                if (context != null)
                {
                    _groupContexts[groupId] = context;
                    OnLog?.Invoke($"[系统] 上下文初始化成功: {context.GroupModel.Name} ({context.GameType})");
                }
            }

            if (context == null) return;

            try
            {
                // 2. 【重构点】尝试处理注单确认回执
                // 如果该消息是对我们注单的回复，TryHandleBetConfirmation 会处理并返回 true
                bool isBetConfirmation = TryHandleBetConfirmation(msg, message, context);

                // 3. 如果不是注单回执，则执行常规的游戏逻辑 (如采集期号、识别开始停止指令等)
                if (!isBetConfirmation)
                {
                    var resultState = context.ProcessMessage(message);
                    HandleProcessResult(resultState, context);
                }
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"[处理消息异常] Group:{groupId} Msg:{message} Err:{ex.Message}");
            }
        }
        /// <summary>
        /// 尝试处理注单确认回执 (封装后的方法)
        /// </summary>
        /// <returns>如果是注单回复并已处理，返回 true；否则返回 false</returns>
        private bool TryHandleBetConfirmation(TL.Message msg, string messageContent, GroupGameContext context)
        {
            // 1. 检查是否是引用回复
            if (msg.reply_to is TL.MessageReplyHeader replyHeader)
            {
                int repliedMsgId = replyHeader.reply_to_msg_id; // 被回复的消息ID

                // 2. 在缓存中查找对应的“已发送”状态的订单
                // 注意：这里假设 OrderStatus.Confirmed 代表“已发送给TG，等待机器人确认”的状态
                var targetOrders = CacheData.Orders.FindAll(item =>
                    item.TgMsgId == repliedMsgId &&
                    item.Status == OrderStatus.Confirmed
                );

                // 如果找到了对应的注单，说明这条消息是机器人给我们的回执
                if (targetOrders.Count > 0)
                {
                    // 3. 解析回执内容 (使用之前优化的 ParseBotReply 方法)
                    var parseResult = context.ParseBotReply(messageContent);

                    if (parseResult.IsSuccess)
                    {
                        // --- 下注成功处理 ---

                        // 更新全局余额
                        AppGlobal.Balance = parseResult.Balance;

                        foreach (var order in targetOrders)
                        {
                            // 状态流转：已确认 -> 待结算
                            order.Status = OrderStatus.PendingSettlement;

                            // 可选：在这里记录日志
                            OnLog?.Invoke($"[实盘]下注成功:{order.SchemeName} 内容:{order.BetContent} 金额{order.Amount} 余额:{parseResult.Balance}");
                        }

                    }
                    else
                    {
                        // --- 下注失败处理 ---
                        foreach (var order in targetOrders)
                        {
                            // 状态流转：已确认 -> 下注失败
                            order.Status = OrderStatus.BetFailed;
                            order.Remark = parseResult.ErrorMessage; // 记录失败原因 (如: 余额不足/封盘)
                            OnLog?.Invoke($"[下注失败] 方案:{order.SchemeName} 原因:{parseResult.ErrorMessage}");
                        }
                    }

                    // 返回 true，表示这是一条注单回执，已经被处理了，不需要再走后续逻辑
                    return true;
                }
            }

            // 不是引用消息，或者引用ID对不上我们的注单，返回 false
            return false;
        }
        /// <summary>
        /// 工厂方法：仅基于本地缓存数据尝试创建上下文
        /// </summary>
        private GroupGameContext TryCreateContext(long groupId)
        {
            // --- 步骤 A: 检查群组缓存是否存在 ---
            if (CacheData.GroupLst == null || CacheData.GroupLst.Count == 0)
            {
                // 还没拉取过群组列表，无法判断，直接放弃
                return null;
            }

            // --- 步骤 B: 检查该群是否在我们的群组列表中 ---
            // 这里直接在内存 List 中查找对象
            var targetGroup = CacheData.GroupLst.Find(item => item.Id == groupId);

            if (targetGroup == null)
            {
                // 收到了消息，但在我们的群组缓存列表里找不到这个群ID (可能是新加的群但还没刷新列表)
                return null;
            }

            // --- 步骤 C: 检查是否配置了方案 ---
            var scheme = CacheData.Schemes.Find(item => item.TgGroupId == groupId && item.IsEnabled);
            if (scheme == null)
            {
                // 有这个群，但是没配置方案，不处理
                return null;
            }

            // --- 步骤 D: 创建上下文 ---
            switch (scheme.GameType)
            {
                case GameType.Minesweeper:
                    // 将找到的 targetGroup 传入，以便后续发消息使用
                    return new MinesweeperContext(groupId, scheme.GameType, targetGroup);

                case GameType.Kuai3:

                    return new KuaiSanContext(groupId, scheme.GameType, targetGroup);

                case GameType.HashLottery:

                    return new HashLotteryContext(groupId, scheme.GameType, targetGroup);

                default:
                    return null;
            }
        }
        /// <summary>
        /// 处理结果分发
        /// </summary>
        private void HandleProcessResult(GameMessageState state, GroupGameContext context)
        {
            switch (state)
            {
                case GameMessageState.StartBetting:
                    string log1 = $"[{context.GroupModel.Name}] 第 {context.CurrentIssue} 期 开始销售";
                    OnLog?.Invoke(log1); // 通知 UI 写日志

                    BettingService.Instance.ProcessBetting(context.GroupId, context);

                    break;

                case GameMessageState.LotteryResult:
                    // 这里的 CurrentIssue 还是上一期的，或者你需要从 History 取最新
                    var lastRecord = context.History.Count > 0 ? context.History[0] : null;
                    if (lastRecord != null)
                    {
                        string log2 = $"[开奖] 群{context.GroupModel.Name} 第 {lastRecord.IssueNumber} 期 开奖: {lastRecord.Result}";
                        OnLog?.Invoke(log2);
                        SettlementService.Instance.ProcessSettlement(context.GroupId, lastRecord.IssueNumber, lastRecord.Result);

                        if (context.UserBalances.Count > 0)
                        {
                            if (context.UserBalances.Keys.Contains(CacheData.tgService.CurrentUser.id))
                            {
                                AppGlobal.Balance = context.UserBalances[CacheData.tgService.CurrentUser.id];
                            }
                        }
                    }
                    break;
            }
        }

    }
}
