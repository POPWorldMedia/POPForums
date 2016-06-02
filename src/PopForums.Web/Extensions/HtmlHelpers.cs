using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Models;
using PopForums.Services;

namespace PopForums.Web.Extensions
{
	public static class HtmlHelpers
	{
		// TODO: PagerLinks
		//public static HtmlString PagerLinks(this HtmlHelper htmlHelper, string controllerName, string actionName, PagerContext pagerContext, string cssClass, string moreTextCssClass, string currentPageCssClass)
		//{
		//	return PagerLinks(htmlHelper, controllerName, actionName, pagerContext, Resources.First, Resources.Previous, Resources.Next, Resources.Last, Resources.More + ":", cssClass, moreTextCssClass, currentPageCssClass, null);
		//}

		//public static HtmlString PagerLinks(this HtmlHelper htmlHelper, string controllerName, string actionName, PagerContext pagerContext, string cssClass, string moreTextCssClass, string currentPageCssClass, Dictionary<string, object> routeParameters)
		//{
		//	return PagerLinks(htmlHelper, controllerName, actionName, pagerContext, Resources.First, Resources.Previous, Resources.Next, Resources.Last, Resources.More + ":", cssClass, moreTextCssClass, currentPageCssClass, routeParameters);
		//}

		//public static HtmlString PagerLinks(this HtmlHelper htmlHelper, string controllerName, string actionName, PagerContext pagerContext, int low, int high, string id, string cssClass, string moreTextCssClass, string currentPageCssClass)
		//{
		//	return PagerLinks(htmlHelper, controllerName, actionName, pagerContext, low, high, id, Resources.First, Resources.Previous, Resources.Next, Resources.Last, Resources.More + ":", cssClass, moreTextCssClass, currentPageCssClass);
		//}

		//public static HtmlString PagerLinks(this HtmlHelper htmlHelper, string controllerName, string actionName, PagerContext pagerContext, string firstPage, string previousPage, string nextPage, string lastPage, string moreText, string cssClass, string moreTextCssClass, string currentPageCssClass, Dictionary<string, object> routeParameters)
		//{
		//	if (pagerContext == null)
		//		return null;
		//	var builder = new StringBuilder();
		//	if (String.IsNullOrEmpty(controllerName) || String.IsNullOrEmpty(actionName))
		//		throw new Exception("controllerName and actionName must be specified for PageLinks.");
		//	if (pagerContext.PageCount <= 1)
		//		return new HtmlString(String.Empty);

		//	if (String.IsNullOrEmpty(cssClass)) builder.Append("<ul class=\"pagination\">");
		//	else builder.Append(String.Format("<ul class=\"pagination {0}\">", cssClass));
		//	if (String.IsNullOrEmpty(moreTextCssClass)) builder.Append(String.Format("<li><span>{0}</span></li>", moreText));
		//	else builder.Append(String.Format("<li class=\"{0}\"><span>{1}</span></li>", moreTextCssClass, moreText));

		//	if (pagerContext.PageIndex != 1)
		//	{
		//		// first page link
		//		builder.Append("<li>");
		//		var firstRouteDictionary = new RouteValueDictionary(new { controller = controllerName, action = actionName, page = 1 });
		//		if (routeParameters != null)
		//			foreach (var item in routeParameters)
		//				firstRouteDictionary.Add(item.Key, item.Value);
		//		var firstLink = HtmlHelper.GenerateRouteLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, "", null, firstRouteDictionary, new Dictionary<string, object> { { "title", firstPage }, { "class", "glyphicon glyphicon-step-backward" } });
		//		builder.Append(firstLink);
		//		builder.Append("</li>");
		//		if (pagerContext.PageIndex > 2)
		//		{
		//			// previous page link
		//			var previousIndex = pagerContext.PageIndex - 1;
		//			builder.Append("<li>");
		//			var previousRouteDictionary = new RouteValueDictionary(new { controller = controllerName, action = actionName, page = previousIndex });
		//			if (routeParameters != null)
		//				foreach (var item in routeParameters)
		//					previousRouteDictionary.Add(item.Key, item.Value);
		//			var previousLink = HtmlHelper.GenerateRouteLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, "", null, previousRouteDictionary, new Dictionary<string, object> { { "title", previousPage }, { "rel", "prev" }, { "class", "glyphicon glyphicon-chevron-left" } });
		//			builder.Append(previousLink);
		//			builder.Append("</li>");
		//		}
		//	}

		//	// calc low and high limits for numeric links
		//	var low = pagerContext.PageIndex - 1;
		//	var high = pagerContext.PageIndex + 3;
		//	if (low < 1) low = 1;
		//	if (high > pagerContext.PageCount) high = pagerContext.PageCount;
		//	if (high - low < 5) while ((high < low + 4) && high < pagerContext.PageCount) high++;
		//	if (high - low < 5) while ((low > high - 4) && low > 1) low--;
		//	for (var x = low; x < high + 1; x++)
		//	{
		//		// numeric links
		//		if (x == pagerContext.PageIndex)
		//		{
		//			if (String.IsNullOrEmpty(currentPageCssClass))
		//				builder.Append(String.Format("<li><span class=\"active\">{0} of {1}</span></li>", x, pagerContext.PageCount));
		//			else builder.Append(String.Format("<li class=\"active {0}\"><span>{1} of {2}</span></li>", currentPageCssClass, x, pagerContext.PageCount));
		//		}
		//		else
		//		{
		//			builder.Append("<li>");
		//			var numericRouteDictionary = new RouteValueDictionary { { "controller", controllerName }, { "action", actionName }, { "page", x } };
		//			if (routeParameters != null)
		//				foreach (var item in routeParameters)
		//					numericRouteDictionary.Add(item.Key, item.Value);
		//			builder.Append(htmlHelper.RouteLink(x.ToString(), numericRouteDictionary));
		//			builder.Append("</li>");
		//		}
		//	}
		//	if (pagerContext.PageIndex != pagerContext.PageCount)
		//	{
		//		if (pagerContext.PageIndex < pagerContext.PageCount - 1)
		//		{
		//			// next page link
		//			var nextIndex = pagerContext.PageIndex + 1;
		//			builder.Append("<li>");
		//			var nextRouteDictionary = new RouteValueDictionary(new { controller = controllerName, action = actionName, page = nextIndex });
		//			if (routeParameters != null)
		//				foreach (var item in routeParameters)
		//					nextRouteDictionary.Add(item.Key, item.Value);
		//			var nextLink = HtmlHelper.GenerateRouteLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, "", null, nextRouteDictionary, new Dictionary<string, object> { { "title", nextPage }, { "rel", "next" }, { "class", "glyphicon glyphicon-chevron-right" } });
		//			builder.Append(nextLink);
		//			builder.Append("</li>");
		//		}
		//		// last page link
		//		builder.Append("<li>");
		//		var lastRouteDictionary = new RouteValueDictionary(new { controller = controllerName, action = actionName, page = pagerContext.PageCount });
		//		if (routeParameters != null)
		//			foreach (var item in routeParameters)
		//				lastRouteDictionary.Add(item.Key, item.Value);
		//		var lastLink = HtmlHelper.GenerateRouteLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, "", null, lastRouteDictionary, new Dictionary<string, object> { { "title", lastPage }, { "class", "glyphicon glyphicon-step-forward" } });
		//		builder.Append(lastLink);
		//		builder.Append("</li>");
		//	}
		//	builder.Append("</ul>");

		//	return new HtmlString(builder.ToString());
		//}

		//public static HtmlString PagerLinks(this HtmlHelper htmlHelper, string controllerName, string actionName, PagerContext pagerContext, int low, int high, string id, string firstPage, string previousPage, string nextPage, string lastPage, string moreText, string cssClass, string moreTextCssClass, string currentPageCssClass)
		//{
		//	if (pagerContext == null)
		//		return null;
		//	var builder = new StringBuilder();
		//	if (String.IsNullOrEmpty(controllerName) || String.IsNullOrEmpty(actionName))
		//		throw new Exception("controllerName and actionName must be specified for PageLinks.");
		//	if (pagerContext.PageCount <= 1)
		//		return new HtmlString(String.Empty);

		//	if (String.IsNullOrEmpty(cssClass)) builder.Append("<ul class=\"pagination\">");
		//	else builder.Append(String.Format("<ul class=\"pagination {0}\">", cssClass));
		//	if (String.IsNullOrEmpty(moreTextCssClass)) builder.Append(String.Format("<li><span>{0}</span></li>", moreText));
		//	else builder.Append(String.Format("<li class=\"{0}\"><span>{1}</span></li>", moreTextCssClass, moreText));

		//	if (pagerContext.PageIndex != 1 && low != 1)
		//	{
		//		// first page link
		//		builder.Append("<li>");
		//		var firstLink = HtmlHelper.GenerateRouteLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, "", null, new RouteValueDictionary(new { controller = controllerName, action = actionName, page = 1, id }), new Dictionary<string, object> { { "title", firstPage }, { "class", "glyphicon glyphicon-step-backward" } });
		//		builder.Append(firstLink);
		//		builder.Append("</li>");
		//		if (pagerContext.PageIndex > 2 && !(low < pagerContext.PageIndex))
		//		{
		//			// previous page link
		//			var previousIndex = pagerContext.PageIndex - 1;
		//			builder.Append("<li>");
		//			var previousLink = HtmlHelper.GenerateRouteLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, "", null, new RouteValueDictionary(new { controller = controllerName, action = actionName, page = previousIndex, id }), new Dictionary<string, object> { { "title", previousPage }, { "rel", "prev" }, { "class", "glyphicon glyphicon-chevron-left" } });
		//			builder.Append(previousLink);
		//			builder.Append("</li>");
		//		}
		//	}

		//	// calc low and high limits for numeric links
		//	var calcLow = pagerContext.PageIndex - 1;
		//	var calcHigh = pagerContext.PageIndex + 3;
		//	if (calcLow < 1) calcLow = 1;
		//	if (calcHigh > pagerContext.PageCount) calcHigh = pagerContext.PageCount;
		//	if (calcHigh - calcLow < 5) while ((calcHigh < calcLow + 4) && calcHigh < pagerContext.PageCount) calcHigh++;
		//	if (calcHigh - calcLow < 5) while ((calcLow > calcHigh - 4) && calcLow > 1) calcLow--;
		//	var isRangeRendered = false;
		//	for (var x = calcLow; x < calcHigh + 1; x++)
		//	{
		//		// numeric links
		//		if (x >= low && x <= high)
		//		{
		//			if (!isRangeRendered)
		//			{
		//				isRangeRendered = true;
		//				if (String.IsNullOrEmpty(currentPageCssClass))
		//					builder.Append(String.Format("<li class=\"active\"><span>{0}-{1} of {2}</span></li>", low, high, pagerContext.PageCount));
		//				else builder.Append(String.Format("<li class=\"active {0}\"><span>{1}-{2} of {3}</span></li>", currentPageCssClass, low, high, pagerContext.PageCount));
		//			}
		//		}
		//		else
		//		{
		//			builder.Append("<li>");
		//			builder.Append(htmlHelper.RouteLink(x.ToString(), new { controller = controllerName, action = actionName, page = x, id }));
		//			builder.Append("</li>");
		//		}
		//	}
		//	if (pagerContext.PageIndex != pagerContext.PageCount && high < pagerContext.PageCount)
		//	{
		//		if (pagerContext.PageIndex < pagerContext.PageCount - 1)
		//		{
		//			// next page link
		//			var nextIndex = pagerContext.PageIndex + 1;
		//			builder.Append("<li>");
		//			var nextLink = HtmlHelper.GenerateRouteLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, "", null, new RouteValueDictionary(new { controller = controllerName, action = actionName, page = nextIndex, id }), new Dictionary<string, object> { { "title", nextPage }, { "rel", "next" }, { "class", "glyphicon glyphicon-chevron-right" } });
		//			builder.Append(nextLink);
		//			builder.Append("</li>");
		//		}
		//		// last page link
		//		builder.Append("<li>");
		//		var lastLink = HtmlHelper.GenerateRouteLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, "", null, new RouteValueDictionary(new { controller = controllerName, action = actionName, page = pagerContext.PageCount, id }), new Dictionary<string, object> { { "title", lastPage }, { "class", "glyphicon glyphicon-step-forward" } });
		//		builder.Append(lastLink);
		//		builder.Append("</li>");
		//	}
		//	builder.Append("</ul>");

		//	return new HtmlString(builder.ToString());
		//}

		public static IHtmlContent TimeZoneDropDown(this IHtmlHelper helper, string name, object htmlAttributes, object selectedValue)
		{
			var result = helper.DropDownList(name, new SelectList(DataCollections.TimeZones(), "Key", "Value", selectedValue), htmlAttributes);
			return result;
		}
		
		public static HtmlString RoleCheckBoxes(this IHtmlHelper helper, string name, string[] checkedRoles)
		{
			var build = new StringBuilder();
			var userService = helper.ViewContext.HttpContext.RequestServices.GetService<IUserService>();
			var roles = userService.GetAllRoles();
			foreach (var role in roles)
			{
				build.Append("<input type=\"checkbox\" name=\"");
				build.Append(name);
				build.Append("\" value=\"");
				build.Append(role);
				build.Append("\"");
				if (checkedRoles.Contains(role))
					build.Append(" checked=\"checked\"");
				build.Append(" /><label for=\"");
				build.Append(role);
				build.Append("\">");
				build.Append(role);
				build.Append("</label><br />");
			}
			return new HtmlString(build.ToString());
		}


		public static HtmlString PMReadIndicator(this IHtmlHelper helper, PrivateMessage pm, string path)
		{
			return PMReadIndicator(helper, pm, path, null);
		}

		public static HtmlString PMReadIndicator(this IHtmlHelper helper, PrivateMessage pm, string path, string cssClass)
		{
			var alt = Resources.NoNewPosts;
			var image = "NoNewIndicator.png";
			if (pm.LastPostTime > pm.LastViewDate)
			{
				alt = Resources.NewPosts;
				image = "NewIndicator.png";
			}
			var build = new StringBuilder();
			build.Append("<img src=\"");
			build.Append(path);
			build.Append(image);
			build.Append("\" alt=\"");
			build.Append(alt);
			build.Append("\"");
			if (!String.IsNullOrEmpty(cssClass))
			{
				build.Append(" class=\"");
				build.Append(cssClass);
				build.Append("\"");
			}
			build.Append(" />");
			return new HtmlString(build.ToString());
		}

		public static bool IsNewPosts(this IHtmlHelper helper, Topic topic, PagedTopicContainer container)
		{
			if (!container.ReadStatusLookup.ContainsKey(topic.TopicID))
				return false;
			if (container.ReadStatusLookup[topic.TopicID] == (ReadStatus.NewPosts | container.ReadStatusLookup[topic.TopicID]))
				return true;
			return false;
		}

		public static string AddValidationClass(this IHtmlHelper helper, string fieldName, string cssClass)
		{
			if (!helper.ViewContext.ViewData.ModelState.ContainsKey(fieldName))
				return String.Empty;
			var result = String.Empty;
			var field = helper.ViewContext.ViewData.ModelState.SingleOrDefault(x => x.Key == fieldName);
			if (field.Value.Errors.Count > 0)
				result = cssClass;
			return result;
		}
	}
}
