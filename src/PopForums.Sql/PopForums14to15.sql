CREATE CLUSTERED INDEX [IX_pf_Topic_ForumID] ON [dbo].[pf_Topic] 
(
	[ForumID] ASC,
	[IsPinned] DESC,
	[LastPostTime] DESC
) WITH (drop_existing = on)

IF IndexProperty(Object_Id('pf_Topic'), 'IX_pf_Topic_LastPostTime', 'IndexId') IS NULL
CREATE NONCLUSTERED INDEX [IX_pf_Topic_LastPostTime] ON [dbo].[pf_Topic]
(
	[LastPostTime] DESC
)

IF IndexProperty(Object_Id('pf_Favorite'), 'IX_pf_Favorite_TopicID', 'IndexId') IS NULL
CREATE NONCLUSTERED INDEX [IX_pf_Favorite_TopicID] ON [dbo].[pf_Favorite]
(
	[TopicID] ASC
)

IF IndexProperty(Object_Id('pf_LastTopicView'), 'IX_pf_LastTopicView_TopicID', 'IndexId') IS NULL
CREATE NONCLUSTERED INDEX [IX_pf_LastTopicView_TopicID] ON [dbo].[pf_LastTopicView]
(
	[TopicID] ASC
)

IF EXISTS( SELECT TOP 1 1 FROM sys.objects o INNER JOIN sys.columns c ON o.object_id = c.object_id WHERE o.name = 'pf_Profile' AND c.name = 'ICQ')
ALTER TABLE dbo.pf_Profile DROP COLUMN [ICQ]

IF EXISTS( SELECT TOP 1 1 FROM sys.objects o INNER JOIN sys.columns c ON o.object_id = c.object_id WHERE o.name = 'pf_Profile' AND c.name = 'YahooMessenger')
ALTER TABLE dbo.pf_Profile DROP COLUMN [YahooMessenger]

IF NOT EXISTS( SELECT TOP 1 1 FROM sys.objects o INNER JOIN sys.columns c ON o.object_id = c.object_id WHERE o.name = 'pf_Profile' AND c.name = 'Instagram')
ALTER TABLE dbo.pf_Profile ADD [Instagram] nvarchar(256) NULL

IF IndexProperty(Object_Id('pf_SecurityLog'), 'IX_pf_SecurityLog_IP', 'IndexId') IS NOT NULL
DROP INDEX [IX_pf_SecurityLog_IP] ON [dbo].[pf_SecurityLog]

IF IndexProperty(Object_Id('pf_SecurityLog'), 'IX_pf_SecurityLog_IP_ActivityDate', 'IndexId') IS NULL
CREATE NONCLUSTERED INDEX [IX_pf_SecurityLog_IP_ActivityDate] ON [dbo].[pf_SecurityLog] 
(
	[IP] ASC, [ActivityDate] DESC
)

IF IndexProperty(Object_Id('pf_SecurityLog'), 'IX_pf_SecurityLog_UserID_ActivityDate', 'IndexId') IS NULL
CREATE NONCLUSTERED INDEX [IX_pf_SecurityLog_UserID_ActivityDate] ON [dbo].[pf_SecurityLog] 
(
	[UserID] ASC, [ActivityDate] DESC
)

IF IndexProperty(Object_Id('pf_SecurityLog'), 'IX_pf_SecurityLog_TargetUserID_ActivityDate', 'IndexId') IS NULL
CREATE NONCLUSTERED INDEX [IX_pf_SecurityLog_TargetUserID_ActivityDate] ON [dbo].[pf_SecurityLog] 
(
	[TargetUserID] ASC, [ActivityDate] DESC
)

IF IndexProperty(Object_Id('pf_Post'), 'IX_pf_Post_IP', 'IndexId') IS NOT NULL
DROP INDEX [IX_pf_Post_IP] ON [dbo].[pf_Post]

IF IndexProperty(Object_Id('pf_Post'), 'IX_pf_Post_IP_PostTime', 'IndexId') IS NULL
CREATE NONCLUSTERED INDEX [IX_pf_Post_IP_PostTime] ON [dbo].[pf_Post] 
(
	[IP] ASC, [PostTime] DESC
) 



DROP TABLE [dbo].[pf_AwardCalculationQueue]

CREATE TABLE [dbo].[pf_AwardCalculationQueue](
	[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,
	[Payload] [nvarchar](256) NOT NULL
)

DROP TABLE [dbo].[pf_SearchQueue]

CREATE TABLE [dbo].[pf_SearchQueue](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Payload] [nvarchar](256) NOT NULL
)

IF IndexProperty(Object_Id('pf_Topic'), 'pf_Topic_IsIndexed_IsDeleted', 'IndexId') IS NOT NULL
DROP INDEX [pf_Topic_IsIndexed_IsDeleted] ON [dbo].[pf_Topic]

IF EXISTS( SELECT TOP 1 1 FROM sys.objects o INNER JOIN sys.columns c ON o.object_id = c.object_id WHERE o.name = 'pf_Topic' AND c.name = 'IsIndexed')
ALTER TABLE dbo.pf_Topic DROP COLUMN [IsIndexed]

DROP TABLE [dbo].[pf_ServiceHeartbeat]

CREATE TABLE [dbo].[pf_ServiceHeartbeat](
	[ServiceName] [nvarchar](256) NOT NULL,
	[MachineName] [nvarchar](256) NOT NULL,
	[LastRun] [datetime] NOT NULL,
)


IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'pf_TopicViewLog'))
BEGIN
	CREATE TABLE [dbo].[pf_TopicViewLog](
		[ID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY NONCLUSTERED,
		[UserID] [int] NULL,
		[TopicID] [int] NULL,
		[TimeStamp] [datetime] NOT NULL
	)
END

IF IndexProperty(Object_Id('pf_TopicViewLog'), 'IX_pf_TopicViewLog_TimeStamp', 'IndexId') IS NULL
CREATE CLUSTERED INDEX [IX_pf_TopicViewLog_TimeStamp] ON [dbo].[pf_TopicViewLog]
(
	[TimeStamp] ASC
)

