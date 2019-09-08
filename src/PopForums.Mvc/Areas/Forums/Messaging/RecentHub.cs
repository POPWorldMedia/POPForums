using Microsoft.AspNetCore.SignalR;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Messaging
{
	public class RecentHub : Hub
	{
		public RecentHub(IForumService forumService, IUserService userService, ITenantService tenantService)
		{
			_forumService = forumService;
			_userService = userService;
			_tenantService = tenantService;
		}

		private readonly IForumService _forumService;
		private readonly IUserService _userService;
		private readonly ITenantService _tenantService;

		public void Register()
		{
			var tenant = _tenantService.GetTenant();
			Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:forum:all");
			var principal = Context.User;
			if (principal != null)
			{
				var user = _userService.GetUserByName(principal.Identity.Name).Result;
				var visibleForumIDs = _forumService.GetViewableForumIDsFromViewRestrictedForums(user).Result;
				foreach (var forumID in visibleForumIDs)
					Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:forum:{forumID}");
			}
		}
	}
}