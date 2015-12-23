using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace PopForums.Web.Areas.Forums.Messaging
{
	[HubName("Forums")]
	public class ForumsHub : Hub
	{
		public void ListenTo(int forumID)
		{
			Groups.Add(Context.ConnectionId, forumID.ToString());
		}
	}
}