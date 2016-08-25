using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;

namespace PopForums.Web.Messaging
{
	[HubName("Feed")]
	public class FeedHub : Hub
	{
	}
}