IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'pf_Forum' AND COLUMN_NAME = 'ForumAdapterName')
BEGIN
   ALTER TABLE pf_Forum ADD ForumAdapterName VARCHAR(256) NULL
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'pf_Post' AND COLUMN_NAME = 'Votes')
BEGIN
   ALTER TABLE pf_Post ADD Votes int
   CONSTRAINT DF_pf_Post_Votes DEFAULT 0 NOT NULL
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'pf_Profile' AND COLUMN_NAME = 'Points')
BEGIN
   ALTER TABLE pf_Profile ADD Points int
   CONSTRAINT DF_pf_Profile_Points DEFAULT 0 NOT NULL
END

-- new tables for v9.2

CREATE TABLE [dbo].[pf_PostVote](
	[PostID] [int] NOT NULL,
	[UserID] [int] NOT NULL
) ON [PRIMARY]

CREATE CLUSTERED INDEX [IX_pf_PostVote_PostID] ON [dbo].[pf_PostVote] 
(
	[PostID] ASC
)

CREATE TABLE [dbo].[pf_Feed](
	[UserID] [int] NOT NULL,
	[Message] [nvarchar](max) NOT NULL,
	[Points] [int] NOT NULL,
	[TimeStamp] [datetime] NOT NULL
) ON [PRIMARY]

CREATE CLUSTERED INDEX [IX_pf_Feed_UserID] ON [dbo].[pf_Feed] 
(
	[UserID] ASC
)

CREATE TABLE [dbo].[pf_PointLedger](
	[UserID] [int] NOT NULL,
	[EventDefinitionID] [nvarchar](256) NOT NULL,
	[Points] [int] NOT NULL,
	[TimeStamp] [datetime] NOT NULL
)

CREATE CLUSTERED INDEX [IX_pf_PointLedger_UserID] ON [dbo].[pf_PointLedger] 
(
	[UserID] ASC
)

CREATE TABLE [dbo].[pf_EventDefinition](
	[EventDefinitionID] [nvarchar](256) NOT NULL PRIMARY KEY NONCLUSTERED,
	[Description] [nvarchar](max) NOT NULL,
	[PointValue] [int] NOT NULL,
	[IsPublishedToFeed] [bit] NOT NULL
)

CREATE TABLE [dbo].[pf_AwardCalculationQueue](
	[ID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY NONCLUSTERED,
	[EventDefinitionID] [nvarchar](256) NOT NULL,
	[UserID] [int] NOT NULL,
	[TimeStamp] [datetime] NOT NULL
)



CREATE TABLE [dbo].[pf_UserAward](
	[UserAwardID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY NONCLUSTERED,
	[UserID] [int] NOT NULL,
	[AwardDefinitionID] [nvarchar](256) NOT NULL,
	[Title] [nvarchar](256) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[TimeStamp] [datetime] NOT NULL
)

CREATE CLUSTERED INDEX [IX_pf_UserAward_UserID] ON [dbo].[pf_UserAward] 
(
	[UserID] ASC
)


CREATE TABLE [dbo].[pf_AwardDefinition](
	[AwardDefinitionID] [nvarchar](256) NOT NULL PRIMARY KEY NONCLUSTERED,
	[Title] [nvarchar](256) NOT NULL,
	[Description] [nvarchar](256) NOT NULL,
	[IsSingleTimeAward] [bit] NOT NULL
)


CREATE TABLE [dbo].[pf_AwardCondition](
	[AwardDefinitionID] [nvarchar](256) NOT NULL,
	[EventDefinitionID] [nvarchar](256) NOT NULL,
	[EventCount] [int] NOT NULL
)

CREATE CLUSTERED INDEX [IX_AwardCondition_EventDefinitionID] ON [dbo].[pf_AwardCondition] 
(
	[EventDefinitionID] ASC
)
