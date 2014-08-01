using Microsoft.AspNet.SignalR;
using PopForums.Services;
using PopForums.Web;

namespace PopForums.Messaging
{
	public class Recent : Hub
	{
		public Recent()
		{
			var container = PopForumsActivation.Container;
			_forumService = container.GetInstance<IForumService>();
			_userService = container.GetInstance<IUserService>();
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