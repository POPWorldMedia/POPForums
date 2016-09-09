using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Models;
using PopForums.Services;

namespace PopForums.Web.Areas.Forums.Extensions
{
	public static class HtmlHelpers
	{
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
	}
}
