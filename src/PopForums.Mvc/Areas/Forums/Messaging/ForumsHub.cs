using Microsoft.AspNetCore.SignalR;

namespace PopForums.Mvc.Areas.Forums.Messaging
{
	public class ForumsHub : Hub
	{
		public void ListenTo(int forumID)
		{
			Groups.AddAsync(Context.ConnectionId, forumID.ToString());
		}
	}
}