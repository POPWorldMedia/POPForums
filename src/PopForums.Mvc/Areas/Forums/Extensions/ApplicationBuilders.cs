using Microsoft.AspNetCore.Builder;
using PopForums.Mvc.Areas.Forums.Messaging;

namespace PopForums.Mvc.Areas.Forums.Extensions
{
	public static class ApplicationBuilders
	{
		public static IApplicationBuilder UsePopForumsSignalR(this IApplicationBuilder app)
		{
			app.UseSignalR(routes =>
			{
				routes.MapHub<TopicsHub>("TopicsHub");
				routes.MapHub<RecentHub>("RecentHub");
				routes.MapHub<ForumsHub>("ForumsHub");
				routes.MapHub<FeedHub>("FeedHub");
			});
			return app;
		}
	}
}