using Microsoft.AspNet.SignalR;

namespace PopForums.Messaging
{
	public class Forums : Hub
	{
		public void ListenTo(int forumID)
		{
			Groups.Add(Context.ConnectionId, forumID.ToString());
		}
	}
}