using Microsoft.AspNet.SignalR;
using Ninject;
using PopForums.Services;
using PopForums.Web;

namespace PopForums.Messaging
{
	public class Topics : Hub
	{
		public Topics()
		{
			var container = PopForumsActivation.Kernel;
			_topicService = container.Get<ITopicService>();
		}

		public Topics(ITopicService topicService)
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