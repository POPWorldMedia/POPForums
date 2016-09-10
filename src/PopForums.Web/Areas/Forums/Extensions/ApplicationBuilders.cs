using Microsoft.AspNetCore.Builder;
using PopForums.Web.Areas.Forums.Authorization;

namespace PopForums.Web.Areas.Forums.Extensions
{
	public static class ApplicationBuilders
	{
		/// <summary>
		/// Sets up the cookie based authentication for POP Forums.
		/// </summary>
		/// <param name="app"></param>
		/// <returns></returns>
		public static IApplicationBuilder UseCookieAuthenticationForPopForums(this IApplicationBuilder app)
		{
			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AuthenticationScheme = PopForumsAuthorizationDefaults.AuthenticationScheme,
				CookieName = PopForumsAuthorizationDefaults.CookieName,
				AutomaticAuthenticate = true
			});
			return app;
		}
	}
}