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