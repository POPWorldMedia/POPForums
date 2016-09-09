using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Repositories;
using PopForums.Web.Areas.Forums.Controllers;

namespace PopForums.Web.Areas.Forums.Extensions
{
	public static class RouteBuilders
	{
		public static IRouteBuilder AddPopForumsRoutes(this IRouteBuilder routes, IApplicationBuilder app)
		{
			routes.MapRoute(
				"pfsetup",
				"Forums/Setup",
				new { controller = "Setup", action = "Index", Area = "Forums" }
				);
			routes.MapRoute(
				"pfrecent",
				"Forums/Recent/{page?}",
				new { controller = ForumController.Name, action = "Recent", page = 1, Area = "Forums" }
				);
			var forumRepository = app.ApplicationServices.GetService<IForumRepository>();
			var forumConstraint = new ForumRouteConstraint(forumRepository);
			routes.MapRoute(
				"pfroot",
				"Forums/{urlName}/{page?}",
				new { controller = ForumController.Name, action = "Index", page = 1, Area = "Forums" },
				new { forum = forumConstraint }
				);
			routes.MapRoute(
				"pftopic",
				"Forums/Topic/{id}/{page?}",
				new { controller = ForumController.Name, action = "Topic", page = 1, Area = "Forums" }
				);
			routes.MapRoute(
				"pflink",
				"Forums/PostLink/{id}",
				new { controller = ForumController.Name, action = "PostLink", Area = "Forums" }
				);
			routes.MapRoute(
				"pfsubtopics",
				"Forums/Subscription/Topics/{page?}",
				new { controller = SubscriptionController.Name, action = "Topics", page = 1, Area = "Forums" }
				);
			routes.MapRoute(
				"pffavetopics",
				"Forums/Favorites/Topics/{page?}",
				new { controller = FavoritesController.Name, action = "Topics", page = 1, Area = "Forums" }
				);
			routes.MapRoute(
				"pfpagedudertopics",
				"Forums/Account/Posts/{id}/{page?}",
				new { controller = AccountController.Name, action = "Posts", page = 1, Area = "Forums" }
				);
			routes.MapRoute(
				"pftopicunsub",
				"Forums/Subscription/Unsubscribe/{topicID}/{authKey}",
				new { controller = SubscriptionController.Name, action = "Unsubscribe", Area = "Forums" }
				);
			routes.MapRoute(
				"pfpagedadmin",
				"Forums/Admin/ErrorLog/{page?}",
				new { controller = AdminController.Name, action = "ErrorLog", Area = "Forums" }
				);
			return routes;
		}
	}
}