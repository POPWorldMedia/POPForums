using Microsoft.AspNetCore.SignalR;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Messaging
{
	public class ForumsHub : Hub
	{
		private readonly ITenantService _tenantService;

		public ForumsHub(ITenantService tenantService)
		{
			_tenantService = tenantService;
		}

		public void ListenToAll()
		{
			var tenant = _tenantService.GetTenant();
			Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:all");
		}

		public void ListenTo(int forumID)
		{
			var tenant = _tenantService.GetTenant();
			Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:{forumID}");
		}
	}
}