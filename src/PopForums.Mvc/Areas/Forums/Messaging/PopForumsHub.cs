namespace PopForums.Mvc.Areas.Forums.Messaging;

public class PopForumsHub : Hub
{
	private readonly ITenantService _tenantService;
	private readonly IUserService _userService;
	private readonly IForumService _forumService;

	public PopForumsHub(ITenantService tenantService, IUserService userService, IForumService forumService)
	{
		_tenantService = tenantService;
		_userService = userService;
		_forumService = forumService;
	}

	// *** Forums

	public void ListenToAllForums()
	{
		var tenant = _tenantService.GetTenant();
		Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:all");
	}

	public void ListenToForum(int forumID)
	{
		var tenant = _tenantService.GetTenant();
		Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:{forumID}");
	}

	// *** Recent

	public void ListenRecent()
	{
		var tenant = _tenantService.GetTenant();
		Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:forum:all");
		var principal = Context.User;
		if (principal != null && principal.Identity != null)
		{
			var user = _userService.GetUserByName(principal.Identity.Name).Result;
			var visibleForumIDs = _forumService.GetViewableForumIDsFromViewRestrictedForums(user).Result;
			foreach (var forumID in visibleForumIDs)
				Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:forum:{forumID}");
		}
	}
}