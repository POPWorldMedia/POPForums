using Microsoft.AspNetCore.Http;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Services
{
	public class TopicViewCountService : ITopicViewCountService
	{
		public TopicViewCountService(ITopicRepository topicRepository, IHttpContextAccessor httpContextAccessor)
		{
			_topicRepository = topicRepository;
			_httpContextAccessor = httpContextAccessor;
		}

		private readonly ITopicRepository _topicRepository;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private const string CookieKey = "PopForums.LastTopicID";

		public void ProcessView(Topic topic)
		{
			var context = _httpContextAccessor.HttpContext;
			if (context.Request.Cookies.ContainsKey(CookieKey))
			{
				if (int.TryParse(context.Request.Cookies[CookieKey], out var topicID))
				{
					if (topicID != topic.TopicID)
						_topicRepository.IncrementViewCount(topic.TopicID);
				}
			}
			else
				_topicRepository.IncrementViewCount(topic.TopicID);
			SetViewedTopic(topic);
		}

		public void SetViewedTopic(Topic topic)
		{
			_httpContextAccessor.HttpContext.Response.Cookies.Append(CookieKey, topic.TopicID.ToString());
		}
	}
}
