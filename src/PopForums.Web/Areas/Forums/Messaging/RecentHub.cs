using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using PopForums.Services;

namespace PopForums.Web.Areas.Forums.Messaging
{
	[HubName("Recent")]
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
			var id = Context.User;
			if (id != null)
			{
				var user = _userService.GetUserByName(id.Identity.Name);
				var visibleForumIDs = _forumService.GetViewableForumIDsFromViewRestrictedForums(user);
				foreach (var forumID in visibleForumIDs)
					Groups.Add(Context.ConnectionId, "forum" + forumID);
			}
		}
	}
}