using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Configuration;
using PopForums.ExternalLogin;
using PopForums.Services;
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
			var setupService = app.ApplicationServices.GetService<ISetupService>();
			if (!setupService.IsConnectionPossible() || !setupService.IsDatabaseSetup())
				return app;
			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AuthenticationScheme = PopForumsAuthorizationDefaults.AuthenticationScheme,
				CookieName = PopForumsAuthorizationDefaults.CookieName,
				AutomaticAuthenticate = true
			});

			var settingsManager = app.ApplicationServices.GetService<ISettingsManager>();
			var settings = settingsManager.Current;

			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AuthenticationScheme = ExternalUserAssociationManager.AuthenticationContextName,
				CookieName = ExternalUserAssociationManager.AuthenticationContextName,
				ExpireTimeSpan = TimeSpan.FromMinutes(5)
			});

			if (settings.UseTwitterLogin)
				app.UseTwitterAuthentication(new TwitterOptions
				{
					ConsumerKey = settings.TwitterConsumerKey,
					ConsumerSecret = settings.TwitterConsumerSecret
				});

			if (settings.UseMicrosoftLogin)
				app.UseMicrosoftAccountAuthentication(new MicrosoftAccountOptions
				{
					ClientId = settings.MicrosoftClientID,
					ClientSecret = settings.MicrosoftClientSecret
				});

			if (settings.UseFacebookLogin)
				app.UseFacebookAuthentication(new FacebookOptions
				{
					AppId = settings.FacebookAppID,
					AppSecret = settings.FacebookAppSecret
				});

			if (settings.UseGoogleLogin)
				app.UseGoogleAuthentication(new GoogleOptions
				{
					ClientId = settings.GoogleClientId,
					ClientSecret = settings.GoogleClientSecret
				});

			return app;
		}
	}
}