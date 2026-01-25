-- Tạo bảng PasswordResets để lưu token reset password
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PasswordResets')
BEGIN
    CREATE TABLE [dbo].[PasswordResets](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [Email] [nvarchar](255) NOT NULL,
        [Token] [nvarchar](500) NOT NULL,
        [ExpiresAt] [datetime] NOT NULL,
        [IsUsed] [bit] NOT NULL DEFAULT 0,
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        [UsedAt] [datetime] NULL,
        CONSTRAINT [PK_PasswordResets] PRIMARY KEY CLUSTERED ([Id] ASC)
    )

    -- Index để tìm kiếm nhanh theo token
    CREATE NONCLUSTERED INDEX [IX_PasswordResets_Token] 
    ON [dbo].[PasswordResets] ([Token])

    -- Index để tìm kiếm theo email
    CREATE NONCLUSTERED INDEX [IX_PasswordResets_Email] 
    ON [dbo].[PasswordResets] ([Email])

    -- Index để tìm kiếm theo UserId
    CREATE NONCLUSTERED INDEX [IX_PasswordResets_UserId] 
    ON [dbo].[PasswordResets] ([UserId])

    PRINT 'Bảng PasswordResets đã được tạo thành công'
END
ELSE
BEGIN
    PRINT 'Bảng PasswordResets đã tồn tại'
END
GO

-- Stored Procedure: Tạo password reset token
CREATE OR ALTER PROCEDURE [dbo].[sp_CreatePasswordResetToken]
    @UserId INT,
    @Email NVARCHAR(255),
    @Token NVARCHAR(500),
    @ExpiresAt DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    -- Vô hiệu hóa tất cả token cũ của user này (nếu có)
    UPDATE [dbo].[PasswordResets]
    SET [IsUsed] = 1, [UsedAt] = GETDATE()
    WHERE [UserId] = @UserId AND [IsUsed] = 0

    -- Tạo token mới
    INSERT INTO [dbo].[PasswordResets] ([UserId], [Email], [Token], [ExpiresAt], [IsUsed], [CreatedAt])
    VALUES (@UserId, @Email, @Token, @ExpiresAt, 0, GETDATE())

    SELECT SCOPE_IDENTITY() AS Id
END
GO

-- Stored Procedure: Verify password reset token
CREATE OR ALTER PROCEDURE [dbo].[sp_VerifyPasswordResetToken]
    @Token NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        pr.[Id],
        pr.[UserId],
        pr.[Email],
        pr.[Token],
        pr.[ExpiresAt],
        pr.[IsUsed],
        pr.[CreatedAt],
        pr.[UsedAt]
    FROM [dbo].[PasswordResets] pr
    WHERE pr.[Token] = @Token
      AND pr.[IsUsed] = 0
      AND pr.[ExpiresAt] > GETDATE()
END
GO

-- Stored Procedure: Mark token as used
CREATE OR ALTER PROCEDURE [dbo].[sp_MarkPasswordResetTokenAsUsed]
    @Token NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[PasswordResets]
    SET [IsUsed] = 1, [UsedAt] = GETDATE()
    WHERE [Token] = @Token

    SELECT @@ROWCOUNT AS RowsAffected
END
GO

-- Stored Procedure: Cleanup expired tokens (nên chạy định kỳ)
CREATE OR ALTER PROCEDURE [dbo].[sp_CleanupExpiredPasswordResetTokens]
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [dbo].[PasswordResets]
    WHERE [ExpiresAt] < DATEADD(DAY, -7, GETDATE()) -- Xóa token hết hạn cách đây hơn 7 ngày

    SELECT @@ROWCOUNT AS DeletedRows
END
GO

PRINT 'Tất cả stored procedures đã được tạo/cập nhật thành công'
GO
