-- For recent users view in admin
IF INDEXPROPERTY(Object_Id('pf_SecurityLog'), 'IX_pf_SecurityLog_TargetUserID_SecurityLogType', 'IndexID') IS NULL
BEGIN
	CREATE NONCLUSTERED INDEX IX_pf_SecurityLog_TargetUserID_SecurityLogType 
	ON pf_SecurityLog
	(
		TargetUserID DESC,
		SecurityLogType
	);
END


DROP INDEX IF EXISTS [IX_Friend_ToUserID] ON [pf_Friend];
DROP INDEX IF EXISTS [IX_Friend_FromUserID] ON [pf_Friend];
IF OBJECT_ID('pf_Friend', 'U') IS NOT NULL
BEGIN
	DROP TABLE pf_Friend;
END


IF COL_LENGTH('dbo.pf_Profile', 'TimeZone') IS NOT NULL
BEGIN
	ALTER TABLE pf_Profile DROP COLUMN TimeZone;
END
IF COL_LENGTH('dbo.pf_Profile', 'IsDaylightSaving') IS NOT NULL
BEGIN
	ALTER TABLE pf_Profile DROP COLUMN IsDaylightSaving;
END

DELETE FROM pf_Setting WHERE Setting = 'ServerDaylightSaving' OR Setting = 'ServerTimeZone';






IF OBJECT_ID('pf_Notifications', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[pf_Notifications]
	(
		[UserID] INT NOT NULL, 
		[NotificationType] INT NOT NULL, 
		[ContextID] BIGINT NOT NULL, 
		[TimeStamp] DATETIME NOT NULL, 
		[IsRead] BIT NOT NULL, 
		[Data] NVARCHAR(MAX) NULL
	);
END

IF INDEXPROPERTY(Object_Id('pf_Notifications'), 'IX_pf_Notifications_UserID_TimeStamp', 'IndexID') IS NULL
BEGIN
	CREATE CLUSTERED INDEX [IX_pf_Notifications_UserID_TimeStamp] ON [dbo].[pf_Notifications]
	(
		UserID, [TimeStamp] DESC
	);
END
IF INDEXPROPERTY(Object_Id('pf_Notifications'), 'IX_pf_Notifications_Context', 'IndexID') IS NULL
BEGIN
	CREATE INDEX [IX_pf_Notifications_Context] ON [dbo].[pf_Notifications]
	(
		UserID, NotificationType, ContextID
	);
END



IF COL_LENGTH('dbo.pf_Profile', 'IsAutoFollowOnReply') IS NULL
BEGIN
	ALTER TABLE pf_Profile ADD [IsAutoFollowOnReply] [bit] NOT NULL DEFAULT(1);
END



IF COL_LENGTH('dbo.pf_SubscribeTopic', 'IsViewed') IS NOT NULL
BEGIN
	ALTER TABLE pf_SubscribeTopic DROP COLUMN IsViewed;
END




IF INDEXPROPERTY(Object_Id('pf_SubscribeTopic'), 'IX_pf_SubscribeTopic_TopicID_UserID', 'IndexID') IS NOT NULL
BEGIN
	DROP INDEX [IX_pf_SubscribeTopic_TopicID_UserID] ON [dbo].[pf_SubscribeTopic];
END
IF INDEXPROPERTY(Object_Id('pf_SubscribeTopic'), 'IX_pf_SubscribeTopic_UserID_TopicID', 'IndexID') IS NULL
BEGIN
	CREATE UNIQUE CLUSTERED INDEX [IX_pf_SubscribeTopic_UserID_TopicID] ON [dbo].[pf_SubscribeTopic] 
	(
		[TopicID] ASC,
		[UserID] ASC
	);
END
IF INDEXPROPERTY(Object_Id('pf_SubscribeTopic'), 'IX_pf_SubscribeTopic_UserID', 'IndexID') IS NULL
BEGIN
	CREATE INDEX [IX_pf_SubscribeTopic_UserID] ON [dbo].[pf_SubscribeTopic] 
	(
		[UserID] ASC
	);
END



IF OBJECT_ID('pf_PostImage', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[pf_PostImage]
	(
		[ID] NVARCHAR(50) NOT NULL PRIMARY KEY,
		[TimeStamp] DATETIME NOT NULL,
		[TenantID] NVARCHAR(100) NULL,
		[ContentType] NVARCHAR(50) NOT NULL,
		[ImageData] VARBINARY(MAX) NOT NULL
	);
END
IF INDEXPROPERTY(Object_Id('pf_PostImage'), 'IX_pf_PostImage_TenantID', 'IndexID') IS NULL
BEGIN
	CREATE INDEX [IX_pf_PostImage_TenantID] ON [dbo].[pf_PostImage] ([TenantID]);
END



IF OBJECT_ID('pf_PostImageTemp', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[pf_PostImageTemp](
		[PostImageTempID] [uniqueidentifier] NOT NULL PRIMARY KEY,
		[TimeStamp] [datetime] NOT NULL
	);
END
IF INDEXPROPERTY(Object_Id('pf_PostImageTemp'), 'IX_pf_PostImageTemp_TimeStamp', 'IndexID') IS NULL
BEGIN
	CREATE NONCLUSTERED INDEX [IX_pf_PostImageTemp_TimeStamp] ON [dbo].[pf_PostImageTemp] ([TimeStamp]);
END
