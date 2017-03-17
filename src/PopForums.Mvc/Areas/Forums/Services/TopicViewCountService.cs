using Microsoft.AspNetCore.Http;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Mvc.Areas.Forums.Services
{
	public interface ITopicViewCountService
	{
		void ProcessView(Topic topic, HttpContext context);
		void SetViewedTopic(Topic topic, HttpContext context);
	}

	// TODO: Test that this works
	public class TopicViewCountService : ITopicViewCountService
	{
		public TopicViewCountService(ITopicRepository topicRepository)
		{
			_topicRepository = topicRepository;
		}

		private readonly ITopicRepository _topicRepository;
		private const string CookieKey = "PopForums.LastTopicID";

		public void ProcessView(Topic topic, HttpContext context)
		{
			if (context.Request.Cookies.ContainsKey(CookieKey))
			{
				int topicID;
				if (int.TryParse(context.Request.Cookies[CookieKey], out topicID))
				{
					if (topicID != topic.TopicID)
						_topicRepository.IncrementViewCount(topic.TopicID);
				}
			}
			else
				_topicRepository.IncrementViewCount(topic.TopicID);
			SetViewedTopic(topic, context);
		}

		public void SetViewedTopic(Topic topic, HttpContext context)
		{
			context.Response.Cookies.Append(CookieKey, topic.TopicID.ToString());
		}
	}
}
