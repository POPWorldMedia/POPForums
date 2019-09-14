-- ******************************************************** pf_PopForumsUser

CREATE TABLE [dbo].[pf_PopForumsUser](
	[UserID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,
	[Name] [nvarchar](256) NOT NULL,
	[Email] [nvarchar](256) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[IsApproved] [bit] NOT NULL DEFAULT ((-1)),
	[Password] [nvarchar](256) NOT NULL,
	[AuthorizationKey] [uniqueidentifier] NOT NULL DEFAULT ('00000000-0000-0000-0000-000000000000'),
	[Salt] [uniqueidentifier] NULL
) 

CREATE UNIQUE NONCLUSTERED INDEX [IX_PopForumsUser_UserName] ON [dbo].[pf_PopForumsUser]([Name])
CREATE UNIQUE NONCLUSTERED INDEX [IX_PopForumsUser_Email] ON [dbo].[pf_PopForumsUser]([Email])




-- ******************************************************** pf_Profile

CREATE TABLE [dbo].[pf_Profile](
	[UserID] [int] NOT NULL PRIMARY KEY,
	[IsSubscribed] [bit] NOT NULL DEFAULT ((0)),
	[Signature] nvarchar(MAX) NOT NULL,
	[ShowDetails] [bit] NOT NULL DEFAULT ((0)),
	[Location] [nvarchar](256) NOT NULL,
	[IsPlainText] [bit] NOT NULL DEFAULT ((0)),
	[DOB] [datetime] NULL,
	[Web] [nvarchar](256) NOT NULL,
	[Facebook] [nvarchar](256) NULL,
	[Twitter] [nvarchar](256) NULL,
	[Instagram] [nvarchar](256) NULL,
	[IsTos] [bit] NOT NULL DEFAULT ((0)),
	[TimeZone] [int] NOT NULL,
	[IsDaylightSaving] [bit] NOT NULL,
	[AvatarID] [int] NULL,
	[ImageID] [int] NULL,
	[HideVanity] [bit] NOT NULL DEFAULT ((0)),
	[LastPostID] [int] NULL,
	[Points] [int] NOT NULL DEFAULT (0)
) 


ALTER TABLE [dbo].[pf_Profile]  WITH CHECK ADD  CONSTRAINT [FK_pf_Profile_pf_PopForumsUser] FOREIGN KEY([UserID])
REFERENCES [dbo].[pf_PopForumsUser] ([UserID])
ON DELETE CASCADE

ALTER TABLE [dbo].[pf_Profile] CHECK CONSTRAINT [FK_pf_Profile_pf_PopForumsUser]


-- ******************************************************** pf_UserActivity

CREATE TABLE [dbo].[pf_UserActivity](
	[UserID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[LastActivityDate] [datetime] NOT NULL,
	[LastLoginDate] [datetime] NOT NULL
)

ALTER TABLE [dbo].[pf_UserActivity]  WITH CHECK ADD  CONSTRAINT [FK_pf_UserActivity_pf_PopForumsUser] FOREIGN KEY([UserID])
REFERENCES [dbo].[pf_PopForumsUser] ([UserID])
ON DELETE CASCADE

ALTER TABLE [dbo].[pf_UserActivity] CHECK CONSTRAINT [FK_pf_UserActivity_pf_PopForumsUser]








-- ******************************************************** pf_EmailBan

CREATE TABLE [dbo].[pf_EmailBan] (
	[EmailBan] [nvarchar] (256) NOT NULL PRIMARY KEY 
) 




-- ******************************************************** pf_IPBan

CREATE TABLE [dbo].[pf_IPBan] (
	[IPBan] [nvarchar] (256) NOT NULL PRIMARY KEY 
) 





-- ******************************************************** pf_Role and pf_PopForumsUserRole

CREATE TABLE [dbo].[pf_Role] (
	[Role] [nvarchar] (256) NOT NULL PRIMARY KEY
) 


CREATE TABLE [dbo].[pf_PopForumsUserRole] (
	[UserID] [int] NOT NULL ,
	[Role] [nvarchar] (256) NOT NULL
) 


CREATE CLUSTERED INDEX [IX_PopForumsUserRole_UserID] ON [dbo].[pf_PopForumsUserRole]([UserID]) 


ALTER TABLE [dbo].[pf_PopForumsUserRole]  WITH CHECK ADD  CONSTRAINT [FK_pf_PopForumsUserRole_Role] FOREIGN KEY([Role])
REFERENCES [dbo].[pf_Role] ([Role])
ON DELETE CASCADE

ALTER TABLE [dbo].[pf_PopForumsUserRole] CHECK CONSTRAINT [FK_pf_PopForumsUserRole_Role]


ALTER TABLE [dbo].[pf_PopForumsUserRole]  WITH CHECK ADD  CONSTRAINT [FK_pf_PopForumsUserRole_UserID] FOREIGN KEY([UserID])
REFERENCES [dbo].[pf_PopForumsUser] ([UserID])
ON DELETE CASCADE

ALTER TABLE [dbo].[pf_PopForumsUserRole] CHECK CONSTRAINT [FK_pf_PopForumsUserRole_UserID]


INSERT INTO pf_Role (Role) VALUES ('Admin')

INSERT INTO pf_Role (Role) VALUES ('Moderator')


-- ******************************************************** pf_SecurityLog

CREATE TABLE [dbo].[pf_SecurityLog](
	[SecurityLogID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[SecurityLogType] [int] NOT NULL,
	[UserID] [int] NULL,
	[TargetUserID] [int] NULL,
	[IP] [nvarchar](40) NOT NULL,
	[Message] [nvarchar](256) NOT NULL,
	[ActivityDate] [datetime] NOT NULL
) 

CREATE NONCLUSTERED INDEX [IX_pf_SecurityLog_IP_ActivityDate] ON [dbo].[pf_SecurityLog] 
(
	[IP] ASC, [ActivityDate] DESC
)

CREATE NONCLUSTERED INDEX [IX_pf_SecurityLog_UserID_ActivityDate] ON [dbo].[pf_SecurityLog] 
(
	[UserID] ASC, [ActivityDate] DESC
)

CREATE NONCLUSTERED INDEX [IX_pf_SecurityLog_TargetUserID_ActivityDate] ON [dbo].[pf_SecurityLog] 
(
	[TargetUserID] ASC, [ActivityDate] DESC
)


-- ******************************************************** pf_Category

CREATE TABLE [dbo].[pf_Category](
	[CategoryID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Title] [nvarchar](256) NOT NULL,
	[SortOrder] [int] NOT NULL
) 




-- ******************************************************** pf_Forum

CREATE TABLE [dbo].[pf_Forum](
	[ForumID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[CategoryID] [int] NULL,
	[Title] [nvarchar](256) NOT NULL,
	[Description] [nvarchar](256) NOT NULL,
	[IsVisible] [bit] NOT NULL,
	[IsArchived] [bit] NOT NULL,
	[SortOrder] [int] NOT NULL,
	[TopicCount] [int] NOT NULL,
	[PostCount] [int] NOT NULL,
	[LastPostTime] [datetime] NOT NULL,
	[LastPostName] [nvarchar](256) NOT NULL,
	[UrlName] [nvarchar](256) NOT NULL,
	[ForumAdapterName] [varchar](256) NULL,
	[IsQAForum] [bit] NOT NULL DEFAULT ((0))
) 


CREATE UNIQUE NONCLUSTERED INDEX [IX_pf_Forum_UrlName] ON [dbo].[pf_Forum] 
(
	[UrlName] ASC
) 






-- ******************************************************** pf_Topic

CREATE TABLE [dbo].[pf_Topic](
	[TopicID] [int] IDENTITY(1,1) NOT NULL,
	[ForumID] [int] NOT NULL DEFAULT (0),
	[Title] [nvarchar](256) NOT NULL,
	[ReplyCount] [int] NOT NULL,
	[ViewCount] [int] NOT NULL,
	[StartedByUserID] [int] NOT NULL,
	[StartedByName] [nvarchar](256) NOT NULL,
	[LastPostUserID] [int] NOT NULL,
	[LastPostName] [nvarchar](256) NOT NULL,
	[LastPostTime] [datetime] NOT NULL,
	[IsClosed] [bit] NOT NULL,
	[IsPinned] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[UrlName] [nvarchar](256) NOT NULL,
	[AnswerPostID] [int] NULL,
	CONSTRAINT [PK_pf_Topic] PRIMARY KEY NONCLUSTERED 
		( [TopicID] ASC ) 
) 

CREATE CLUSTERED INDEX [IX_pf_Topic_ForumID] ON [dbo].[pf_Topic] 
(
	[ForumID] ASC,
	[IsPinned] DESC,
	[LastPostTime] DESC
) 

ALTER TABLE [dbo].[pf_Topic]  WITH CHECK ADD  CONSTRAINT [FK_pf_Topic_pf_Forum] FOREIGN KEY([ForumID])
REFERENCES [dbo].[pf_Forum] ([ForumID])
ON DELETE CASCADE


CREATE UNIQUE NONCLUSTERED INDEX [IX_pf_Topic_UrlName] ON [dbo].[pf_Topic] 
(
	[UrlName] ASC
)

CREATE NONCLUSTERED INDEX [IX_pf_Topic_LastPostTime] ON [dbo].[pf_Topic]
(
	[LastPostTime] DESC
)




-- ******************************************************** pf_Post

CREATE TABLE [dbo].[pf_Post](
	[PostID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY NONCLUSTERED,
	[TopicID] [int] NOT NULL DEFAULT (0),
	[ParentPostID] [int] NOT NULL DEFAULT (0),
	[IP] [nvarchar](40) NOT NULL,
	[IsFirstInTopic] [bit] NOT NULL,
	[ShowSig] [bit] NOT NULL,
	[UserID] [int] NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Title] [nvarchar](256) NOT NULL,
	[FullText] nvarchar(MAX) NOT NULL,
	[PostTime] [datetime] NOT NULL,
	[IsEdited] [bit] NOT NULL,
	[LastEditName] [nvarchar](256) NOT NULL,
	[LastEditTime] [datetime] NULL,
	[IsDeleted] [bit] NOT NULL,
	[Votes] [int] DEFAULT 0 NOT NULL
) 

CREATE CLUSTERED INDEX [IX_pf_Post_TopicID] ON [dbo].[pf_Post] 
(
	[TopicID] ASC,
	[PostTime] ASC
) 

CREATE NONCLUSTERED INDEX [IX_pf_Post_PostTime] ON [dbo].[pf_Post] 
(
	[PostTime] DESC
) 

ALTER TABLE [dbo].[pf_Post]  WITH CHECK ADD  CONSTRAINT [FK_pf_Post_pf_Topic] FOREIGN KEY([TopicID])
REFERENCES [dbo].[pf_Topic] ([TopicID])
ON DELETE CASCADE

CREATE NONCLUSTERED INDEX [IX_pf_Post_UserID] ON [dbo].[pf_Post] 
(
	[UserID] ASC,
	[IsDeleted] ASC
) 

CREATE NONCLUSTERED INDEX [IX_pf_Post_IP_PostTime] ON [dbo].[pf_Post] 
(
	[IP] ASC, [PostTime] DESC
) 




-- ******************************************************** pf_Setting

CREATE TABLE [dbo].[pf_Setting] (
	[Setting] [nvarchar] (256) NOT NULL PRIMARY KEY,
	[Value] nvarchar(MAX) NOT NULL
) 





-- ******************************************************** pf_QueuedEmailMessage

CREATE TABLE [dbo].[pf_QueuedEmailMessage] (
	[MessageID] [int] IDENTITY (1, 1) NOT NULL,
	[FromEmail] [nvarchar] (256) NOT NULL,
	[FromName] [nvarchar] (256) NOT NULL,
	[ToEmail] [nvarchar] (256) NOT NULL,
	[ToName] [nvarchar] (256) NOT NULL,
	[Subject] [nvarchar] (256) NOT NULL,
	[Body] nvarchar(MAX) NOT NULL,
	[HtmlBody] nvarchar(MAX) NULL,
	[QueueTime] [datetime] NOT NULL
) 


ALTER TABLE [dbo].[pf_QueuedEmailMessage]
 ADD CONSTRAINT [PK_pf_QueuedEmailMessage] PRIMARY KEY CLUSTERED 
(
	[MessageID] ASC
)


CREATE NONCLUSTERED INDEX [IX_pf_QueuedEmailMessage_QueueTime] ON [dbo].[pf_QueuedEmailMessage] 
(
	[QueueTime] ASC
) 





-- ******************************************************** pf_ErrorLog

CREATE TABLE [dbo].[pf_ErrorLog] (
	[ErrorID] [int] IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED,
	[TimeStamp] [datetime] NULL ,
	[Message] nvarchar(MAX) NOT NULL ,
	[StackTrace] nvarchar(MAX) NOT NULL ,
	[Data] nvarchar(MAX) NOT NULL ,
	[Severity] [int] NOT NULL
) 








-- ******************************************************** pf_Friend

CREATE TABLE [dbo].[pf_Friend] (
	[FromUserID] [int] NOT NULL ,
	[ToUserID] [int] NOT NULL ,
	[IsApproved] [bit] NOT NULL
)

CREATE CLUSTERED INDEX [IX_Friend_FromUserID] ON [dbo].[pf_Friend] 
(
	[FromUserID] ASC
)

CREATE NONCLUSTERED INDEX [IX_Friend_ToUserID] ON [dbo].[pf_Friend] 
(
	[ToUserID] ASC
) 






-- ******************************************************** pf_ModerationLog

CREATE TABLE [dbo].[pf_ModerationLog](
	[ModerationID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,
	[TimeStamp] [datetime] NOT NULL,
	[UserID] [int] NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
	[ModerationType] [int] NOT NULL,
	[ForumID] [int] NULL,
	[TopicID] [int] NOT NULL,
	[PostID] [int] NULL,
	[Comment] nvarchar(MAX) NOT NULL,
	[OldText] nvarchar(MAX) NULL
) 


CREATE NONCLUSTERED INDEX [IX_pf_ModerationLog_TopicID] ON [dbo].[pf_ModerationLog] 
(
	[TopicID] ASC
) 


CREATE NONCLUSTERED INDEX [IX_pf_ModerationLog_PostID] ON [dbo].[pf_ModerationLog] 
(
	[PostID] ASC
) 





-- ******************************************************** pf_UserSession

CREATE TABLE [dbo].[pf_UserSession] (
	[SessionID] [int] NOT NULL,
	[UserID] [int] NULL, 
	[LastTime] [datetime] NOT NULL
) 


CREATE CLUSTERED INDEX [IX_pf_UserSession_SessionID] ON [dbo].[pf_UserSession] 
(
	[SessionID] ASC
) 


CREATE NONCLUSTERED INDEX [IX_pf_UserSession_UserID] ON [dbo].[pf_UserSession] 
(
	[UserID] ASC
) 





-- ******************************************************** pf_LastForumView

CREATE TABLE [dbo].[pf_LastForumView](
	[UserID] [int] NOT NULL,
	[ForumID] [int] NOT NULL,
	[LastForumViewDate] [datetime] NOT NULL
) 


CREATE CLUSTERED INDEX [IX_pf_LastForumView_UserID_ForumID] ON [dbo].[pf_LastForumView] 
(
	[UserID] ASC,
	[ForumID] ASC
) 


ALTER TABLE [dbo].[pf_LastForumView]  WITH CHECK ADD  CONSTRAINT [FK_pf_LastForumView_ForumID] FOREIGN KEY([ForumID])
REFERENCES [dbo].[pf_Forum] ([ForumID])
ON DELETE CASCADE

ALTER TABLE [dbo].[pf_LastForumView]  WITH CHECK ADD  CONSTRAINT [FK_pf_LastForumView_UserID] FOREIGN KEY([UserID])
REFERENCES [dbo].[pf_PopForumsUser] ([UserID])
ON DELETE CASCADE





-- ******************************************************** pf_UserImages

CREATE TABLE [dbo].[pf_UserImages](
	[UserImageID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY NONCLUSTERED,
	[UserID] [int] NOT NULL,
	[SortOrder] [int] NOT NULL,
	[IsApproved] [bit] NOT NULL,
	[TimeStamp] [datetime] NOT NULL,
	[ImageData] varbinary(MAX) NOT NULL
)


CREATE CLUSTERED INDEX [IX_UserImages_UserID] ON [dbo].[pf_UserImages] 
(
	[UserID] ASC
) 


ALTER TABLE [dbo].[pf_UserImages]  WITH CHECK ADD  CONSTRAINT [FK_pf_UserImages_pf_PopForumsUser] FOREIGN KEY([UserID])
REFERENCES [dbo].[pf_PopForumsUser] ([UserID])
ON DELETE CASCADE





-- ******************************************************** pf_UserAvatar

CREATE TABLE [dbo].[pf_UserAvatar](
	[UserAvatarID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY NONCLUSTERED,
	[UserID] [int] NOT NULL,
	[TimeStamp] [datetime] NOT NULL,
	[ImageData] varbinary(MAX) NOT NULL
)


CREATE CLUSTERED INDEX [IX_pf_UserAvatar_UserID] ON [dbo].[pf_UserAvatar] 
(
	[UserID] ASC
) 


ALTER TABLE [dbo].[pf_UserAvatar]  WITH CHECK ADD  CONSTRAINT [FK_pf_UserAvatar_pf_PopForumsUser] FOREIGN KEY([UserID])
REFERENCES [dbo].[pf_PopForumsUser] ([UserID])
ON DELETE CASCADE





-- ******************************************************** pf_ForumPostRestrictions

CREATE TABLE [dbo].[pf_ForumPostRestrictions](
	[ForumID] [int] NOT NULL,
	[Role] [nvarchar](256) NOT NULL
) 


CREATE CLUSTERED INDEX [IX_ForumPostRestrictions_ForumID] ON [dbo].[pf_ForumPostRestrictions] 
(
	[ForumID] ASC
) 


ALTER TABLE [dbo].[pf_ForumPostRestrictions]  WITH CHECK ADD  CONSTRAINT [FK_pf_ForumPostRestrictions_ForumID] FOREIGN KEY([ForumID])
REFERENCES [dbo].[pf_Forum] ([ForumID])
ON DELETE CASCADE

ALTER TABLE [dbo].[pf_ForumPostRestrictions]  WITH CHECK ADD  CONSTRAINT [FK_pf_ForumPostRestrictions_Role] FOREIGN KEY([Role])
REFERENCES [dbo].[pf_Role] ([Role])
ON DELETE CASCADE





-- ******************************************************** pf_ForumViewRestrictions

CREATE TABLE [dbo].[pf_ForumViewRestrictions](
	[ForumID] [int] NOT NULL,
	[Role] [nvarchar](256) NOT NULL
) 


CREATE CLUSTERED INDEX [IX_pf_ForumViewRestrictions_ForumID] ON [dbo].[pf_ForumViewRestrictions] 
(
	[ForumID] ASC
) 


ALTER TABLE [dbo].[pf_ForumViewRestrictions]  WITH CHECK ADD  CONSTRAINT [FK_pf_ForumViewRestrictions_ForumID] FOREIGN KEY([ForumID])
REFERENCES [dbo].[pf_Forum] ([ForumID])
ON DELETE CASCADE

ALTER TABLE [dbo].[pf_ForumViewRestrictions]  WITH CHECK ADD  CONSTRAINT [FK_pf_ForumViewRestrictions_Role] FOREIGN KEY([Role])
REFERENCES [dbo].[pf_Role] ([Role])
ON DELETE CASCADE






-- ******************************************************** pf_TopicSearchWords

CREATE TABLE [dbo].[pf_TopicSearchWords](
	[SearchWord] [nvarchar](256) NOT NULL,
	[TopicID] [int] NOT NULL,
	[Rank] [int] NOT NULL
) 


CREATE CLUSTERED INDEX [IX_TopicSearchWords_SearchWord_Rank] ON [dbo].[pf_TopicSearchWords] 
(
	[SearchWord] ASC,
	[Rank] DESC
) 


CREATE NONCLUSTERED INDEX [IX_TopicSearchWords_TopicID] ON [dbo].[pf_TopicSearchWords] 
(
	[TopicID] ASC
) 






-- ******************************************************** pf_JunkWords

CREATE TABLE [dbo].[pf_JunkWords](
	[JunkWord] [nvarchar](256) NOT NULL PRIMARY KEY
) 





-- ******************************************************** pf_Favorite

CREATE TABLE [dbo].[pf_Favorite](
	[UserID] [int] NOT NULL,
	[TopicID] [int] NOT NULL
) 


ALTER TABLE [dbo].[pf_Favorite]  WITH CHECK ADD  CONSTRAINT [FK_pf_Favorite_pf_PopForumsUser] FOREIGN KEY([UserID])
REFERENCES [dbo].[pf_PopForumsUser] ([UserID])
ON DELETE CASCADE


ALTER TABLE [dbo].[pf_Favorite]  WITH CHECK ADD  CONSTRAINT [FK_pf_Favorite_pf_Topic] FOREIGN KEY([TopicID])
REFERENCES [dbo].[pf_Topic] ([TopicID])
ON DELETE CASCADE


CREATE CLUSTERED INDEX [IX_pf_Favorite_UserID] ON [dbo].[pf_Favorite] 
(
	[UserID] ASC
)

CREATE NONCLUSTERED INDEX [IX_pf_Favorite_TopicID] ON [dbo].[pf_Favorite]
(
	[TopicID] ASC
)




-- ******************************************************** pf_SubscribeTopic

CREATE TABLE [dbo].[pf_SubscribeTopic](
	[UserID] [int] NOT NULL,
	[TopicID] [int] NOT NULL,
	[IsViewed] [bit] NOT NULL
) 


ALTER TABLE [dbo].[pf_SubscribeTopic]  WITH CHECK ADD  CONSTRAINT [FK_pf_SubscribeTopic_pf_PopForumsUser] FOREIGN KEY([UserID])
REFERENCES [dbo].[pf_PopForumsUser] ([UserID])
ON DELETE CASCADE


ALTER TABLE [dbo].[pf_SubscribeTopic]  WITH CHECK ADD  CONSTRAINT [FK_pf_SubscribeTopic_pf_Topic] FOREIGN KEY([TopicID])
REFERENCES [dbo].[pf_Topic] ([TopicID])
ON DELETE CASCADE


CREATE CLUSTERED INDEX [IX_pf_SubscribeTopic_TopicID_UserID] ON [dbo].[pf_SubscribeTopic] 
(
	[TopicID] ASC,
	[UserID] ASC
) 





-- ******************************************************** pf_LastTopicView

CREATE TABLE [dbo].[pf_LastTopicView](
	[UserID] [int] NOT NULL,
	[TopicID] [int] NOT NULL,
	[LastTopicViewDate] [datetime] NOT NULL
) 


ALTER TABLE [dbo].[pf_LastTopicView]  WITH CHECK ADD  CONSTRAINT [FK_pf_LastTopicView_TopicID] FOREIGN KEY([TopicID])
REFERENCES [dbo].[pf_Topic] ([TopicID])
ON DELETE CASCADE

ALTER TABLE [dbo].[pf_LastTopicView] CHECK CONSTRAINT [FK_pf_LastTopicView_TopicID]

ALTER TABLE [dbo].[pf_LastTopicView]  WITH CHECK ADD  CONSTRAINT [FK_pf_LastTopicView_UserID] FOREIGN KEY([UserID])
REFERENCES [dbo].[pf_PopForumsUser] ([UserID])
ON DELETE CASCADE

ALTER TABLE [dbo].[pf_LastTopicView] CHECK CONSTRAINT [FK_pf_LastTopicView_UserID]


CREATE CLUSTERED INDEX [IX_LastTopicVIew_UserID] ON [dbo].[pf_LastTopicView] 
(
	[UserID] ASC
) 

CREATE NONCLUSTERED INDEX [IX_pf_LastTopicView_TopicID] ON [dbo].[pf_LastTopicView]
(
	[TopicID] ASC
)



-- **************************************************** [pf_PrivateMessage]

CREATE TABLE [dbo].[pf_PrivateMessage](
	[PMID] [int] IDENTITY(1,1) NOT NULL,
	[Subject] [nvarchar](256) NOT NULL,
	[LastPostTime] [datetime] NOT NULL,
	[UserNames] [nvarchar](MAX) NOT NULL,
 CONSTRAINT [PK_pf_PrivateMessage] PRIMARY KEY CLUSTERED 
(
	[PMID] ASC
)
) 


CREATE TABLE [dbo].[pf_PrivateMessagePost](
	[PMPostID] [int] IDENTITY(1,1) NOT NULL,
	[PMID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[PostTime] [datetime] NOT NULL,
	[FullText] [nvarchar](MAX) NOT NULL
)
CREATE CLUSTERED INDEX [IX_pf_PrivateMessagePost_PMID] ON [dbo].[pf_PrivateMessagePost] 
(
	[PMID] ASC
)

ALTER TABLE [dbo].[pf_PrivateMessagePost]  WITH CHECK ADD  CONSTRAINT [FK_pf_PrivateMessagePost_pf_PrivateMessage] FOREIGN KEY([PMID])
REFERENCES [dbo].[pf_PrivateMessage] ([PMID])
ON DELETE CASCADE

ALTER TABLE [dbo].[pf_PrivateMessagePost] CHECK CONSTRAINT [FK_pf_PrivateMessagePost_pf_PrivateMessage]


CREATE TABLE [dbo].[pf_PrivateMessageUser](
	[PMID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[LastViewDate] [datetime] NOT NULL,
	[IsArchived] [bit] NOT NULL
)
CREATE CLUSTERED INDEX [IX_pf_PrivateMessageUser_PMID] ON [dbo].[pf_PrivateMessageUser] 
(
	[PMID] ASC
)


ALTER TABLE [dbo].[pf_PrivateMessageUser]  WITH CHECK ADD  CONSTRAINT [FK_pf_PrivateMessageUser_pf_PopForumsUser] FOREIGN KEY([UserID])
REFERENCES [dbo].[pf_PopForumsUser] ([UserID])
ON DELETE CASCADE

ALTER TABLE [dbo].[pf_PrivateMessageUser] CHECK CONSTRAINT [FK_pf_PrivateMessageUser_pf_PopForumsUser]

ALTER TABLE [dbo].[pf_PrivateMessageUser]  WITH CHECK ADD  CONSTRAINT [FK_pf_PrivateMessageUser_pf_PrivateMessage] FOREIGN KEY([PMID])
REFERENCES [dbo].[pf_PrivateMessage] ([PMID])
ON DELETE CASCADE

ALTER TABLE [dbo].[pf_PrivateMessageUser] CHECK CONSTRAINT [FK_pf_PrivateMessageUser_pf_PrivateMessage]


-- ******************************** [pf_PostVote]

CREATE TABLE [dbo].[pf_PostVote](
	[PostID] [int] NOT NULL,
	[UserID] [int] NOT NULL
) ON [PRIMARY]

CREATE CLUSTERED INDEX [IX_pf_PostVote_PostID] ON [dbo].[pf_PostVote] 
(
	[PostID] ASC
)



-- ***************************** [pf_Feed]

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


-- ***************************** [pf_PointLedger]
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


-- ******************************** [pf_EventDefinition]

CREATE TABLE [dbo].[pf_EventDefinition](
	[EventDefinitionID] [nvarchar](256) NOT NULL PRIMARY KEY CLUSTERED,
	[Description] [nvarchar](max) NOT NULL,
	[PointValue] [int] NOT NULL,
	[IsPublishedToFeed] [bit] NOT NULL
)


-- ****************************** [pf_AwardCalculationQueue]

CREATE TABLE [dbo].[pf_AwardCalculationQueue](
	[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,
	[Payload] [nvarchar](256) NOT NULL
)


-- ******************************* [pf_UserAward]

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



-- ***************************** [pf_AwardDefinition]

CREATE TABLE [dbo].[pf_AwardDefinition](
	[AwardDefinitionID] [nvarchar](256) NOT NULL PRIMARY KEY CLUSTERED,
	[Title] [nvarchar](256) NOT NULL,
	[Description] [nvarchar](256) NOT NULL,
	[IsSingleTimeAward] [bit] NOT NULL
)


-- ******************************* [pf_AwardCondition]

CREATE TABLE [dbo].[pf_AwardCondition](
	[AwardDefinitionID] [nvarchar](256) NOT NULL,
	[EventDefinitionID] [nvarchar](256) NOT NULL,
	[EventCount] [int] NOT NULL
)

CREATE CLUSTERED INDEX [IX_AwardCondition_EventDefinitionID] ON [dbo].[pf_AwardCondition] 
(
	[EventDefinitionID] ASC
)

ALTER TABLE [dbo].[pf_AwardCondition]  WITH CHECK ADD  CONSTRAINT [FK_pf_AwardCondition_pf_AwardDefinition] FOREIGN KEY([AwardDefinitionID])
REFERENCES [dbo].[pf_AwardDefinition] ([AwardDefinitionID])
ON DELETE CASCADE

ALTER TABLE [dbo].[pf_AwardCondition] CHECK CONSTRAINT [FK_pf_AwardCondition_pf_AwardDefinition]




-- ******************************* [pf_ExternalUserAssociation]

CREATE TABLE [dbo].[pf_ExternalUserAssociation](
	[ExternalUserAssociationID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY NONCLUSTERED,
	[UserID] [int] NOT NULL,
	[Issuer] [nvarchar](50) NOT NULL,
	[ProviderKey] [nvarchar](256) NOT NULL,
	[Name] [nvarchar](256) NOT NULL
)

CREATE NONCLUSTERED INDEX [IX_pf_ExternalUserAssociation_Issuer_ProviderKey] ON [dbo].[pf_ExternalUserAssociation]
(
	[Issuer] ASC,
	[ProviderKey] ASC
)

CREATE CLUSTERED INDEX [IX_pf_ExternalUserAssociation_UserID] ON [dbo].[pf_ExternalUserAssociation]
(
	[UserID] ASC
)




CREATE TABLE [dbo].[pf_EmailQueue](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Payload] [nvarchar](256) NOT NULL
)

CREATE CLUSTERED INDEX IX_pf_EmailQueue_Id ON pf_EmailQueue (Id)




CREATE TABLE [dbo].[pf_SearchQueue](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Payload] [nvarchar](256) NOT NULL
)

CREATE CLUSTERED INDEX IX_pf_SearchQueue_ID ON pf_SearchQueue (ID)



CREATE TABLE [dbo].[pf_ServiceHeartbeat](
	[ServiceName] [nvarchar](256) NOT NULL,
	[MachineName] [nvarchar](256) NOT NULL,
	[LastRun] [datetime] NOT NULL,
)



CREATE TABLE [dbo].[pf_TopicViewLog](
	[ID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY NONCLUSTERED,
	[UserID] [int] NULL,
	[TopicID] [int] NULL,
	[TimeStamp] [datetime] NOT NULL
)

CREATE CLUSTERED INDEX [IX_pf_TopicViewLog] ON [dbo].[pf_TopicViewLog]
(
	[TimeStamp] ASC
)





INSERT INTO pf_JunkWords (JunkWord) VALUES ('an')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('and')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('any')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('are')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('as')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('at')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('be')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('been')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('but')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('by')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('can')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('did')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('didn''t')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('do')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('does')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('don''t')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('for')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('from')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('gave')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('get')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('go')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('got')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('had')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('has')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('have')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('he')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('her')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('hers')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('here')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('his')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('i''d')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('if')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('in')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('is')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('it')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('its')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('it''s')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('i''ve')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('let''s')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('like')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('lot')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('me')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('my')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('no')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('not')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('of')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('or')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('our')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('out')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('say')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('says')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('she')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('so')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('some')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('such')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('than')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('that')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('that''s')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('the')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('their')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('there')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('they')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('the''ve')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('this')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('those')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('to')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('us')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('very')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('was')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('was''nt')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('way')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('we')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('went')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('were')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('what')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('where')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('which')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('who')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('why')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('with')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('would')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('you')
INSERT INTO pf_JunkWords (JunkWord) VALUES ('your')
