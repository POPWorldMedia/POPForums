using System;
using System.Net;
using Microsoft.AspNet.Mvc;

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

		public static string FullUrlHelper(this Controller controller, string actionName, string controllerName, object routeValues = null)
		{
			var helper = controller.Url;
			if (controller.Request == null)
				return String.Empty;
			var url = controller.Request.Scheme + "://";
			url += controller.Request.Host;
			url += helper.Action(actionName, controllerName, routeValues);
			return url;
		}
	}
}