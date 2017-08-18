using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Models;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Authorization
{
	public class PopForumsUserAttribute : IAuthorizationFilter, IActionFilter
	{
		public PopForumsUserAttribute(IUserService userService, IUserSessionService userSessionService)
		{
			_userService = userService;
			_userSessionService = userSessionService;
		}

		private readonly IUserService _userService;
		private readonly IUserSessionService _userSessionService;

		private bool _ignore;
		private User _user;

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
			var authResult = context.HttpContext.AuthenticateAsync(PopForumsAuthorizationDefaults.AuthenticationScheme).Result;
			var identity = authResult?.Principal?.Identity as ClaimsIdentity;
			if (identity == null)
				return;
			_user = _userService.GetUserByName(identity.Name);
			if (_user != null)
			{
				foreach (var role in _user.Roles)
					identity.AddClaim(new Claim(PopForumsAuthorizationDefaults.ForumsClaimType, role));
				context.HttpContext.Items["PopForumsUser"] = _user;
				var profileService = context.HttpContext.RequestServices.GetService<IProfileService>();
				var profile = profileService.GetProfile(_user);
				context.HttpContext.Items["PopForumsProfile"] = profile;
				context.HttpContext.User = new ClaimsPrincipal(identity);
			}
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (_ignore)
				return;

			if (filterContext.HttpContext.Response.StatusCode == StatusCodes.Status301MovedPermanently || filterContext.HttpContext.Response.StatusCode == StatusCodes.Status302Found)
				return;
			int cookieSessionID;
			int.TryParse(filterContext.HttpContext.Request.Cookies[UserSessionService._sessionIDCookieName], out cookieSessionID);
			var sessionID = cookieSessionID == 0 ? (int?)null : cookieSessionID;
			var resultSessionID = _userSessionService.ProcessUserRequest(_user, sessionID, filterContext.HttpContext.Connection.RemoteIpAddress.ToString(), 
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