using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Messaging
{
	public class TopicsHub : Hub
	{
		public TopicsHub(ITopicService topicService, ITenantService tenantService)
		{
			_topicService = topicService;
			_tenantService = tenantService;
		}

		private readonly ITopicService _topicService;
		private readonly ITenantService _tenantService;

		public void ListenTo(int topicID)
		{
			var tenant = _tenantService.GetTenant();
			Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:{topicID}");
		}

		public int GetLastPostID(int topicID)
		{
			return _topicService.TopicLastPostID(topicID).Result;
		}
	}
}