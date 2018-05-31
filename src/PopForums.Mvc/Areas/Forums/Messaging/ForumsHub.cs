using Microsoft.AspNetCore.SignalR;

namespace PopForums.Mvc.Areas.Forums.Messaging
{
	public class ForumsHub : Hub
	{
		public void ListenTo(int forumID)
		{
			Groups.AddToGroupAsync(Context.ConnectionId, forumID.ToString());
		}
	}
}