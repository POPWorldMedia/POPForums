using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;

namespace PopForums.Mvc.Areas.Forums.Messaging
{
	[HubName("Feed")]
	public class FeedHub : Hub
	{
	}
}