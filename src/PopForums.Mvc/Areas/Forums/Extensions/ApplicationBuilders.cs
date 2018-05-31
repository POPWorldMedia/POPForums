using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Mvc.Areas.Forums.Authorization;
using PopForums.Mvc.Areas.Forums.Messaging;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Extensions
{
	public static class ApplicationBuilders
	{
		public static IApplicationBuilder UsePopForumsSignalR(this IApplicationBuilder app)
		{
			app.UseSignalR(routes =>
			{
				routes.MapHub<TopicsHub>("/TopicsHub");
				routes.MapHub<RecentHub>("/RecentHub");
				routes.MapHub<ForumsHub>("/ForumsHub");
				routes.MapHub<FeedHub>("/FeedHub");
			});
			return app;
		}

		public static IApplicationBuilder UsePopForumsAuth(this IApplicationBuilder app)
		{
			app.Use(async (context, next) =>
			{
				var authResult = context.AuthenticateAsync(PopForumsAuthorizationDefaults.AuthenticationScheme).Result;
				var identity = authResult?.Principal?.Identity as ClaimsIdentity;
				if (identity != null)
				{
					var userService = app.ApplicationServices.GetService<IUserService>();
					var user = userService.GetUserByName(identity.Name);
					if (user != null)
					{
						foreach (var role in user.Roles)
							identity.AddClaim(new Claim(PopForumsAuthorizationDefaults.ForumsClaimType, role));
						context.Items["PopForumsUser"] = user;
						var profileService = context.RequestServices.GetService<IProfileService>();
						var profile = profileService.GetProfile(user);
						context.Items["PopForumsProfile"] = profile;
						context.User = new ClaimsPrincipal(identity);
					}
				}
				await next.Invoke();
			});
			return app;
		}
	}
}