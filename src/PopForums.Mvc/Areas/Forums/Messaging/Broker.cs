namespace PopForums.Mvc.Areas.Forums.Messaging;

public class Broker : IBroker
{
	public Broker(IForumRepository forumRepo, ITenantService tenantService, IHubContext<PopForumsHub> popForumsHubContext)
	{
		_forumRepo = forumRepo;
		_tenantService = tenantService;
		_popForumsHubContext = popForumsHubContext;
	}
	
	private readonly IForumRepository _forumRepo;
	private readonly ITenantService _tenantService;
	private readonly IHubContext<PopForumsHub> _popForumsHubContext;

	public void NotifyNewPosts(Topic topic, int lasPostID)
	{
		var tenant = _tenantService.GetTenant();
		_popForumsHubContext.Clients.Group($"{tenant}:topic:{topic.TopicID}").SendAsync("notifyNewPosts", lasPostID);
	}

	public void NotifyNewPost(Topic topic, int postID)
	{
		var tenant = _tenantService.GetTenant();
		_popForumsHubContext.Clients.Group($"{tenant}:topic:{topic.TopicID}").SendAsync("fetchNewPost", postID);
	}

	public void NotifyForumUpdate(Forum forum)
	{
		var tenant = _tenantService.GetTenant();
		_popForumsHubContext.Clients.Group($"{tenant}:forum:all").SendAsync("notifyForumUpdate", new { forum.ForumID, TopicCount = forum.TopicCount.ToString("N0"), PostCount = forum.PostCount.ToString("N0"), forum.LastPostName, Utc = forum.LastPostTime.ToString("o") });
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
			_popForumsHubContext.Clients.Group($"{tenant}:recent:{forum.ForumID}").SendAsync("notifyRecentUpdate", result);
		else
			_popForumsHubContext.Clients.Group($"{tenant}:recent:all").SendAsync("notifyRecentUpdate", result);
		_popForumsHubContext.Clients.Group($"{tenant}:forum:{forum.ForumID}").SendAsync("notifyUpdatedTopic", result);
	}

	public async void NotifyPMCount(int userID, int pmCount)
	{
		var tenantID = _tenantService.GetTenant();
		var userIDString = PopForumsUserIdProvider.FormatUserID(tenantID, userID);
		await _popForumsHubContext.Clients.User(userIDString).SendAsync("updatePMCount", pmCount);
	}

	public async void NotifyUser(Notification notification)
	{
		var tenantID = _tenantService.GetTenant();
		var userIDString = PopForumsUserIdProvider.FormatUserID(tenantID, notification.UserID);
		await _popForumsHubContext.Clients.User(userIDString).SendAsync("notify", notification);
	}

	public async void NotifyUser(Notification notification, string tenantID)
	{
		var userIDString = PopForumsUserIdProvider.FormatUserID(tenantID, notification.UserID);
		await _popForumsHubContext.Clients.User(userIDString).SendAsync("notify", notification);
	}

	public async void SendPMMessage(PrivateMessagePost post)
	{
		var message = ClientPrivateMessagePost.MapForClient(post);
		var tenantID = _tenantService.GetTenant();
		await _popForumsHubContext.Clients.Group($"{tenantID}:pm:{post.PMID}").SendAsync("addMessage", message);
	}
}