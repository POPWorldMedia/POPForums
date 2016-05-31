using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using PopForums.Models;

namespace PopForums.Web.Areas.Forums.TagHelpers
{
	[HtmlTargetElement("pf-pagerLinks", Attributes = "controllerName, actionName, pagerContext")]
	public class PagerLinksTagHelper : TagHelper
	{
		private readonly IHtmlGenerator _htmlGenerator;

		[HtmlAttributeName("controllerName")]
		public string ControllerName { get; set; }
		[HtmlAttributeName("actionName")]
		public string ActionName { get; set; }
		[HtmlAttributeName("pagerContext")]
		public PagerContext PagerContext { get; set; }
		[HtmlAttributeName("class")]
		public string Class { get; set; }
		[HtmlAttributeName("moreTextClass")]
		public string MoreTextClass { get; set; }
		[HtmlAttributeName("currentTextClass")]
		public string CurrentTextClass { get; set; }
		[HtmlAttributeName("routeParameters")]
		public Dictionary<string, object> RouteParameters { get; set; }

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		public PagerLinksTagHelper(IHtmlGenerator htmlGenerator)
		{
			_htmlGenerator = htmlGenerator;
		}

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (PagerContext == null)
				return;
			var builder = new StringBuilder();
			if (String.IsNullOrEmpty(ControllerName) || String.IsNullOrEmpty(ActionName))
				throw new Exception("controllerName and actionName must be specified for PageLinks.");
			if (PagerContext.PageCount <= 1)
				return;
			
			if (String.IsNullOrEmpty(MoreTextClass)) builder.Append($"<li><span>{Resources.More}:</span></li>");
			else builder.Append($"<li class=\"{MoreTextClass}\"><span>{Resources.More}</span></li>");

			if (PagerContext.PageIndex != 1)
			{
				// first page link
				builder.Append("<li>");
				var firstRouteDictionary = new RouteValueDictionary(new { controller = ControllerName, action = ActionName, page = 1 });
				if (RouteParameters != null)
					foreach (var item in RouteParameters)
						firstRouteDictionary.Add(item.Key, item.Value);
				var firstLink = _htmlGenerator.GenerateActionLink(ViewContext, Resources.First, ActionName, ControllerName, null, null, null,
					firstRouteDictionary, new { title = Resources.First, @class = "glyphicon glyphicon-step-backward" });
				builder.Append(firstLink);
			//	builder.Append("</li>");
			//	if (pagerContext.PageIndex > 2)
			//	{
			//		// previous page link
			//		var previousIndex = pagerContext.PageIndex - 1;
			//		builder.Append("<li>");
			//		var previousRouteDictionary = new RouteValueDictionary(new { controller = controllerName, action = actionName, page = previousIndex });
			//		if (routeParameters != null)
			//			foreach (var item in routeParameters)
			//				previousRouteDictionary.Add(item.Key, item.Value);
			//		var previousLink = HtmlHelper.GenerateRouteLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, "", null, previousRouteDictionary, new Dictionary<string, object> { { "title", previousPage }, { "rel", "prev" }, { "class", "glyphicon glyphicon-chevron-left" } });
			//		builder.Append(previousLink);
			//		builder.Append("</li>");
			//	}
			//}

			//// calc low and high limits for numeric links
			//var low = pagerContext.PageIndex - 1;
			//var high = pagerContext.PageIndex + 3;
			//if (low < 1) low = 1;
			//if (high > pagerContext.PageCount) high = pagerContext.PageCount;
			//if (high - low < 5) while ((high < low + 4) && high < pagerContext.PageCount) high++;
			//if (high - low < 5) while ((low > high - 4) && low > 1) low--;
			//for (var x = low; x < high + 1; x++)
			//{
			//	// numeric links
			//	if (x == pagerContext.PageIndex)
			//	{
			//		if (String.IsNullOrEmpty(currentPageCssClass))
			//			builder.Append(String.Format("<li><span class=\"active\">{0} of {1}</span></li>", x, pagerContext.PageCount));
			//		else builder.Append(String.Format("<li class=\"active {0}\"><span>{1} of {2}</span></li>", currentPageCssClass, x, pagerContext.PageCount));
			//	}
			//	else
			//	{
			//		builder.Append("<li>");
			//		var numericRouteDictionary = new RouteValueDictionary { { "controller", controllerName }, { "action", actionName }, { "page", x } };
			//		if (routeParameters != null)
			//			foreach (var item in routeParameters)
			//				numericRouteDictionary.Add(item.Key, item.Value);
			//		builder.Append(htmlHelper.RouteLink(x.ToString(), numericRouteDictionary));
			//		builder.Append("</li>");
			//	}
			//}
			//if (pagerContext.PageIndex != pagerContext.PageCount)
			//{
			//	if (pagerContext.PageIndex < pagerContext.PageCount - 1)
			//	{
			//		// next page link
			//		var nextIndex = pagerContext.PageIndex + 1;
			//		builder.Append("<li>");
			//		var nextRouteDictionary = new RouteValueDictionary(new { controller = controllerName, action = actionName, page = nextIndex });
			//		if (routeParameters != null)
			//			foreach (var item in routeParameters)
			//				nextRouteDictionary.Add(item.Key, item.Value);
			//		var nextLink = HtmlHelper.GenerateRouteLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, "", null, nextRouteDictionary, new Dictionary<string, object> { { "title", nextPage }, { "rel", "next" }, { "class", "glyphicon glyphicon-chevron-right" } });
			//		builder.Append(nextLink);
			//		builder.Append("</li>");
			//	}
			//	// last page link
			//	builder.Append("<li>");
			//	var lastRouteDictionary = new RouteValueDictionary(new { controller = controllerName, action = actionName, page = pagerContext.PageCount });
			//	if (routeParameters != null)
			//		foreach (var item in routeParameters)
			//			lastRouteDictionary.Add(item.Key, item.Value);
			//	var lastLink = HtmlHelper.GenerateRouteLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, "", null, lastRouteDictionary, new Dictionary<string, object> { { "title", lastPage }, { "class", "glyphicon glyphicon-step-forward" } });
			//	builder.Append(lastLink);
			//	builder.Append("</li>");
			}


			output.TagName = "ul";
			if (!String.IsNullOrWhiteSpace(Class))
				output.Attributes.Add("class", "pagination " + Class);
			else
				output.Attributes.Add("class", "pagination");
			output.Content.AppendHtml(builder.ToString());
		}
	}
}