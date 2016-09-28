IF OBJECT_ID('pf_EmailQueue', 'U') IS NULL
BEGIN
CREATE TABLE [dbo].[pf_EmailQueue](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Payload] [nvarchar](256) NOT NULL
) ON [PRIMARY]

CREATE CLUSTERED INDEX IX_pf_EmailQueue_Id ON pf_EmailQueue (Id)
END



IF OBJECT_ID('pf_SearchQueue', 'U') IS NULL
BEGIN
CREATE TABLE [dbo].[pf_SearchQueue](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TopicID] [int] NOT NULL
)

CREATE CLUSTERED INDEX IX_pf_SearchQueue_ID ON pf_SearchQueue (ID)
END



IF OBJECT_ID('pf_ServiceHeartbeat', 'U') IS NULL
BEGIN
CREATE TABLE [dbo].[pf_ServiceHeartbeat](
	[ServiceName] [nvarchar](256) NOT NULL,
	[MachineName] [nvarchar](256) NOT NULL,
	[LastRun] [datetime] NOT NULL,
 CONSTRAINT [PK_pf_ServiceHeartbeat] PRIMARY KEY CLUSTERED 
(
	[ServiceName] ASC,
	[MachineName] ASC
)
)
END
