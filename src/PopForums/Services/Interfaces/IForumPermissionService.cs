namespace PopForums.Services.Interfaces;

public interface IForumPermissionService
{
    Task<ForumPermissionContext> GetPermissionContext(Forum forum, User user);

    Task<ForumPermissionContext> GetPermissionContext(Forum forum, User user, Topic topic);
}
