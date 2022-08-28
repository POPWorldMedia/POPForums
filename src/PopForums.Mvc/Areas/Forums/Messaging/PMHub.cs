namespace PopForums.Mvc.Areas.Forums.Messaging;

public class PMHub : Hub
{
	private readonly IPrivateMessageService _privateMessageService;
	private readonly IUserService _userService;
	private readonly ITenantService _tenantService;

	public PMHub(IPrivateMessageService privateMessageService, IUserService userService, ITenantService tenantService)
	{
		_privateMessageService = privateMessageService;
		_userService = userService;
		_tenantService = tenantService;
	}

	private int GetUserID()
	{
		var userIDstring = Context.User?.Claims?.Single(x => x.Type == PopForumsAuthorizationDefaults.ForumsUserIDType);
		if (userIDstring == null)
			throw new Exception("No forum user ID claim found in hub context of User");
		var userID = Convert.ToInt32(userIDstring.Value);
		return userID;
	}

	public async Task ListenTo(int pmID)
	{
		var userID = GetUserID();
		if (! await _privateMessageService.IsUserInPM(userID, pmID))
			return;
		var pm = await _privateMessageService.Get(pmID, userID);
		var tenant = _tenantService.GetTenant();
		await Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:{pmID}");
	}

	public async Task Send(int pmID, string fullText)
	{
		var userID = GetUserID();
		if (!await _privateMessageService.IsUserInPM(userID, pmID))
			return;
		var pm = await _privateMessageService.Get(pmID, userID);
		var user = await _userService.GetUser(userID);
		await _privateMessageService.Reply(pm, fullText, user);
	}

	public async Task AckRead(int pmID)
	{
		var userID = GetUserID();
		await _privateMessageService.MarkPMRead(userID, pmID);
	}

	public async Task<dynamic[]> GetPosts(int pmID, DateTime beforeDateTime)
	{
		var userID = GetUserID();
		if (! await _privateMessageService.IsUserInPM(userID, pmID))
		{
			return null;
		}
		var posts = await _privateMessageService.GetPosts(pmID, beforeDateTime);
		dynamic[] clientMessages = posts.Select(x => new { pmPostID = x.PMPostID, x.UserID, x.Name, PostTime = x.PostTime.ToString("o"), x.FullText }).ToArray();
		return clientMessages;
	}

	public async Task<dynamic[]> GetMostRecentPosts(int pmID, DateTime afterDateTime)
	{
		var userID = GetUserID();
		if (!await _privateMessageService.IsUserInPM(userID, pmID))
		{
			return null;
		}
		var posts = await _privateMessageService.GetMostRecentPosts(pmID, afterDateTime);
		dynamic[] clientMessages = posts.Select(x => new { pmPostID = x.PMPostID, x.UserID, x.Name, PostTime = x.PostTime.ToString("o"), x.FullText }).ToArray();
		return clientMessages;
	}
}