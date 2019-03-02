CREATE CLUSTERED INDEX [IX_pf_Topic_ForumID] ON [dbo].[pf_Topic] 
(
	[ForumID] ASC,
	[IsPinned] DESC,
	[LastPostTime] DESC
) WITH (drop_existing = on)

CREATE NONCLUSTERED INDEX [IX_pf_Topic_LastPostTime] ON [dbo].[pf_Topic]
(
	[LastPostTime] DESC
)

CREATE NONCLUSTERED INDEX [IX_pf_Favorite_TopicID] ON [dbo].[pf_Favorite]
(
	[TopicID] ASC
)

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

