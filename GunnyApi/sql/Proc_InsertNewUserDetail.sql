-- =================================================
-- Author:      AMIS OneAI
-- Create date: 21/01/2026
-- Description: Thêm thông tin chi tiết của một người dùng mới vào hệ thống.
--              Bao gồm nhiều bảng liên quan như Sys_Users_Detail, DailyLogList,
--              Sys_Users_Extra, Sys_Users_Fight, Sys_Users_Texp,
--              Sys_Users_TitelIcon, và Sys_VIP_Info.
-- =================================================
ALTER PROCEDURE Proc_InsertNewUserDetail
    @UserID INT,
    @UserName NVARCHAR(200),
    @NickName NVARCHAR(200),
    @exp INT,
    @gold INT,
    @money INT,
    @sex bit
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        /* =========================================================
           1) INSERT Sys_Users_Detail (có IDENTITY_INSERT)
           ========================================================= */

        SET IDENTITY_INSERT [dbo].[Sys_Users_Detail] ON;

        INSERT INTO [dbo].[Sys_Users_Detail] (
            [UserID], [UserName], [Password], [NickName], [Date],
            [IsConsortia], [ConsortiaID], [Sex], [Win], [Total], [Escape],
            [GP], [Honor], [Gold], [Money],
            [Style], [Colors], [Hide], [Grade], [State], [IsFirst], [Repute],
            [LastDate], [ChargeDate], [ExpendDate], [ActiveIP], [ForbidDate],
            [Skin], [Offer], [IsExist], [ReputeOffer],
            [LastDateSecond], [LastDateThird], [LoginCount], [OnlineTime],
            [AntiAddiction], [AntiDate], [RichesOffer], [RichesRob],
            [LastDayGP], [AddDayGP], [LastWeekGP], [AddWeekGP],
            [LastDayOffer], [AddDayOffer], [LastWeekOffer], [AddWeekOffer],
            [CheckCount], [Site], [IsMarried], [SpouseID], [SpouseName],
            [MarryInfoID], [DayLoginCount], [ForbidReason], [IsCreatedMarryRoom],
            [PasswordTwo], [SelfMarryRoomID], [IsGotRing], [ServerName], [Rename],
            [Nimbus], [LastAward], [GiftToken], [QuestSite], [PvePermission],
            [FightPower], [AnswerSite], [LastAuncherAward], [Medal], [ChatCount],
            [SpaPubGoldRoomLimit], [LastSpaDate], [FightLabPermission],
            [SpaPubMoneyRoomLimit], [IsInSpaPubGoldToday], [IsInSpaPubMoneyToday],
            [AchievementPoint], [LastWeekly], [LastWeeklyVersion], [WeaklessGuildProgressStr], [IsOldPlayer],
            [Score], [OptionOnOff], [isOldPlayerHasValidEquitAtLogin],
            [badLuckNumber], [luckyNum], [lastLuckyNumDate], [lastLuckNum],
            [CardSoul], [totemId], [damageScores], [petScore],
            [IsShowConsortia], [LastRefreshPet], [LastGetEgg], [IsFistGetPet],
            [myHonor], [hardCurrency], [MaxBuyHonor],
            [necklaceExp], [necklaceExpAdd], [GetSoulCount]
        )
        VALUES (
            @UserID,                                        -- [UserID]
            @UserName,                                      -- [UserName]
            N'',                                            -- [Password]
            @NickName,                                      -- [NickName]
            CAST(N'2016-07-27 14:00:53.757' AS DateTime),    -- [Date]
            1,                                              -- [IsConsortia]
            0,                                              -- [ConsortiaID]
            @sex,                                              -- [Sex]
            0,                                              -- [Win]
            0,                                              -- [Total]
            0,                                              -- [Escape]
            @exp,                                           -- [GP]
            N'',                                            -- [Honor]
            @gold,                                          -- [Gold]
            @money,                                         -- [Money]
            N',,,,,,,,,,,,,,,',                             -- [Style]
            N',,,,,,,,,,,,,,,',                             -- [Colors]
            1111112223,                                     -- [Hide]
            10,                                             -- [Grade]
            0,                                              -- [State]
            564,                                            -- [IsFirst]
            0,                                              -- [Repute]
            CAST(N'2017-06-05 16:34:02.143' AS DateTime),    -- [LastDate]
            NULL,                                           -- [ChargeDate]
            NULL,                                           -- [ExpendDate]
            N'113.182.86.106',                              -- [ActiveIP]
            CAST(N'2016-07-27 14:00:53.757' AS DateTime),    -- [ForbidDate]
            N'',                                            -- [Skin]
            1333,                                           -- [Offer]
            1,                                              -- [IsExist]
            0,                                              -- [ReputeOffer]
            CAST(N'2017-06-04 14:52:59.820' AS DateTime),    -- [LastDateSecond]
            CAST(N'2017-05-25 14:29:38.120' AS DateTime),    -- [LastDateThird]
            2,                                              -- [LoginCount]
            0,                                              -- [OnlineTime]
            7,                                              -- [AntiAddiction]
            CAST(N'2017-06-05 16:36:40.170' AS DateTime),    -- [AntiDate]
            0,                                              -- [RichesOffer]
            0,                                              -- [RichesRob]
            0,                                              -- [LastDayGP]
            0,                                              -- [AddDayGP]
            0,                                              -- [LastWeekGP]
            0,                                              -- [AddWeekGP]
            0,                                              -- [LastDayOffer]
            0,                                              -- [AddDayOffer]
            0,                                              -- [LastWeekOffer]
            0,                                              -- [AddWeekOffer]
            0,                                              -- [CheckCount]
            N'',                                            -- [Site]
            0,                                              -- [IsMarried]
            0,                                              -- [SpouseID]
            NULL,                                           -- [SpouseName]
            0,                                              -- [MarryInfoID]
            0,                                              -- [DayLoginCount]
            NULL,                                           -- [ForbidReason]
            0,                                              -- [IsCreatedMarryRoom]
            N'',                                            -- [PasswordTwo]
            0,                                              -- [SelfMarryRoomID]
            0,                                              -- [IsGotRing]
            NULL,                                           -- [ServerName]
            0,                                              -- [Rename]
            504,                                            -- [Nimbus]
            CAST(N'2017-03-03 12:53:03.000' AS DateTime),    -- [LastAward]
            450,                                            -- [GiftToken]
            0x5F7000380E000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000, -- [QuestSite]
            N'FFFFFFFFFFFFFFFFF',                           -- [PvePermission]
            0,                                              -- [FightPower]
            301,                                            -- [AnswerSite]
            CAST(N'2017-03-03 12:53:03.000' AS DateTime),    -- [LastAuncherAward]
            0,                                              -- [Medal]
            0,                                              -- [ChatCount]
            0,                                              -- [SpaPubGoldRoomLimit]
            CAST(N'2016-07-27 02:00:53.000' AS DateTime),    -- [LastSpaDate]
            N'0000000000',                                  -- [FightLabPermission]
            0,                                              -- [SpaPubMoneyRoomLimit]
            0,                                              -- [IsInSpaPubGoldToday]
            0,                                              -- [IsInSpaPubMoneyToday]
            0,                                              -- [AchievementPoint]
            CAST(N'2016-07-27 02:00:53.000' AS DateTime),    -- [LastWeekly]
            0,                                              -- [LastWeeklyVersion]
            N'RfE/DaAAAtgWdQ8AAAAAAAAAAAAAAAAAAAAAABgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=', -- [WeaklessGuildProgressStr]
            0,                                              -- [IsOldPlayer]
            0,                                              -- [Score]
            0,                                              -- [OptionOnOff]
            0,                                              -- [isOldPlayerHasValidEquitAtLogin]
            0,                                              -- [badLuckNumber]
            0,                                              -- [luckyNum]
            1,                                              -- [lastLuckyNumDate]
            0,                                              -- [lastLuckNum]
            0,                                              -- [CardSoul]
            0,                                              -- [totemId]
            0,                                              -- [damageScores]
            0,                                              -- [petScore]
            0,                                              -- [IsShowConsortia]
            29,                                             -- [LastRefreshPet]
            0,                                              -- [LastGetEgg]
            1,                                              -- [IsFistGetPet]
            0,                                              -- [myHonor]
            0,                                              -- [hardCurrency]
            265775,                                         -- [MaxBuyHonor]
            0,                                              -- [necklaceExp]
            174,                                            -- [necklaceExpAdd]
            20000                                           -- [GetSoulCount]
        );

        SET IDENTITY_INSERT [dbo].[Sys_Users_Detail] OFF;

        /* =========================================================
           2) INSERT DailyLogList
           ========================================================= */
        INSERT INTO [dbo].[DailyLogList] (
            [UserID],        -- User id
            [UserAwardLog],  -- Log thưởng ngày
            [DayLog]         -- Chuỗi trạng thái theo ngày
        )
        VALUES (
            @UserID,                         -- [UserID]
            0,                               -- [UserAwardLog]
            N'False,False,False,False'       -- [DayLog]
        );

        /* =========================================================
           3) INSERT Sys_Users_Extra (có IDENTITY_INSERT theo script bạn đưa)
           ========================================================= */

        INSERT INTO [dbo].[Sys_Users_Extra] (
            [UserID],            -- User id
            [FreeSendMailCount]  -- Lượt free send mail
        )
        VALUES (
            @UserID,                                        -- [UserID]
            0                                               -- [FreeSendMailCount]
        );

        /* =========================================================
           4) INSERT Sys_Users_Fight
           ========================================================= */
        INSERT INTO [dbo].[Sys_Users_Fight] (
            [UserID],          -- User id
            [Attack],          -- Công
            [Defence],         -- Thủ
            [Luck],            -- May mắn
            [Agility],         -- Nhanh nhẹn
            [Delay],           -- Delay
            [Honor],           -- Danh hiệu
            [Map],             -- Map
            [Directory],       -- Directory
            [IsExist],         -- Tồn tại
            [hp]               -- Máu
        )
        VALUES (
            @UserID,   -- [UserID]
            500,       -- [Attack]
            500,       -- [Defence]
            500,       -- [Luck]
            500,       -- [Agility]
            0,         -- [Delay]
            N'',       -- [Honor]
            N'',       -- [Map]
            N'',       -- [Directory]
            1,         -- [IsExist]
            3000       -- [hp]
        );

        /* =========================================================
           5) INSERT Sys_Users_Texp
           ========================================================= */
        INSERT INTO [dbo].[Sys_Users_Texp] (
            [UserID],        -- User id
            [spdTexpExp],    -- Exp tốc
            [attTexpExp],    -- Exp công
            [defTexpExp],    -- Exp thủ
            [hpTexpExp],     -- Exp máu
            [lukTexpExp],    -- Exp may mắn
            [texpTaskCount], -- Số task texp
            [texpCount],     -- Count texp
            [texpTaskDate]   -- Ngày task
        )
        VALUES (
            @UserID,                                    -- [UserID]
            0,                                          -- [spdTexpExp]
            0,                                          -- [attTexpExp]
            0,                                          -- [defTexpExp]
            0,                                          -- [hpTexpExp]
            0,                                          -- [lukTexpExp]
            0,                                          -- [texpTaskCount]
            0,                                          -- [texpCount]
            CAST(N'2016-08-01 10:53:31.000' AS DateTime) -- [texpTaskDate]
        );

        /* =========================================================
           6) INSERT Sys_VIP_Info
           ========================================================= */
        SET IDENTITY_INSERT [dbo].[Sys_VIP_Info] ON;
         
        INSERT INTO [dbo].[Sys_VIP_Info] (
            [UserID],               -- User id
            [typeVIP],              -- Loại VIP
            [VIPLevel],             -- Level VIP
            [VIPExp],               -- Exp VIP
            [VIPOnlineDays],        -- Ngày online
            [VIPOfflineDays],       -- Ngày offline
            [VIPExpireDay],         -- Ngày hết hạn
            [LastVIPPackTime],      -- Lần nhận pack cuối
            [VIPLastdate],          -- Lastdate VIP
            [VIPNextLevelDaysNeeded], -- Ngày cần để lên level
            [CanTakeVipReward]      -- Có thể nhận thưởng
        )
        VALUES (
            @UserID,                                        -- [UserID]
            1,                                              -- [typeVIP]
            0,                                              -- [VIPLevel]
            0,                                              -- [VIPExp]
            0,                                              -- [VIPOnlineDays]
            0,                                              -- [VIPOfflineDays]
            CAST(N'2033-04-29 23:16:07.100' AS DateTime),    -- [VIPExpireDay]
            CAST(N'2017-04-02 09:58:30.137' AS DateTime),    -- [LastVIPPackTime]
            CAST(N'2016-07-26 14:00:53.757' AS DateTime),    -- [VIPLastdate]
            0,                                              -- [VIPNextLevelDaysNeeded]
            1                                               -- [CanTakeVipReward]
        );

        SET IDENTITY_INSERT [dbo].[Sys_VIP_Info] OFF;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
