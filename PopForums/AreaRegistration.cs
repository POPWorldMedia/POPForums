using System.Collections.Generic;
using System.Web.Mvc;
using PopForums.Controllers;
using PopForums.Repositories;
using PopForums.Web;

namespace PopForums
{
	public class PopForumsAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "PopForums";
			}
		}

		public static class RouteName
		{
			public const string Default = "PopForums_Default";
			public const string ForumRoot = "PopForums_Root";
			public const string Topic = "PopForums_Topic";
			public const string TopicUnsubscribe = "PopForums_Unsubscribe";
			public const string SubscribedTopics = "PopForums_SubscribedTopics";
			public const string Recent = "PopForums_Recent";
			public const string FavoriteTopics = "PopForums_Favorite";
			public const string Setup = "PopForums_Setup";
			public const string PostLink = "PopForums_PostLink";
			public const string SearchQuery = "PopForums_SearchQuery";
			public const string PagedUserTopics = "PopForums_PagedUserTopics";
			public const string PagedAdminAction = "PopForums_PagedAdminAction";
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			var serviceLocator = PopForumsActivation.ServiceLocator;
			var forumRepository = serviceLocator.GetInstance<IForumRepository>();
			var nameSpaces = new List<string> {"PopForums.Controllers"};
			if (PopForumsActivation.AdditionalControllerNamespaces != null)
				nameSpaces.AddRange(PopForumsActivation.AdditionalControllerNamespaces);
			context.MapRoute(
				RouteName.Setup,
				"Forums/Setup",
				new { controller = SetupController.Name, action = "Index" },
				new[] { "PopForums.Controllers" }
				);
			context.MapRoute(
				RouteName.ForumRoot,
				"Forums/{urlName}/{page}",
				new { controller = ForumController.Name, action = "Index", page = 1 },
				new { forum = new ForumRouteConstraint(forumRepository) },
				new [] {"PopForums.Controllers"});
			context.MapRoute(
				RouteName.Recent,
				"Forums/Recent/{page}",
				new { controller = ForumController.Name, action = "Recent", page = 1 },
				new[] { "PopForums.Controllers" }
				);
			context.MapRoute(
				RouteName.Topic,
				"Forums/Topic/{id}/{page}",
				new { controller = ForumController.Name, action = "Topic", page = 1 },
				new[] { "PopForums.Controllers" }
				);
			context.MapRoute(
				RouteName.PostLink,
				"Forums/PostLink/{id}",
				new { controller = ForumController.Name, action = "PostLink" },
				new[] { "PopForums.Controllers" }
				);
			context.MapRoute(
				RouteName.SubscribedTopics,
				"Forums/Subscription/Topics/{page}",
				new { controller = SubscriptionController.Name, action = "Topics", page = 1 },
				new[] { "PopForums.Controllers" }
				);
			context.MapRoute(
				RouteName.FavoriteTopics,
				"Forums/Favorites/Topics/{page}",
				new { controller = FavoritesController.Name, action = "Topics", page = 1 },
				new[] { "PopForums.Controllers" }
				);
			context.MapRoute(
				RouteName.PagedUserTopics,
				"Forums/Account/Posts/{id}/{page}",
				new { controller = AccountController.Name, action = "Posts", page = 1 },
				new[] { "PopForums.Controllers" });
			context.MapRoute(
				RouteName.TopicUnsubscribe,
				"Forums/Subscription/Unsubscribe/{topicID}/{authKey}",
				new { controller = SubscriptionController.Name, action = "Unsubscribe" },
				new[] { "PopForums.Controllers" });
			context.MapRoute(
				RouteName.PagedAdminAction,
				"Forums/Admin/ErrorLog/{page}",
				new { controller = AdminController.Name, action = "ErrorLog", page = UrlParameter.Optional },
				new[] { "PopForums.Controllers" });
			context.MapRoute(
				RouteName.Default,
				"Forums/{controller}/{action}/{id}",
				new { controller = ForumHomeController.Name, action = "Index", id = UrlParameter.Optional },
				nameSpaces.ToArray());
		}
	}
}
