﻿using Microsoft.AspNetCore.Builder;
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
			routes.MapRoute(
				"pfadmin",
				"Forums/Admin/{**app}",
				new { controller = AdminController.Name, action = "App", Area = "Forums" });

			var setupService = app.ApplicationServices.GetService<ISetupService>();
			if (!setupService.IsConnectionPossible() || !setupService.IsDatabaseSetup())
				return routes;

			routes.MapRoute(
				"pfrecent1",
				"Forums/Recent",
				new { controller = ForumController.Name, action = "Recent", pageNumber = 1, Area = "Forums" }
			);
			routes.MapRoute(
				"pfrecent",
				"Forums/Recent/{pageNumber}",
				new { controller = ForumController.Name, action = "Recent", Area = "Forums" }
				);
			var forumRepository = app.ApplicationServices.GetService<IForumRepository>();
			var forumConstraint = new ForumRouteConstraint(forumRepository);
			routes.MapRoute(
				"pfroot1",
				"Forums/{urlName}",
				new { controller = ForumController.Name, action = "Index", pageNumber = 1, Area = "Forums" },
				new { forum = forumConstraint }
			);
			routes.MapRoute(
				"pfroot",
				"Forums/{urlName}/{pageNumber}",
				new { controller = ForumController.Name, action = "Index", Area = "Forums" },
				new { forum = forumConstraint }
				);
			routes.MapRoute(
				"pftopic1",
				"Forums/Topic/{id}",
				new { controller = ForumController.Name, action = "Topic", pageNumber = 1, Area = "Forums" }
			);
			routes.MapRoute(
				"pftopic",
				"Forums/Topic/{id}/{pageNumber}",
				new { controller = ForumController.Name, action = "Topic", Area = "Forums" }
				);
			routes.MapRoute(
				"pflink",
				"Forums/PostLink/{id}",
				new { controller = ForumController.Name, action = "PostLink", Area = "Forums" }
				);
			routes.MapRoute(
				"pfsubtopics1",
				"Forums/Subscription/Topics",
				new { controller = SubscriptionController.Name, action = "Topics", pageNumber = 1, Area = "Forums" }
			);
			routes.MapRoute(
				"pfsubtopics",
				"Forums/Subscription/Topics/{pageNumber}",
				new { controller = SubscriptionController.Name, action = "Topics", Area = "Forums" }
				);
			routes.MapRoute(
				"pffavetopics1",
				"Forums/Favorites/Topics",
				new { controller = FavoritesController.Name, action = "Topics", pageNumber = 1, Area = "Forums" }
			);
			routes.MapRoute(
				"pffavetopics",
				"Forums/Favorites/Topics/{pageNumber}",
				new { controller = FavoritesController.Name, action = "Topics", Area = "Forums" }
				);
			routes.MapRoute(
				"pfpagedusertopics1",
				"Forums/Account/Posts/{id}",
				new { controller = AccountController.Name, action = "Posts", pageNumber = 1, Area = "Forums" }
			);
			routes.MapRoute(
				"pfpagedusertopics",
				"Forums/Account/Posts/{id}/{pageNumber}",
				new { controller = AccountController.Name, action = "Posts", Area = "Forums" }
				);
			routes.MapRoute(
				"pftopicunsub",
				"Forums/Subscription/Unsubscribe/{topicID}/{authKey}",
				new { controller = SubscriptionController.Name, action = "Unsubscribe", Area = "Forums" }
				);
			return routes;
		}
	}
}