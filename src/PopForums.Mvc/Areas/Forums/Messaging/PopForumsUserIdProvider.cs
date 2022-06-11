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

	public static string GetBaseUserID(string tenantID, string userID)
	{
		var colonIndex = userID.IndexOf(":");
		if (colonIndex == -1)
			throw new ArgumentException("Bummer, can't figure out the userID", nameof(userID));
		return userID.Substring(colonIndex + 1);
	}
}