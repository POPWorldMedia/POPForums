namespace PopForums.Mvc.Areas.Forums.Authorization;

public class PopForumsPrivateForumsFilter : IActionFilter
{
	private readonly IUserRetrievalShim _userRetrievalShim;
	private readonly ISettingsManager _settingsManager;
	private readonly IConfig _config;

	public PopForumsPrivateForumsFilter(IUserRetrievalShim userRetrievalShim, ISettingsManager settingsManager, IConfig config)
	{
		_userRetrievalShim = userRetrievalShim;
		_settingsManager = settingsManager;
		_config = config;
	}

	public void OnActionExecuting(ActionExecutingContext context)
	{
		if (!_settingsManager.Current.IsPrivateForumInstance && !_config.IsOAuthOnly)
			return;
		if (_userRetrievalShim.GetUser() == null)
			context.Result = new RedirectToActionResult("Login", AccountController.Name, null);
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
	}
}