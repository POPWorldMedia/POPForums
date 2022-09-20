namespace PopForums.Mvc.Areas.Forums.Messaging;

public class PopForumsHub : Hub
{
	private readonly ITenantService _tenantService;

	public PopForumsHub(ITenantService tenantService)
	{
		_tenantService = tenantService;
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
}