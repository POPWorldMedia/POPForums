IF COL_LENGTH('dbo.pf_PopForumsUser', 'TokenExpiration') IS NULL
    BEGIN
        ALTER TABLE pf_PopForumsUser ADD [TokenExpiration] [datetime] NULL;
    END

IF COL_LENGTH('dbo.pf_UserActivity', 'RefreshToken') IS NULL
    BEGIN
        ALTER TABLE pf_UserActivity ADD [RefreshToken] [nvarchar](MAX) NULL;
    END