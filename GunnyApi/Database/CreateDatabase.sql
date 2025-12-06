-- Tạo database
CREATE DATABASE GunnyDB;
GO

USE GunnyDB;
GO

-- Tạo bảng Users
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
GO

-- Insert dữ liệu mẫu
INSERT INTO Users (Username, Email, FullName, CreatedAt, IsActive)
VALUES 
    ('admin', 'admin@gunny.com', 'Administrator', GETDATE(), 1),
    ('user1', 'user1@gunny.com', 'User One', GETDATE(), 1),
    ('user2', 'user2@gunny.com', 'User Two', GETDATE(), 1),
    ('testuser', 'test@gunny.com', 'Test User', GETDATE(), 0);
GO

-- Tạo index
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);
GO
