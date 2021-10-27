namespace PopForums.Mvc.Areas.Forums.Extensions;

public static class Controllers
{
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