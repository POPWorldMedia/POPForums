using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Mvc.Areas.Forums.Controllers;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Extensions
{
	public static class RouteBuilders
	{
		/// <summary>
		/// Adds the POP Forums routes to the application.
		/// </summary>
		/// <param name="routes"></param>
		/// <param name="app"></param>
		/// <returns></returns>
		public static IRouteBuilder AddPopForumsRoutes(this IRouteBuilder routes, IApplicationBuilder app)
		{
			routes.MapRoute(
				"pfsetup",
				"Forums/Setup",
				new { controller = "Setup", action = "Index", Area = "Forums" }
				);

			var setupService = app.ApplicationServices.GetService<ISetupService>();
			if (!setupService.IsConnectionPossible() || !setupService.IsDatabaseSetup())
				return routes;

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
			routes.MapRoute(
				"pfadmin",
				"Forums/Admin/App/{**app}",
				new { controller = AdminController.Name, action = "App", Area = "Forums" });
			return routes;
		}
	}
}