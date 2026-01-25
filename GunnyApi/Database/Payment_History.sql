-- =============================================
-- Script: Create Payment_History Table
-- Description: Lưu lịch sử giao dịch PayMongo
-- Date: 2026-01-25
-- =============================================

-- Drop table if exists (for development only - remove in production)
-- DROP TABLE IF EXISTS Payment_History;

CREATE TABLE Payment_History (
    -- Primary Key
    Id INT IDENTITY(1,1) PRIMARY KEY,
    
    -- User Information
    UserId INT NOT NULL,
    Username NVARCHAR(255) NOT NULL,
    
    -- Payment Information
    Amount DECIMAL(18, 2) NOT NULL, -- Số tiền (PHP)
    AmountInCentavos INT NOT NULL, -- Số tiền tính bằng centavos
    Currency NVARCHAR(10) NOT NULL DEFAULT 'PHP',
    
    -- PayMongo Information
    CheckoutSessionId NVARCHAR(255) NULL, -- cs_xxx từ PayMongo
    PaymentIntentId NVARCHAR(255) NULL, -- pi_xxx từ PayMongo  
    PaymentId NVARCHAR(255) NULL, -- pay_xxx từ PayMongo
    CheckoutUrl NVARCHAR(1000) NULL, -- URL để khách hàng thanh toán
    
    -- Reward Information
    TierId INT NULL, -- ID của payment tier
    MoneyReward INT NULL, -- Xu nhận được
    GoldReward INT NULL, -- Vàng nhận được
    GiftTokenReward INT NULL, -- Lễ kim nhận được
    
    -- Transaction Status
    Status NVARCHAR(50) NOT NULL DEFAULT 'pending', -- pending, paid, failed, expired, cancelled
    PaymentMethod NVARCHAR(50) NULL, -- gcash, card, qrph, etc.
    
    -- Description & Metadata
    Description NVARCHAR(500) NULL,
    ProductName NVARCHAR(255) NULL,
    Metadata NVARCHAR(MAX) NULL, -- JSON metadata từ PayMongo
    
    -- Event Information
    EventType NVARCHAR(100) NULL, -- checkout_session.payment.paid, etc.
    EventId NVARCHAR(255) NULL, -- evt_xxx từ webhook
    
    -- Error Information
    FailureCode NVARCHAR(100) NULL,
    FailureMessage NVARCHAR(1000) NULL,
    
    -- Reward Status
    IsRewardProcessed BIT NOT NULL DEFAULT 0, -- Đã xử lý reward chưa
    RewardProcessedAt DATETIME NULL, -- Thời gian xử lý reward
    RewardErrorMessage NVARCHAR(1000) NULL, -- Lỗi khi xử lý reward (nếu có)
    
    -- Timestamps
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    PaidAt DATETIME NULL, -- Thời gian thanh toán thành công
    ExpiresAt DATETIME NULL, -- Thời gian hết hạn checkout session
    
    -- Indexes
    INDEX IX_Payment_History_UserId (UserId),
    INDEX IX_Payment_History_Username (Username),
    INDEX IX_Payment_History_CheckoutSessionId (CheckoutSessionId),
    INDEX IX_Payment_History_PaymentId (PaymentId),
    INDEX IX_Payment_History_Status (Status),
    INDEX IX_Payment_History_CreatedAt (CreatedAt DESC),
    
    -- Constraints
    CONSTRAINT CK_Payment_History_Amount CHECK (Amount > 0),
    CONSTRAINT CK_Payment_History_Status CHECK (Status IN ('pending', 'paid', 'failed', 'expired', 'cancelled'))
);
GO

-- Add comments/descriptions (SQL Server Extended Properties)
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Lịch sử giao dịch PayMongo', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'Payment_History';
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'ID của user trong Mem_Account', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'Payment_History',
    @level2type = N'COLUMN', @level2name = N'UserId';
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'CheckoutSession ID từ PayMongo (cs_xxx)', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'Payment_History',
    @level2type = N'COLUMN', @level2name = N'CheckoutSessionId';
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Trạng thái: pending, paid, failed, expired, cancelled', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'Payment_History',
    @level2type = N'COLUMN', @level2name = N'Status';
GO

-- Sample query to view payment history
-- SELECT * FROM Payment_History WHERE UserId = 123 ORDER BY CreatedAt DESC;
-- SELECT * FROM Payment_History WHERE Status = 'paid' ORDER BY PaidAt DESC;
