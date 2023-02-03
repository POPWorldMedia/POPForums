namespace PopForums.Mvc.Areas.Forums.Extensions;

public static class WebApplications
{
	/// <summary>
	/// Adds the POP Forums app to the application.
	/// </summary>
	/// <param name="app"></param>
	/// <returns></returns>
	public static WebApplication AddPopForumsEndpoints(this WebApplication app)
	{
		app.MapHub<PopForumsHub>("/PopForumsHub");

		app.MapControllerRoute(
			"pfadmin",
			"Forums/Admin/{**vue}",
			new { controller = AdminController.Name, action = "App", Area = "Forums" });
		app.MapControllerRoute(
			"pfadmin2",
			"Forums/Admin/{vue}/{**admin}",
			new { controller = AdminController.Name, action = "App", Area = "Forums" });
		app.MapControllerRoute(
			"pfsetup",
			"Forums/Setup",
			new { controller = "Setup", action = "Index", Area = "Forums" }
		);

		var setupService = app.Services.GetService<ISetupService>();
		if (!setupService.IsConnectionPossible() || !setupService.IsDatabaseSetup())
			return app;

		app.MapControllerRoute(
			"pfrecent1",
			"Forums/Recent",
			new { controller = ForumController.Name, action = "Recent", pageNumber = 1, Area = "Forums" }
		);
		app.MapControllerRoute(
			"pfrecent",
			"Forums/Recent/{pageNumber}",
			new { controller = ForumController.Name, action = "Recent", Area = "Forums" }
		);
		var forumRepository = app.Services.GetService<IForumRepository>();
		var forumConstraint = new ForumRouteConstraint(forumRepository);
		app.MapControllerRoute(
			"pfroot1",
			"Forums/{urlName}",
			new { controller = ForumController.Name, action = "Index", pageNumber = 1, Area = "Forums" },
			new { forum = forumConstraint }
		);
		app.MapControllerRoute(
			"pfroot",
			"Forums/{urlName}/{pageNumber}",
			new { controller = ForumController.Name, action = "Index", Area = "Forums" },
			new { forum = forumConstraint }
		);
		app.MapControllerRoute(
			"pftopic1",
			"Forums/Topic/{id}",
			new { controller = ForumController.Name, action = "Topic", pageNumber = 1, Area = "Forums" }
		);
		app.MapControllerRoute(
			"pftopic",
			"Forums/Topic/{id}/{pageNumber}",
			new { controller = ForumController.Name, action = "Topic", Area = "Forums" }
		);
		app.MapControllerRoute(
			"pflink",
			"Forums/PostLink/{id}",
			new { controller = ForumController.Name, action = "PostLink", Area = "Forums" }
		);
		app.MapControllerRoute(
			"pfsubtopics1",
			"Forums/Subscription/Topics",
			new { controller = SubscriptionController.Name, action = "Topics", pageNumber = 1, Area = "Forums" }
		);
		app.MapControllerRoute(
			"pfsubtopics",
			"Forums/Subscription/Topics/{pageNumber}",
			new { controller = SubscriptionController.Name, action = "Topics", Area = "Forums" }
		);
		app.MapControllerRoute(
			"pffavetopics1",
			"Forums/Favorites/Topics",
			new { controller = FavoritesController.Name, action = "Topics", pageNumber = 1, Area = "Forums" }
		);
		app.MapControllerRoute(
			"pffavetopics",
			"Forums/Favorites/Topics/{pageNumber}",
			new { controller = FavoritesController.Name, action = "Topics", Area = "Forums" }
		);
		app.MapControllerRoute(
			"pfpagedusertopics1",
			"Forums/Account/Posts/{id}",
			new { controller = AccountController.Name, action = "Posts", pageNumber = 1, Area = "Forums" }
		);
		app.MapControllerRoute(
			"pfpagedusertopics",
			"Forums/Account/Posts/{id}/{pageNumber}",
			new { controller = AccountController.Name, action = "Posts", Area = "Forums" }
		);
		app.MapControllerRoute(
			"pftopicunsub",
			"Forums/Subscription/Unsubscribe/{topicID}/{authKey}",
			new { controller = SubscriptionController.Name, action = "Unsubscribe", Area = "Forums" }
		);
		return app;
	}
}