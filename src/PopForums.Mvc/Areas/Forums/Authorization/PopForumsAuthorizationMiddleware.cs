namespace PopForums.Mvc.Areas.Forums.Authorization;

public class PopForumsAuthorizationMiddleware
{
	private readonly RequestDelegate _next;

	public PopForumsAuthorizationMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context, IUserService userService, IProfileService profileService, ISetupService setupService)
	{
		var isSetupAndConnectionGood = setupService.IsRuntimeConnectionAndSetupGood();
		if (!isSetupAndConnectionGood)
		{
			await _next.Invoke(context);
			return;
		}
		var authResult = await context.AuthenticateAsync(PopForumsAuthorizationDefaults.AuthenticationScheme);
		var identity = authResult?.Principal?.Identity as ClaimsIdentity;
		if (identity != null)
		{
			var user = userService.GetUserByName(identity.Name).Result;
			if (user != null)
			{
				foreach (var role in user.Roles)
					identity.AddClaim(new Claim(PopForumsAuthorizationDefaults.ForumsClaimType, role));
				context.Items["PopForumsUser"] = user;
				var profile = await profileService.GetProfile(user);
				context.Items["PopForumsProfile"] = profile;
				context.User = new ClaimsPrincipal(identity);
			}
		}
		await _next.Invoke(context);
	}
}