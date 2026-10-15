using PopForums.Mvc.Areas.Forums.Authentication;

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
		var attributes = controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(PopForumsAuthenticationIgnoreAttribute), false);
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

		if (filterContext.HttpContext.Response.StatusCode == StatusCodes.Status301MovedPermanently || filterContext.HttpContext.Response.StatusCode == StatusCodes.Status302Found)
		{
			await next.Invoke();
			return;
		}
		int.TryParse(filterContext.HttpContext.Request.Cookies[UserSessionService._sessionIDCookieName], out var cookieSessionID);
		var sessionID = cookieSessionID == 0 ? (int?)null : cookieSessionID;
		var user = filterContext.HttpContext.Items["PopForumsUser"] as User;

		// Only a request that would start a new anonymous session needs screening; a returned session cookie
		// or a signed-in user means the client already holds cookies like a browser.
		if (sessionID == null && user == null && IsNonSessionClient(filterContext.HttpContext.Request))
		{
			await next.Invoke();
			return;
		}
		await userSessionService.ProcessUserRequest(user, sessionID, filterContext.HttpContext.Connection.RemoteIpAddress.ToString(), 
			() => filterContext.HttpContext.Response.Cookies.Delete(UserSessionService._sessionIDCookieName), 
			s => filterContext.HttpContext.Response.Cookies.Append(UserSessionService._sessionIDCookieName, s.ToString(), new CookieOptions { HttpOnly = true, Secure = true }));
		await next.Invoke();
	}

	private static readonly string[] NonSessionUserAgentFragments =
	[
		"bot", "crawl", "spider", "slurp", "curl", "wget", "python", "go-http-client",
		"headless", "httpclient", "okhttp", "java/", "libwww", "scrapy", "axios", "node-fetch"
	];

	/// <summary>
	/// Identifies requests that almost certainly don't come from a person in a browser, so they
	/// shouldn't create or update a session. Real browsers always send a user agent and an
	/// Accept-Language header; most scripts and crawlers omit at least one.
	/// </summary>
	public static bool IsNonSessionClient(HttpRequest request)
	{
		var userAgent = request.Headers.UserAgent.ToString();
		if (string.IsNullOrWhiteSpace(userAgent))
			return true;
		if (string.IsNullOrWhiteSpace(request.Headers.AcceptLanguage.ToString()))
			return true;
		foreach (var fragment in NonSessionUserAgentFragments)
			if (userAgent.Contains(fragment, StringComparison.OrdinalIgnoreCase))
				return true;
		return false;
	}

	private bool IsValidToRunOnController(TypeInfo controllerType)
	{
		if (IsGlobalFilter())
			return true;
		var controllerNamespace = controllerType.Namespace;
		return controllerNamespace != null && controllerNamespace.StartsWith("PopForums");
	}
}