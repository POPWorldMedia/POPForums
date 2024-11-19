namespace PopForums.Mvc.Areas.Forums.Authorization;

/// <summary>
/// This attribute, typically applied globally, is used to track sessions for all users (authenticated or not)
/// and drive the "currently online" list and user count.
/// </summary>
/// <param name="userSessionService"></param>
public class PopForumsUserAttribute(IUserSessionService userSessionService) : IAuthorizationFilter, IAsyncActionFilter
{
	private bool _ignore;

	protected virtual bool IsGlobalFilter()
	{
		return false;
	}

	public void OnAuthorization(AuthorizationFilterContext context)
	{
		var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
		if (controllerActionDescriptor == null)
			return;
		if (!IsValidToRunOnController(controllerActionDescriptor.ControllerTypeInfo))
		{
			_ignore = true;
			return;
		}
		var attributes = controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(PopForumsAuthorizationIgnoreAttribute), false);
		if (attributes.Any())
		{
			_ignore = true;
			return;
		}
		_ignore = false;
	}

	public async Task OnActionExecutionAsync(ActionExecutingContext filterContext, ActionExecutionDelegate next)
	{
		if (_ignore)
		{
			await next.Invoke();
			return;
		}

		var userAgents = filterContext.HttpContext.Request.Headers.UserAgent;
		if (userAgents.Count > 0 && (userAgents[0].ToLower().Contains("bot") || userAgents[0].ToLower().Contains("crawl")))
		{
			await next.Invoke();
			return;
		}

		if (filterContext.HttpContext.Response.StatusCode == StatusCodes.Status301MovedPermanently || filterContext.HttpContext.Response.StatusCode == StatusCodes.Status302Found)
		{
			await next.Invoke();
			return;
		}
		int.TryParse(filterContext.HttpContext.Request.Cookies[UserSessionService._sessionIDCookieName], out var cookieSessionID);
		var sessionID = cookieSessionID == 0 ? (int?)null : cookieSessionID;
		var user = filterContext.HttpContext.Items["PopForumsUser"] as User;
		await userSessionService.ProcessUserRequest(user, sessionID, filterContext.HttpContext.Connection.RemoteIpAddress.ToString(), 
			() => filterContext.HttpContext.Response.Cookies.Delete(UserSessionService._sessionIDCookieName), 
			s => filterContext.HttpContext.Response.Cookies.Append(UserSessionService._sessionIDCookieName, s.ToString()));
		await next.Invoke();
	}

	private bool IsValidToRunOnController(TypeInfo controllerType)
	{
		if (IsGlobalFilter())
			return true;
		var controllerNamespace = controllerType.Namespace;
		return controllerNamespace != null && controllerNamespace.StartsWith("PopForums");
	}
}