using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Authorization
{
	public class PopForumsAuthorizationMiddleware
	{
		private readonly RequestDelegate _next;

		public PopForumsAuthorizationMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public Task InvokeAsync(HttpContext context, IUserService userService, IProfileService profileService, ISetupService setupService)
		{
			if (!setupService.IsRuntimeConnectionAndSetupGood())
				return _next.Invoke(context);
			var authResult = context.AuthenticateAsync(PopForumsAuthorizationDefaults.AuthenticationScheme).Result;
			var identity = authResult?.Principal?.Identity as ClaimsIdentity;
			if (identity != null)
			{
				var user = userService.GetUserByName(identity.Name).Result;
				if (user != null)
				{
					foreach (var role in user.Roles)
						identity.AddClaim(new Claim(PopForumsAuthorizationDefaults.ForumsClaimType, role));
					context.Items["PopForumsUser"] = user;
					var profile = profileService.GetProfile(user).Result;
					context.Items["PopForumsProfile"] = profile;
					context.User = new ClaimsPrincipal(identity);
				}
			}
			return _next.Invoke(context);
		}
	}
}