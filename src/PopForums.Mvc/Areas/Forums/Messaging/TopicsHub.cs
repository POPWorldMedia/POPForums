using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Messaging
{
	[HubName("Topics")]
	public class TopicsHub : Hub
	{
		public TopicsHub(ITopicService topicService)
		{
			_topicService = topicService;
		}

		private readonly ITopicService _topicService;

		public void ListenTo(int topicID)
		{
			Groups.Add(Context.ConnectionId, topicID.ToString());
		}

		public int GetLastPostID(int topicID)
		{
			return _topicService.TopicLastPostID(topicID);
		}
	}
}