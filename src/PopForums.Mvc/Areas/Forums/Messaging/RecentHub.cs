using Microsoft.AspNetCore.SignalR;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Messaging
{
	public class RecentHub : Hub
	{
		public RecentHub(IForumService forumService, IUserService userService)
		{
			_forumService = forumService;
			_userService = userService;
		}

		private readonly IForumService _forumService;
		private readonly IUserService _userService;

		public void Register()
		{
			var principal = Context.User;
			if (principal != null)
			{
				var user = _userService.GetUserByName(principal.Identity.Name);
				var visibleForumIDs = _forumService.GetViewableForumIDsFromViewRestrictedForums(user);
				foreach (var forumID in visibleForumIDs)
					Groups.AddAsync(Context.ConnectionId, "forum" + forumID);
			}
		}
	}
}