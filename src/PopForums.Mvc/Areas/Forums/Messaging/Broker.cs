namespace PopForums.Mvc.Areas.Forums.Messaging;

public class Broker : IBroker
{
	public Broker(IForumRepository forumRepo, IHubContext<TopicsHub> topicHubContext, IHubContext<FeedHub> feedHubContext, IHubContext<ForumsHub> forumsHubContext, IHubContext<RecentHub> recentHubContext, IHubContext<NotificationHub> notificationHubContext, ITenantService tenantService)
	{
		_forumRepo = forumRepo;
		_topicHubContext = topicHubContext;
		_feedHubContext = feedHubContext;
		_forumsHubContext = forumsHubContext;
		_recentHubContext = recentHubContext;
		_notificationHubContext = notificationHubContext;
		_tenantService = tenantService;
	}
	
	private readonly IForumRepository _forumRepo;
	private readonly IHubContext<TopicsHub> _topicHubContext;
	private readonly IHubContext<FeedHub> _feedHubContext;
	private readonly IHubContext<ForumsHub> _forumsHubContext;
	private readonly IHubContext<RecentHub> _recentHubContext;
	private readonly IHubContext<NotificationHub> _notificationHubContext;
	private readonly ITenantService _tenantService;

	public void NotifyNewPosts(Topic topic, int lasPostID)
	{
		var tenant = _tenantService.GetTenant();
		_topicHubContext.Clients.Group($"{tenant}:{topic.TopicID}").SendAsync("notifyNewPosts", lasPostID);
	}

	public void NotifyNewPost(Topic topic, int postID)
	{
		var tenant = _tenantService.GetTenant();
		_topicHubContext.Clients.Group($"{tenant}:{topic.TopicID}").SendAsync("fetchNewPost", postID);
	}

	public void NotifyFeed(string message)
	{
		var tenant = _tenantService.GetTenant();
		var data = new { Message = message, Utc = new DateTime(DateTime.UtcNow.Ticks, DateTimeKind.Unspecified).ToString("o"), TimeStamp = Resources.LessThanMinute };
		_feedHubContext.Clients.Group($"{tenant}:feed").SendAsync("notifyFeed", data);
	}

	public void NotifyForumUpdate(Forum forum)
	{
		var tenant = _tenantService.GetTenant();
		_forumsHubContext.Clients.Group($"{tenant}:all").SendAsync("notifyForumUpdate", new { forum.ForumID, TopicCount = forum.TopicCount.ToString("N0"), PostCount = forum.PostCount.ToString("N0"), forum.LastPostName, Utc = forum.LastPostTime.ToString("o") });
	}

	public void NotifyTopicUpdate(Topic topic, Forum forum, string topicLink)
	{
		var tenant = _tenantService.GetTenant();
		var isForumViewRestricted = _forumRepo.GetForumViewRoles(forum.ForumID).Result.Count > 0;
		var result = new
		{
			Link = topicLink,
			topic.TopicID,
			topic.StartedByName,
			topic.Title,
			ForumTitle = forum.Title,
			topic.ViewCount,
			topic.ReplyCount,
			Utc = topic.LastPostTime.ToString("o"),
			topic.LastPostName
		};
		if (isForumViewRestricted)
			_recentHubContext.Clients.Group($"{tenant}:forum:{forum.ForumID}").SendAsync("notifyRecentUpdate", result);
		else
			_recentHubContext.Clients.Group($"{tenant}:forum:all").SendAsync("notifyRecentUpdate", result);
		_forumsHubContext.Clients.Group($"{tenant}:{forum.ForumID}").SendAsync("notifyUpdatedTopic", result);
	}

	public async void NotifyPMCount(int userID, int pmCount)
	{
		var tenantID = _tenantService.GetTenant();
		var userIDString = PopForumsUserIdProvider.FormatUserID(tenantID, userID);
		await _notificationHubContext.Clients.User(userIDString).SendAsync("updatePMCount", pmCount);
	}

	public async void NotifyUser(Notification notification)
	{
		var tenantID = _tenantService.GetTenant();
		var userIDString = PopForumsUserIdProvider.FormatUserID(tenantID, notification.UserID);
		await _notificationHubContext.Clients.User(userIDString).SendAsync("notify", notification);
	}
}