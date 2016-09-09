using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Models;

namespace PopForums.Web.Extensions
{
	public static class ServiceCollections
	{
		public static IServiceCollection ConfigurePopForumsAuthorizationPolicies(this IServiceCollection services)
		{
			services.Configure<AuthorizationOptions>(options =>
			{
				options.AddPolicy(PermanentRoles.Admin, policy => policy.RequireClaim("ForumClaims", PermanentRoles.Admin));
				options.AddPolicy(PermanentRoles.Moderator, policy => policy.RequireClaim("ForumClaims", PermanentRoles.Moderator));
			});
			return services;
		}
	}
}