using System;
using System.Net;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Routing;
using PopForums.Models;

namespace PopForums.Web.Extensions
{
	public static class Controllers
	{
		public static ViewResult Forbidden(this Controller controller, string viewName, object model)
		{
			controller.Response.StatusCode = (int)HttpStatusCode.Forbidden;
			return BuildViewResult(viewName, model);
		}

		public static ViewResult NotFound(this Controller controller, string viewName, object model)
		{
			controller.Response.StatusCode = (int)HttpStatusCode.NotFound;
			return BuildViewResult(viewName, model);
		}

		private static ViewResult BuildViewResult(string viewName, object model)
		{
			var result = new ViewResult
			{
				ViewName = viewName,
				ViewData = { Model = model }
			};
			return result;
		}

		// TODO: FullUrlHelper
		//public static string FullUrlHelper(this Controller controller, string actionName, string controllerName, object routeValues = null)
		//{
		//	var helper = new UrlHelper(controller.Request.RequestContext);
		//	var requestUrl = controller.Request.Url;
		//	if (requestUrl == null)
		//		return String.Empty;
		//	var url = requestUrl.Scheme + "://";
		//	url += requestUrl.Host;
		//	url += (requestUrl.Port != 80 ? ":" + requestUrl.Port : "");
		//	url += helper.Action(actionName, controllerName, routeValues);
		//	return url;
		//}
		
		public static User CurrentUser(this Controller controller)
		{
			var user = controller.HttpContext.Items["PopForumsUser"] as User;
			return user;
		}
	}
}