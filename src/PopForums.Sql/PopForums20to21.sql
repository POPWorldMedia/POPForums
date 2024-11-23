IF COL_LENGTH('dbo.pf_Profile', 'Twitter') IS NOT NULL
    BEGIN
        ALTER TABLE pf_Profile DROP COLUMN [Twitter];
    END
