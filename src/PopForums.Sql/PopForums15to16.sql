DROP INDEX [IX_PopForumsUser_UserName] ON [dbo].[pf_PopForumsUser]
CREATE UNIQUE NONCLUSTERED INDEX [IX_PopForumsUser_UserName] ON [dbo].[pf_PopForumsUser]([Name])
DROP INDEX [IX_PopForumsUser_Email] ON [dbo].[pf_PopForumsUser]
CREATE UNIQUE NONCLUSTERED INDEX [IX_PopForumsUser_Email] ON [dbo].[pf_PopForumsUser]([Email])
