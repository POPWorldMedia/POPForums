using Microsoft.AspNet.SignalR;
using Ninject;
using PopForums.Services;
using PopForums.Web;

namespace PopForums.Messaging
{
	public class Recent : Hub
	{
		public Recent()
		{
			var container = PopForumsActivation.Kernel;
			_forumService = container.Get<IForumService>();
			_userService = container.Get<IUserService>();
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