IF OBJECT_ID('pf_Ignore', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[pf_Ignore](
          [UserID] [int] NOT NULL,
          [IgnoreUserID] [int] NOT NULL
        );
    END
IF INDEXPROPERTY(Object_Id('pf_Ignore'), 'IX_pf_Ignore_UserID', 'IndexID') IS NULL
    BEGIN
        CREATE CLUSTERED INDEX IX_pf_Ignore_UserID ON pf_Ignore (UserID, IgnoreUserID);
    END
