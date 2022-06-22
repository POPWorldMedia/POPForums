namespace PopForums.Mvc.Areas.Forums.Messaging;

public class PopForumsUserIdProvider : IUserIdProvider
{
	private readonly IUserRetrievalShim _userRetrievalShim;
	private readonly ITenantService _tenantService;

	public PopForumsUserIdProvider(IUserRetrievalShim userRetrievalShim, ITenantService tenantService)
	{
		_userRetrievalShim = userRetrievalShim;
		_tenantService = tenantService;
	}

	public string GetUserId(HubConnectionContext connection)
	{
		var user = _userRetrievalShim.GetUser();
		if (user == null)
			return null;
		var tenantID = _tenantService.GetTenant();
		var id = FormatUserID(tenantID, user.UserID);
		return id;
	}

	public static string FormatUserID(string tenantID, int userID)
	{
		return $"{tenantID}:{userID}";
	}
}