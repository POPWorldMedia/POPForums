using PopForums.Mvc.Areas.Forums.Authentication;

namespace PopForums.Mvc.Areas.Forums.Messaging;

public class PopForumsHub : Hub
{
	private readonly ITenantService _tenantService;
	private readonly IUserService _userService;
	private readonly IForumService _forumService;
	private readonly ITopicService _topicService;
	private readonly INotificationManager _notificationManager;
	private readonly IPrivateMessageService _privateMessageService;

	public PopForumsHub(ITenantService tenantService, IUserService userService, IForumService forumService, ITopicService topicService, INotificationManager notificationManager, IPrivateMessageService privateMessageService)
	{
		_tenantService = tenantService;
		_userService = userService;
		_forumService = forumService;
		_topicService = topicService;
		_notificationManager = notificationManager;
		_privateMessageService = privateMessageService;
	}

	// *** Forums

	public void ListenToAllForums()
	{
		var tenant = _tenantService.GetTenant();
		Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:forum:all");
	}

	public void ListenToForum(int forumID)
	{
		var tenant = _tenantService.GetTenant();
		Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:forum:{forumID}");
	}

	// *** Recent

	public void ListenRecent()
	{
		var tenant = _tenantService.GetTenant();
		Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:recent:all");
		var principal = Context.User;
		if (principal != null && principal.Identity != null)
		{
			var user = _userService.GetUserByName(principal.Identity.Name).Result;
			var visibleForumIDs = _forumService.GetViewableForumIDsFromViewRestrictedForums(user).Result;
			foreach (var forumID in visibleForumIDs)
				Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:recent:{forumID}");
		}
	}

	// *** Topics

	public void ListenToTopic(int topicID)
	{
		var tenant = _tenantService.GetTenant();
		Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:topic:{topicID}");
	}

	public int GetLastPostID(int topicID)
	{
		return _topicService.TopicLastPostID(topicID).Result;
	}

	// *** Notifications

	private int GetUserID()
	{
		var userIDstring = Context.User?.Claims.FirstOrDefault(x => x.Type == PopForumsAuthenticationDefaults.ForumsUserIDType);
		if (userIDstring == null)
			throw new Exception("No forum user ID claim found in hub context of User");
		var userID = Convert.ToInt32(userIDstring.Value);
		return userID;
	}

	public async Task MarkNotificationRead(long contextID, int notificationType)
	{
		var notificationTypeEnum = (NotificationType)notificationType;
		var userID = GetUserID();
		await _notificationManager.MarkNotificationRead(userID, notificationTypeEnum, contextID);
	}

	public async Task MarkAllRead()
	{
		var userID = GetUserID();
		await _notificationManager.MarkAllRead(userID);
	}

	public async Task<List<Notification>> GetNotifications(DateTime afterDateTime)
	{
		var userID = GetUserID();
		var notifications = await _notificationManager.GetNotifications(userID, afterDateTime);
		return notifications;
	}

	public async Task<int> GetNotificationCount()
	{
		var userID = GetUserID();
		var count = await _notificationManager.GetUnreadNotificationCount(userID);
		return count;
	}

	public async Task<int> GetPMCount()
	{
		var userID = GetUserID();
		var count = await _privateMessageService.GetUnreadCount(userID);
		return count;
	}

	// *** Private Messages

	public async Task ListenToPm(int pmID)
	{
		var userID = GetUserID();
		if (!await _privateMessageService.IsUserInPM(userID, pmID))
			return;
		var tenant = _tenantService.GetTenant();
		await Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:pm:{pmID}");
	}

	public async Task SendPm(int pmID, string fullText)
	{
		var userID = GetUserID();
		if (!await _privateMessageService.IsUserInPM(userID, pmID))
			return;
		var pm = await _privateMessageService.Get(pmID, userID);
		var user = await _userService.GetUser(userID);
		await _privateMessageService.Reply(pm, fullText, user);
	}

	public async Task AckReadPm(int pmID)
	{
		var userID = GetUserID();
		await _privateMessageService.MarkPMRead(userID, pmID);
	}

	public async Task<ClientPrivateMessagePost[]> GetPmPosts(int pmID, DateTime beforeDateTime)
	{
		var userID = GetUserID();
		if (!await _privateMessageService.IsUserInPM(userID, pmID))
		{
			return null;
		}
		var posts = await _privateMessageService.GetPosts(pmID, beforeDateTime);
		var clientMessages = ClientPrivateMessagePost.MapForClient(posts);
		return clientMessages;
	}

	public async Task<ClientPrivateMessagePost[]> GetMostRecentPmPosts(int pmID, DateTime afterDateTime)
	{
		var userID = GetUserID();
		if (!await _privateMessageService.IsUserInPM(userID, pmID))
		{
			return null;
		}
		var posts = await _privateMessageService.GetMostRecentPosts(pmID, afterDateTime);
		var clientMessages = ClientPrivateMessagePost.MapForClient(posts);
		return clientMessages;
	}
}