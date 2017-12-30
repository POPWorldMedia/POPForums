using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using PopForums.Models;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Authorization
{
	public class PopForumsUserAttribute : IAuthorizationFilter, IActionFilter
	{
		public PopForumsUserAttribute(IUserSessionService userSessionService)
		{
			_userSessionService = userSessionService;
		}
		
		private readonly IUserSessionService _userSessionService;

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

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (_ignore)
				return;

			if (filterContext.HttpContext.Response.StatusCode == StatusCodes.Status301MovedPermanently || filterContext.HttpContext.Response.StatusCode == StatusCodes.Status302Found)
				return;
			int.TryParse(filterContext.HttpContext.Request.Cookies[UserSessionService._sessionIDCookieName], out var cookieSessionID);
			var sessionID = cookieSessionID == 0 ? (int?)null : cookieSessionID;
			var user = filterContext.HttpContext.Items["PopForumsUser"] as User;
			_userSessionService.ProcessUserRequest(user, sessionID, filterContext.HttpContext.Connection.RemoteIpAddress.ToString(), 
				() => filterContext.HttpContext.Response.Cookies.Delete(UserSessionService._sessionIDCookieName), 
				s => filterContext.HttpContext.Response.Cookies.Append(UserSessionService._sessionIDCookieName, s.ToString()));
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
		}

		private bool IsValidToRunOnController(TypeInfo controllerType)
		{
			if (IsGlobalFilter())
				return true;
			var controllerNamespace = controllerType.Namespace;
			return controllerNamespace != null && controllerNamespace.StartsWith("PopForums");
		}
	}
}