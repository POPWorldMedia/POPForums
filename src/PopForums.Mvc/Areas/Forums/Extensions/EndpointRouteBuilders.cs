using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Mvc.Areas.Forums.Controllers;
using PopForums.Mvc.Areas.Forums.Messaging;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Extensions
{
	public static class EndpointRouteBuilders
	{
		/// <summary>
		/// Adds the POP Forums endpoints to the application.
		/// </summary>
		/// <param name="endpoints"></param>
		/// <param name="app"></param>
		/// <returns></returns>
		public static IEndpointRouteBuilder AddPopForumsEndpoints(this IEndpointRouteBuilder endpoints, IApplicationBuilder app)
		{
			endpoints.MapHub<TopicsHub>("/TopicsHub");
			endpoints.MapHub<RecentHub>("/RecentHub");
			endpoints.MapHub<ForumsHub>("/ForumsHub");
			endpoints.MapHub<FeedHub>("/FeedHub");

			endpoints.MapControllerRoute(
				"pfadmin",
				"Forums/Admin/{**vue}",
				new { controller = AdminController.Name, action = "App", Area = "Forums" });
			endpoints.MapControllerRoute(
				"pfadmin2",
				"Forums/Admin/{vue}/{**admin}",
				new { controller = AdminController.Name, action = "App", Area = "Forums" });
			endpoints.MapControllerRoute(
				"pfsetup",
				"Forums/Setup",
				new { controller = "Setup", action = "Index", Area = "Forums" }
				);

			var setupService = app.ApplicationServices.GetService<ISetupService>();
			if (!setupService.IsConnectionPossible() || !setupService.IsDatabaseSetup())
				return endpoints;

			endpoints.MapControllerRoute(
				"pfrecent1",
				"Forums/Recent",
				new { controller = ForumController.Name, action = "Recent", pageNumber = 1, Area = "Forums" }
			);
			endpoints.MapControllerRoute(
				"pfrecent",
				"Forums/Recent/{pageNumber}",
				new { controller = ForumController.Name, action = "Recent", Area = "Forums" }
				);
			var forumRepository = app.ApplicationServices.GetService<IForumRepository>();
			var forumConstraint = new ForumRouteConstraint(forumRepository);
			endpoints.MapControllerRoute(
				"pfroot1",
				"Forums/{urlName}",
				new { controller = ForumController.Name, action = "Index", pageNumber = 1, Area = "Forums" },
				new { forum = forumConstraint }
			);
			endpoints.MapControllerRoute(
				"pfroot",
				"Forums/{urlName}/{pageNumber}",
				new { controller = ForumController.Name, action = "Index", Area = "Forums" },
				new { forum = forumConstraint }
				);
			endpoints.MapControllerRoute(
				"pftopic1",
				"Forums/Topic/{id}",
				new { controller = ForumController.Name, action = "Topic", pageNumber = 1, Area = "Forums" }
			);
			endpoints.MapControllerRoute(
				"pftopic",
				"Forums/Topic/{id}/{pageNumber}",
				new { controller = ForumController.Name, action = "Topic", Area = "Forums" }
				);
			endpoints.MapControllerRoute(
				"pflink",
				"Forums/PostLink/{id}",
				new { controller = ForumController.Name, action = "PostLink", Area = "Forums" }
				);
			endpoints.MapControllerRoute(
				"pfsubtopics1",
				"Forums/Subscription/Topics",
				new { controller = SubscriptionController.Name, action = "Topics", pageNumber = 1, Area = "Forums" }
			);
			endpoints.MapControllerRoute(
				"pfsubtopics",
				"Forums/Subscription/Topics/{pageNumber}",
				new { controller = SubscriptionController.Name, action = "Topics", Area = "Forums" }
				);
			endpoints.MapControllerRoute(
				"pffavetopics1",
				"Forums/Favorites/Topics",
				new { controller = FavoritesController.Name, action = "Topics", pageNumber = 1, Area = "Forums" }
			);
			endpoints.MapControllerRoute(
				"pffavetopics",
				"Forums/Favorites/Topics/{pageNumber}",
				new { controller = FavoritesController.Name, action = "Topics", Area = "Forums" }
				);
			endpoints.MapControllerRoute(
				"pfpagedusertopics1",
				"Forums/Account/Posts/{id}",
				new { controller = AccountController.Name, action = "Posts", pageNumber = 1, Area = "Forums" }
			);
			endpoints.MapControllerRoute(
				"pfpagedusertopics",
				"Forums/Account/Posts/{id}/{pageNumber}",
				new { controller = AccountController.Name, action = "Posts", Area = "Forums" }
				);
			endpoints.MapControllerRoute(
				"pftopicunsub",
				"Forums/Subscription/Unsubscribe/{topicID}/{authKey}",
				new { controller = SubscriptionController.Name, action = "Unsubscribe", Area = "Forums" }
				);
			return endpoints;
		}
	}
}