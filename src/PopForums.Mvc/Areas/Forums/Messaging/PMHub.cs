namespace PopForums.Mvc.Areas.Forums.Messaging;

public class PMHub : Hub
{
	private readonly IPrivateMessageService _privateMessageService;
	private readonly ITenantService _tenantService;

	public PMHub(IPrivateMessageService privateMessageService, ITenantService tenantService)
	{
		_privateMessageService = privateMessageService;
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

	public async void ListenTo(int pmID)
	{
		return;

		var pm = await _privateMessageService.Get(pmID);
		var userID = GetUserID();
		if (! await _privateMessageService.IsUserInPM(userID, pmID))
			return;
		var tenant = _tenantService.GetTenant();
		await Groups.AddToGroupAsync(Context.ConnectionId, $"{tenant}:{pmID}");
	}
}