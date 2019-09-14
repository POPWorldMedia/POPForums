DROP INDEX [IX_PopForumsUser_UserName] ON [dbo].[pf_PopForumsUser]
CREATE UNIQUE NONCLUSTERED INDEX [IX_PopForumsUser_UserName] ON [dbo].[pf_PopForumsUser]([Name])
DROP INDEX [IX_PopForumsUser_Email] ON [dbo].[pf_PopForumsUser]
CREATE UNIQUE NONCLUSTERED INDEX [IX_PopForumsUser_Email] ON [dbo].[pf_PopForumsUser]([Email])

DELETE FROM pf_Setting WHERE [Setting] = 'TwitterConsumerKey'
DELETE FROM pf_Setting WHERE [Setting] = 'TwitterConsumerSecret'
DELETE FROM pf_Setting WHERE [Setting] = 'UseTwitterLogin'

IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'pf_UserActivity'))
BEGIN
	CREATE TABLE [dbo].[pf_UserActivity](
		[UserID] [int] NOT NULL PRIMARY KEY CLUSTERED,
		[LastActivityDate] [datetime] NOT NULL,
		[LastLoginDate] [datetime] NOT NULL
	)

	ALTER TABLE [dbo].[pf_UserActivity]  WITH CHECK ADD  CONSTRAINT [FK_pf_UserActivity_pf_PopForumsUser] FOREIGN KEY([UserID])
	REFERENCES [dbo].[pf_PopForumsUser] ([UserID])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[pf_UserActivity] CHECK CONSTRAINT [FK_pf_UserActivity_pf_PopForumsUser]

	INSERT INTO pf_UserActivity SELECT UserID, LastActivityDate, LastLoginDate FROM pf_PopForumsUser
END

IF EXISTS( SELECT TOP 1 1 FROM sys.objects o INNER JOIN sys.columns c ON o.object_id = c.object_id WHERE o.name = 'pf_PopForumsUser' AND c.name = 'LastActivityDate')
ALTER TABLE dbo.pf_PopForumsUser DROP COLUMN [LastActivityDate]

IF EXISTS( SELECT TOP 1 1 FROM sys.objects o INNER JOIN sys.columns c ON o.object_id = c.object_id WHERE o.name = 'pf_PopForumsUser' AND c.name = 'LastLoginDate')
ALTER TABLE dbo.pf_PopForumsUser DROP COLUMN [LastLoginDate]