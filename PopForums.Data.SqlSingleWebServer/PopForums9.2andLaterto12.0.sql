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

ALTER TABLE [pf_PopForumsUser]
ADD [Salt] [uniqueidentifier] NULL
