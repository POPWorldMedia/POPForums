IF COL_LENGTH('dbo.pf_PopForumsUser', 'SubscriptionExpiration') IS NULL
    BEGIN
        ALTER TABLE pf_PopForumsUser ADD [SubscriptionExpiration] [date] NULL;
    END

IF COL_LENGTH('dbo.pf_Profile', 'IsAutoRenewal') IS NULL
    BEGIN
        ALTER TABLE pf_Profile ADD [IsAutoRenewal] [bit] NOT NULL DEFAULT(0);
    END

IF COL_LENGTH('dbo.pf_Profile', 'Last4') IS NULL
    BEGIN
        ALTER TABLE pf_Profile ADD [Last4] [nvarchar](50) NULL;
    END

IF COL_LENGTH('dbo.pf_Profile', 'CustomerID') IS NULL
    BEGIN
        ALTER TABLE pf_Profile ADD [CustomerID] [nvarchar](256) NULL;
    END

IF COL_LENGTH('dbo.pf_Profile', 'SkuID') IS NULL
    BEGIN
        ALTER TABLE pf_Profile ADD [SkuID] [nvarchar](256) NULL;
    END

IF OBJECT_ID('pf_Sku', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[pf_Sku](
          [SkuID] [nvarchar](256) NOT NULL PRIMARY KEY CLUSTERED,
          [Name] [nvarchar](256) NOT NULL,
          [Description] [nvarchar](MAX) NOT NULL,
          [Price] [decimal](18, 2) NOT NULL,
          [IsActive] [bit] NOT NULL,
          [Months] [smallint] NOT NULL
        );
    END

IF OBJECT_ID('pf_Transaction', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[pf_Transaction](
          [TransactionID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,
          [ProcessorID] [nvarchar](256) NOT NULL,
          [CustomerID] [nvarchar](256) NOT NULL,
          [Status] [nvarchar](256) NOT NULL,
          [Raw] [nvarchar](MAX) NOT NULL,
          [Last4] [nvarchar](50) NOT NULL,
          [UserID] [int] NOT NULL,
          [TimeStamp] [datetime] NOT NULL,
          [SkuID] [nvarchar](256) NOT NULL,
          [Amount] [decimal](18, 2) NOT NULL
        );
    END
IF INDEXPROPERTY(Object_Id('pf_Transaction'), 'IX_pf_Transaction_UserID', 'IndexID') IS NULL
    BEGIN
        CREATE NONCLUSTERED INDEX IX_pf_Transaction_UserID ON pf_Transaction (UserID, TimeStamp);
    END

IF OBJECT_ID('pf_SubscriptionHistory', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[pf_SubscriptionHistory](
          [SubscriptionHistoryID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,
          [UserID] [int] NOT NULL,
          [TimeStamp] [datetime] NOT NULL,
          [SkuID] [nvarchar](256) NOT NULL,
          [Message] [nvarchar](MAX) NOT NULL
        );
    END
IF INDEXPROPERTY(Object_Id('pf_SubscriptionHistory'), 'IX_pf_SubscriptionHistory_UserID', 'IndexID') IS NULL
    BEGIN
        CREATE NONCLUSTERED INDEX IX_pf_SubscriptionHistory_UserID ON pf_SubscriptionHistory (UserID, TimeStamp);
    END

IF OBJECT_ID('pf_RenewalQueue', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[pf_RenewalQueue](
          [Id] [int] IDENTITY(1,1) NOT NULL,
          [Payload] [nvarchar](256) NOT NULL
        );
        CREATE CLUSTERED INDEX IX_pf_RenewalQueue_Id ON pf_RenewalQueue (Id);
    END

IF OBJECT_ID('pf_RenewalEnqueueClaim', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[pf_RenewalEnqueueClaim](
          [ClaimDate] [date] NOT NULL
        );
        INSERT INTO pf_RenewalEnqueueClaim (ClaimDate) VALUES ('1900-01-01');
    END

IF NOT EXISTS (SELECT 1 FROM pf_Role WHERE Role = 'Subscriber')
    BEGIN
        INSERT INTO pf_Role (Role) VALUES ('Subscriber');
    END
