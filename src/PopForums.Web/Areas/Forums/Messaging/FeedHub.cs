using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace PopForums.Web.Areas.Forums.Messaging
{
	[HubName("Feed")]
	public class FeedHub : Hub
	{
	}
}