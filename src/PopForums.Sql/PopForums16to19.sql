-- For recent users view in admin
IF INDEXPROPERTY(Object_Id('pf_SecurityLog'), 'IX_pf_SecurityLog_TargetUserID_SecurityLogType', 'IndexID') IS NULL
BEGIN
	CREATE NONCLUSTERED INDEX IX_pf_SecurityLog_TargetUserID_SecurityLogType 
	ON pf_SecurityLog
	(
		TargetUserID DESC,
		SecurityLogType
	);
END


DROP INDEX IF EXISTS [IX_Friend_ToUserID] ON [pf_Friend];
DROP INDEX IF EXISTS [IX_Friend_FromUserID] ON [pf_Friend];
IF OBJECT_ID('pf_Friend', 'U') IS NOT NULL
BEGIN
	DROP TABLE pf_Friend;
END