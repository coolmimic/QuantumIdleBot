-- ================================================================
-- 量子挂机 完整数据库迁移脚本
-- 适用于 Ubuntu Docker 环境
-- 执行方式: 
--   docker exec -i quantumidle-db /opt/mssql-tools18/bin/sqlcmd \
--     -S localhost -U sa -P 'YourStrong@Passw0rd' -d QuantumIdleBot -C \
--     -i /path/to/this/script.sql
-- ================================================================

-- ========== 1. Users 表新增字段 ==========

-- TelegramChatId (服务机器人会话ID)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'TelegramChatId')
BEGIN
    ALTER TABLE [Users] ADD [TelegramChatId] BIGINT NOT NULL DEFAULT 0;
    PRINT 'Added Users.TelegramChatId';
END

-- PushOrders (是否推送注单)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PushOrders')
BEGIN
    ALTER TABLE [Users] ADD [PushOrders] BIT NOT NULL DEFAULT 0;
    PRINT 'Added Users.PushOrders';
END

-- PushAlerts (是否推送报警)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PushAlerts')
BEGIN
    ALTER TABLE [Users] ADD [PushAlerts] BIT NOT NULL DEFAULT 1;
    PRINT 'Added Users.PushAlerts';
END

-- Profit (实盘盈亏)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'Profit')
BEGIN
    ALTER TABLE [Users] ADD [Profit] DECIMAL(18,4) NOT NULL DEFAULT 0;
    PRINT 'Added Users.Profit';
END

-- Turnover (实盘流水)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'Turnover')
BEGIN
    ALTER TABLE [Users] ADD [Turnover] DECIMAL(18,4) NOT NULL DEFAULT 0;
    PRINT 'Added Users.Turnover';
END

-- SimProfit (模拟盈亏)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'SimProfit')
BEGIN
    ALTER TABLE [Users] ADD [SimProfit] DECIMAL(18,4) NOT NULL DEFAULT 0;
    PRINT 'Added Users.SimProfit';
END

-- SimTurnover (模拟流水)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'SimTurnover')
BEGIN
    ALTER TABLE [Users] ADD [SimTurnover] DECIMAL(18,4) NOT NULL DEFAULT 0;
    PRINT 'Added Users.SimTurnover';
END

-- ========== 2. Schemes 表新增字段 ==========

-- Profit (方案实盘盈亏)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Schemes]') AND name = 'Profit')
BEGIN
    ALTER TABLE [Schemes] ADD [Profit] DECIMAL(18,4) NOT NULL DEFAULT 0;
    PRINT 'Added Schemes.Profit';
END

-- Turnover (方案实盘流水)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Schemes]') AND name = 'Turnover')
BEGIN
    ALTER TABLE [Schemes] ADD [Turnover] DECIMAL(18,4) NOT NULL DEFAULT 0;
    PRINT 'Added Schemes.Turnover';
END

-- SimProfit (方案模拟盈亏)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Schemes]') AND name = 'SimProfit')
BEGIN
    ALTER TABLE [Schemes] ADD [SimProfit] DECIMAL(18,4) NOT NULL DEFAULT 0;
    PRINT 'Added Schemes.SimProfit';
END

-- SimTurnover (方案模拟流水)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Schemes]') AND name = 'SimTurnover')
BEGIN
    ALTER TABLE [Schemes] ADD [SimTurnover] DECIMAL(18,4) NOT NULL DEFAULT 0;
    PRINT 'Added Schemes.SimTurnover';
END

-- ========== 3. TelegramChats 新表 ==========

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TelegramChats')
BEGIN
    CREATE TABLE [TelegramChats] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] INT NOT NULL,
        [ChatId] BIGINT NOT NULL,
        [Name] NVARCHAR(200) NULL,
        [IsChannel] BIT NOT NULL DEFAULT 0,
        [UpdateTime] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_TelegramChats_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id])
    );
    PRINT 'Created TelegramChats table';
END

-- ========== 4. BetOrders 表新增字段 ==========

-- TgMsgId (Telegram 消息 ID)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BetOrders]') AND name = 'TgMsgId')
BEGIN
    ALTER TABLE [BetOrders] ADD [TgMsgId] INT NOT NULL DEFAULT 0;
    PRINT 'Added BetOrders.TgMsgId';
END

-- TgGroupId (Telegram 群组 ID)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BetOrders]') AND name = 'TgGroupId')
BEGIN
    ALTER TABLE [BetOrders] ADD [TgGroupId] BIGINT NOT NULL DEFAULT 0;
    PRINT 'Added BetOrders.TgGroupId';
END

-- ========== 完成 ==========

PRINT '======================================';
PRINT 'Migration completed successfully!';
PRINT '======================================';
