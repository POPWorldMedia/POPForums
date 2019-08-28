using Microsoft.AspNetCore.SignalR;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Messaging
{
	public class TopicsHub : Hub
	{
		public TopicsHub(ITopicService topicService)
		{
			_topicService = topicService;
		}

		private readonly ITopicService _topicService;

		public void ListenTo(int topicID)
		{
			Groups.AddToGroupAsync(Context.ConnectionId, topicID.ToString());
		}

		public int GetLastPostID(int topicID)
		{
			return _topicService.TopicLastPostID(topicID).Result;
		}
	}
}