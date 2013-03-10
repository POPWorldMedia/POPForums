using System.Web;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public class TopicViewCountService : ITopicViewCountService
	{
		public TopicViewCountService(ITopicRepository topicRepository)
		{
			_topicRepository = topicRepository;
		}

		private readonly ITopicRepository _topicRepository;
		private const string CookieKey = "PopForums.LastTopicID";

		public void ProcessView(Topic topic, HttpContextBase context)
		{
			if (context.Request.Cookies[CookieKey] != null)
			{
				int topicID;
				if (int.TryParse(context.Request.Cookies[CookieKey].Value, out topicID))
				{
					if (topicID != topic.TopicID)
						_topicRepository.IncrementViewCount(topic.TopicID);
				}
			}
			else
				_topicRepository.IncrementViewCount(topic.TopicID);
			SetViewedTopic(topic, context);
		}

		public void SetViewedTopic(Topic topic, HttpContextBase context)
		{
			var newCookie = new HttpCookie(CookieKey) {Value = topic.TopicID.ToString()};
			context.Response.Cookies.Set(newCookie);
		}
	}
}
