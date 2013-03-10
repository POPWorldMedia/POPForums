using System;
using System.Web.Mvc;
using PopForums.Models;
using PopForums.Services;

namespace PopForums.Web
{
	public class PopForumsUserAttribute : IAuthorizationFilter, IActionFilter
	{
		public PopForumsUserAttribute(IUserService userService, IUserSessionService userSessionService)
		{
			UserService = userService;
			UserSessionService = userSessionService;
		}

		public IUserService UserService { get; set; }
		public IUserSessionService UserSessionService { get; set; }

		private bool _ignore;

		protected virtual bool IsGlobalFilter()
		{
			return false;
		}

		public void OnAuthorization(AuthorizationContext filterContext)
		{
			if (!IsValidToRunOnController(filterContext))
				return;
			var attributes = filterContext.ActionDescriptor.GetCustomAttributes(typeof(PopForumsAuthorizationIgnoreAttribute), false);
			if (attributes.Length > 0)
			{
				_ignore = true;
				return;
			}
			_ignore = false;
			if (filterContext.HttpContext.User == null)
				return;
			var user = UserService.GetUserByName(filterContext.HttpContext.User.Identity.Name);
			filterContext.HttpContext.User = user;
			UserService.SetupUserViewData(user, filterContext.Controller.ViewData);
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (_ignore || !IsValidToRunOnController(filterContext))
				return;
			if (filterContext.HttpContext.User != null && !(filterContext.HttpContext.User is User))
				throw new Exception(String.Format("HttpContext.User should be of type PopForums.Models.User, but is {0}.", filterContext.HttpContext.User.GetType()));
			UserSessionService.ProcessUserRequest((User)filterContext.HttpContext.User, filterContext.HttpContext);
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
		}

		private bool IsValidToRunOnController(ControllerContext context)
		{
			if (IsGlobalFilter())
				return true;
			var controllerNamespace = context.Controller.GetType().Namespace;
			return controllerNamespace != null && controllerNamespace.StartsWith("PopForums");
		}
	}
}