-- =============================================
-- Email Change OTP Management
-- Quản lý OTP cho việc thay đổi email
-- =============================================

USE [Db_Member1]
GO

-- =============================================
-- Table: EmailChangeOTPs
-- Lưu trữ OTP cho quá trình thay đổi email
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmailChangeOTPs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EmailChangeOTPs] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] INT NOT NULL,
        [CurrentEmail] NVARCHAR(100) NOT NULL,
        [NewEmail] NVARCHAR(100) NOT NULL,
        [Step] TINYINT NOT NULL, -- 1: Verify Old Email, 2: Verify New Email
        [OtpCode] NVARCHAR(6) NOT NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
        [ExpiresAt] DATETIME NOT NULL,
        [IsUsed] BIT NOT NULL DEFAULT 0,
        [UsedAt] DATETIME NULL,
        [AttemptCount] INT NOT NULL DEFAULT 0,
        [MaxAttempts] INT NOT NULL DEFAULT 5,
        [IsBlocked] BIT NOT NULL DEFAULT 0,
        [BlockedUntil] DATETIME NULL,
        
        -- Indexes for performance
        INDEX IX_EmailChangeOTPs_UserId_Step NONCLUSTERED (UserId, Step),
        INDEX IX_EmailChangeOTPs_NewEmail NONCLUSTERED (NewEmail),
        INDEX IX_EmailChangeOTPs_ExpiresAt NONCLUSTERED (ExpiresAt)
    )
    
    PRINT 'Table EmailChangeOTPs created successfully'
END
ELSE
BEGIN
    PRINT 'Table EmailChangeOTPs already exists'
END
GO

-- =============================================
-- Stored Procedure: sp_CreateEmailChangeOTP
-- Tạo OTP mới cho việc thay đổi email
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_CreateEmailChangeOTP]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_CreateEmailChangeOTP]
GO

CREATE PROCEDURE [dbo].[sp_CreateEmailChangeOTP]
    @UserId INT,
    @CurrentEmail NVARCHAR(100),
    @NewEmail NVARCHAR(100),
    @Step TINYINT,
    @OtpCode NVARCHAR(6),
    @ExpiresInMinutes INT = 15
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ExpiresAt DATETIME = DATEADD(MINUTE, @ExpiresInMinutes, GETDATE())
    
    -- Vô hiệu hóa các OTP cũ chưa dùng của user cho step này
    UPDATE [dbo].[EmailChangeOTPs]
    SET [IsUsed] = 1, [UsedAt] = GETDATE()
    WHERE [UserId] = @UserId 
        AND [Step] = @Step
        AND [IsUsed] = 0
        AND [ExpiresAt] > GETDATE()
    
    -- Tạo OTP mới
    INSERT INTO [dbo].[EmailChangeOTPs] (
        [UserId], [CurrentEmail], [NewEmail], [Step], [OtpCode], 
        [ExpiresAt], [AttemptCount], [MaxAttempts]
    )
    VALUES (
        @UserId, @CurrentEmail, @NewEmail, @Step, @OtpCode,
        @ExpiresAt, 0, 5
    )
    
    SELECT SCOPE_IDENTITY() AS OtpId
END
GO

-- =============================================
-- Stored Procedure: sp_VerifyEmailChangeOTP
-- Xác thực OTP
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_VerifyEmailChangeOTP]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_VerifyEmailChangeOTP]
GO

CREATE PROCEDURE [dbo].[sp_VerifyEmailChangeOTP]
    @UserId INT,
    @Step TINYINT,
    @OtpCode NVARCHAR(6)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @IsValid BIT = 0
    DECLARE @Message NVARCHAR(200)
    DECLARE @OtpId INT
    DECLARE @NewEmail NVARCHAR(100)
    DECLARE @CurrentAttempts INT
    DECLARE @MaxAttempts INT
    DECLARE @IsBlocked BIT
    DECLARE @BlockedUntil DATETIME
    
    -- Lấy OTP gần nhất chưa dùng
    SELECT TOP 1
        @OtpId = Id,
        @NewEmail = NewEmail,
        @CurrentAttempts = AttemptCount,
        @MaxAttempts = MaxAttempts,
        @IsBlocked = IsBlocked,
        @BlockedUntil = BlockedUntil
    FROM [dbo].[EmailChangeOTPs]
    WHERE [UserId] = @UserId
        AND [Step] = @Step
        AND [IsUsed] = 0
        AND [ExpiresAt] > GETDATE()
    ORDER BY [CreatedAt] DESC
    
    -- Kiểm tra OTP tồn tại
    IF @OtpId IS NULL
    BEGIN
        SET @Message = 'OTP không tồn tại hoặc đã hết hạn'
        SELECT @IsValid AS IsValid, @Message AS Message, NULL AS NewEmail
        RETURN
    END
    
    -- Kiểm tra bị block
    IF @IsBlocked = 1 AND @BlockedUntil > GETDATE()
    BEGIN
        SET @Message = 'Tài khoản tạm thời bị khóa do nhập sai quá nhiều lần'
        SELECT @IsValid AS IsValid, @Message AS Message, NULL AS NewEmail
        RETURN
    END
    
    -- Kiểm tra số lần thử
    IF @CurrentAttempts >= @MaxAttempts
    BEGIN
        -- Block 1 giờ
        UPDATE [dbo].[EmailChangeOTPs]
        SET [IsBlocked] = 1,
            [BlockedUntil] = DATEADD(HOUR, 1, GETDATE())
        WHERE [Id] = @OtpId
        
        SET @Message = 'Đã vượt quá số lần thử. Tài khoản bị khóa 1 giờ'
        SELECT @IsValid AS IsValid, @Message AS Message, NULL AS NewEmail
        RETURN
    END
    
    -- Verify OTP
    IF EXISTS (
        SELECT 1 FROM [dbo].[EmailChangeOTPs]
        WHERE [Id] = @OtpId AND [OtpCode] = @OtpCode
    )
    BEGIN
        -- OTP đúng
        UPDATE [dbo].[EmailChangeOTPs]
        SET [IsUsed] = 1, [UsedAt] = GETDATE()
        WHERE [Id] = @OtpId
        
        SET @IsValid = 1
        SET @Message = 'Xác thực OTP thành công'
        SELECT @IsValid AS IsValid, @Message AS Message, @NewEmail AS NewEmail
    END
    ELSE
    BEGIN
        -- OTP sai - tăng attempt count
        UPDATE [dbo].[EmailChangeOTPs]
        SET [AttemptCount] = [AttemptCount] + 1
        WHERE [Id] = @OtpId
        
        SET @Message = 'Mã OTP không chính xác'
        SELECT @IsValid AS IsValid, @Message AS Message, NULL AS NewEmail
    END
END
GO

-- =============================================
-- Stored Procedure: sp_CleanupExpiredOTPs
-- Dọn dẹp OTP hết hạn (chạy định kỳ)
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_CleanupExpiredOTPs]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_CleanupExpiredOTPs]
GO

CREATE PROCEDURE [dbo].[sp_CleanupExpiredOTPs]
    @DaysOld INT = 7
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM [dbo].[EmailChangeOTPs]
    WHERE [ExpiresAt] < DATEADD(DAY, -@DaysOld, GETDATE())
    
    SELECT @@ROWCOUNT AS DeletedRows
END
GO

PRINT '============================================='
PRINT 'Email Change OTP System created successfully'
PRINT '============================================='
