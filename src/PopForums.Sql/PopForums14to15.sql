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
