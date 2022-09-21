namespace PopForums.Mvc.Areas.Forums.Messaging;

public class PopForumsHub : Hub
{
	private readonly ITenantService _tenantService;
	private readonly IUserService _userService;
	private readonly IForumService _forumService;
	private readonly ITopicService _topicService;

	public PopForumsHub(ITenantService tenantService, IUserService userService, IForumService forumService, ITopicService topicService)
	{
		_tenantService = tenantService;
		_userService = userService;
		_forumService = forumService;
		_topicService = topicService;
	}

	// *** Forums

	public void ListenToAllForums()
	{
		var tenant = _tenantService.GetTenant();
		Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:forum:all");
	}

	public void ListenToForum(int forumID)
	{
		var tenant = _tenantService.GetTenant();
		Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:forum:{forumID}");
	}

	// *** Recent

	public void ListenRecent()
	{
		var tenant = _tenantService.GetTenant();
		Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:recent:all");
		var principal = Context.User;
		if (principal != null && principal.Identity != null)
		{
			var user = _userService.GetUserByName(principal.Identity.Name).Result;
			var visibleForumIDs = _forumService.GetViewableForumIDsFromViewRestrictedForums(user).Result;
			foreach (var forumID in visibleForumIDs)
				Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:recent:{forumID}");
		}
	}

	// *** Topics

	public void ListenToTopic(int topicID)
	{
		var tenant = _tenantService.GetTenant();
		Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:topic:{topicID}");
	}

	public int GetLastPostID(int topicID)
	{
		return _topicService.TopicLastPostID(topicID).Result;
	}
}