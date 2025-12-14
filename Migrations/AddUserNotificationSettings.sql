-- Migration: Add TelegramChatId and notification settings to Users table
-- Run this SQL on the database to add new columns

-- Add TelegramChatId if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'TelegramChatId')
BEGIN
    ALTER TABLE [Users] ADD [TelegramChatId] BIGINT NOT NULL DEFAULT 0;
    PRINT 'Added TelegramChatId column';
END

-- Add PushOrders if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PushOrders')
BEGIN
    ALTER TABLE [Users] ADD [PushOrders] BIT NOT NULL DEFAULT 0;
    PRINT 'Added PushOrders column';
END

-- Add PushAlerts if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PushAlerts')
BEGIN
    ALTER TABLE [Users] ADD [PushAlerts] BIT NOT NULL DEFAULT 1;
    PRINT 'Added PushAlerts column';
END

PRINT 'Migration completed successfully';
