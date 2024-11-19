namespace PopForums.Mvc.Areas.Forums.Authentication;

public class PopForumsAuthenticationMiddleware(RequestDelegate next)
{
	public async Task InvokeAsync(HttpContext context, IUserService userService, IProfileService profileService, ISetupService setupService, IConfig config, IOAuthOnlyService oAuthOnlyService)
	{
		var isSetupAndConnectionGood = setupService.IsRuntimeConnectionAndSetupGood();
		if (!isSetupAndConnectionGood)
		{
			await next.Invoke(context);
			return;
		}

		var endpoint = context.GetEndpoint();
		if (endpoint?.Metadata.GetMetadata<PopForumsAuthenticationIgnoreAttribute>() is not null)
		{
			await next.Invoke(context);
			return;
		}
		
		var authResult = await context.AuthenticateAsync(PopForumsAuthenticationDefaults.AuthenticationScheme);
		if (authResult.Principal?.Identity is ClaimsIdentity identity)
		{
			var user = await userService.GetUserByName(identity.Name);
			if (user != null)
			{
				foreach (var role in user.Roles)
					identity.AddClaim(new Claim(PopForumsAuthenticationDefaults.ForumsClaimType, role));
				identity.AddClaim(new Claim(PopForumsAuthenticationDefaults.ForumsUserIDType, user.UserID.ToString()));
				context.Items["PopForumsUser"] = user;
				var profile = await profileService.GetProfile(user);
				context.Items["PopForumsProfile"] = profile;
				context.User = new ClaimsPrincipal(identity);
				if (config.IsOAuthOnly && user.TokenExpiration < DateTime.UtcNow)
				{
					var isSuccess = await oAuthOnlyService.AttemptTokenRefresh(user);
					if (!isSuccess)
						context.Response.Redirect("/Forums/Account/Login");
				}
			}
		}
		await next.Invoke(context);
	}
}