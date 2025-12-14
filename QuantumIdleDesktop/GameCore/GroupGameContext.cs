using Microsoft.VisualBasic;
using QuantumIdleDesktop.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using TL;

namespace QuantumIdleDesktop.GameCore
{


    /// <summary>
    /// 群游戏上下文基类
    /// </summary>
    public abstract class GroupGameContext
    {

        public TelegramGroupModel GroupModel;


        public Dictionary<long, decimal> UserBalances { get; private set; } = new Dictionary<long, decimal>();

        public long GroupId { get; private set; }
        public Models.GameType GameType { get; private set; }

        // 当前销售期号
        public string CurrentIssue { get; protected set; }
        // 历史记录 (基类负责存)
        public List<LotteryRecord> History { get; private set; } = new List<LotteryRecord>();

        public List<string> SaleLst { get; set; } = new List<string>();


        // ===========================
        // 2. 构造函数
        // ===========================
        protected GroupGameContext(long groupId, Models.GameType gameType, TelegramGroupModel groupModel)
        {
            GroupId = groupId;
            GameType = gameType;
            GroupModel = groupModel;
        }

        // ==========================================
        // 1. 核心流程方法
        // ==========================================
        public GameMessageState ProcessMessage(string message)
        {
            // 1. 第一步：先调用子类/抽象方法，判断消息的大致类型
            GameMessageState msgType = GetMessageType(message);

            if (msgType == GameMessageState.Unknown) return GameMessageState.Unknown;

            // 2. 根据类型分发给具体的方法处理
            switch (msgType)
            {
                case GameMessageState.StartBetting:
                    return HandleStartBetting(message);
                case GameMessageState.LotteryResult:
                    return HandleLotteryResult(message);
                case GameMessageState.StopBetting:
                    // 封盘通常不需要复杂逻辑，直接返回状态即可，或者更新 UI
                    return GameMessageState.StopBetting;

                default:
                    return GameMessageState.Unknown;
            }
        }



        // --- 独立处理方法 ---

        /// <summary>
        /// 处理【开始销售】逻辑
        /// </summary>
        private GameMessageState HandleStartBetting(string message)
        {
            // 1. 提取期号
            string saleIssue = GetSaleIssue(message);

            if (string.IsNullOrEmpty(saleIssue)) return GameMessageState.Unknown;

            // 2. 检查 SaleLst 是否已存在 (防止机器人重复发送开始指令导致逻辑混乱)
            // 使用 Contains 检查比 Find 更直观
            if (SaleLst.Contains(saleIssue))
            {
                // 已经处理过这个开始指令了，忽略
                return GameMessageState.Unknown;
            }

            // 3. 确认为新的一期
            CurrentIssue = saleIssue; // 更新当前期号
            SaleLst.Add(saleIssue);   // 记录到已开盘列表

            // 4. 维护列表长度 (防止内存无限增长，保留最近50期即可)
            if (SaleLst.Count > 50)
            {
                SaleLst.RemoveAt(0); // 移除最旧的
            }

            return GameMessageState.StartBetting;
        }

        /// <summary>
        /// 处理【开奖结果】逻辑
        /// </summary>
        private GameMessageState HandleLotteryResult(string message)
        {
            // 1. 提取开奖数据
            LotteryRecord openData = GetLotteryData(message);

            if (openData == null || string.IsNullOrEmpty(openData.IssueNumber))
                return GameMessageState.Unknown;

            // 2. 检查 History 是否已存在 (防止重复采集同一期的结果)
            // 这里查找是否存在期号相同的记录
            var existingRecord = History.Find(x => x.IssueNumber == openData.IssueNumber);

            if (existingRecord != null)
            {
                // 如果记录已存在，通常说明重复抓取，直接忽略即可
                // (除非你需要修正结果，比如之前抓错了，这里可以做更新逻辑)
                return GameMessageState.Unknown;
            }

            // 3. 新的开奖记录 -> 入库
            openData.OpenTime = DateTime.Now; // 记录采集时间

            // 插入到队首 (最新的在最前面)
            History.Insert(0, openData);
          
            // 4. 维护历史记录长度
            if (History.Count > 50)
            {
                History.RemoveAt(History.Count - 1); // 移除最旧的
            }

            return GameMessageState.LotteryResult;
        }







        /// <summary>
        /// 简单判断消息类型 (Contains)
        /// </summary>
        protected abstract GameMessageState GetMessageType(string message);

        /// <summary>
        /// 提取销售期号 (Regex)
        /// </summary>
        protected abstract string GetSaleIssue(string message);

        /// <summary>
        /// 提取开奖数据 (Regex)
        /// </summary>
        protected abstract LotteryRecord GetLotteryData(string message);
        public abstract string FormatOrderBets(List<OrderModel> orders);

        public abstract (decimal Balance, bool IsSuccess, string ErrorMessage) ParseBotReply(string message);
    }
}
