namespace PopForums.Mvc.Areas.Forums.Extensions;

public static class AuthorizationOptionsExtensions
{
	/// <summary>
	/// Adds authorization options to require certain claims for moderator and admin roles in POP Forums.
	/// </summary>
	/// <param name="options"></param>
	public static void AddPopForumsPolicies(this AuthorizationOptions options)
	{
		options.AddPolicy(PermanentRoles.Admin, policy => policy.RequireClaim(PopForumsAuthorizationDefaults.ForumsClaimType, PermanentRoles.Admin));
		options.AddPolicy(PermanentRoles.Moderator, policy => policy.RequireClaim(PopForumsAuthorizationDefaults.ForumsClaimType, PermanentRoles.Moderator));
	}
}