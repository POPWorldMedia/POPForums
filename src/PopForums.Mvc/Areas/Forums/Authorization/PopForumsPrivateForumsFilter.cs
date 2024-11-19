namespace PopForums.Mvc.Areas.Forums.Authorization;

public class PopForumsPrivateForumsFilter(IUserRetrievalShim userRetrievalShim, ISettingsManager settingsManager, IConfig config) : IActionFilter
{
	public void OnActionExecuting(ActionExecutingContext context)
	{
		if (!settingsManager.Current.IsPrivateForumInstance && !config.IsOAuthOnly)
			return;
		if (userRetrievalShim.GetUser() == null)
			context.Result = new RedirectToActionResult("Login", AccountController.Name, null);
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
	}
}