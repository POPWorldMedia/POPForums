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
