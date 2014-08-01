using System;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using PopForums.ExternalLogin;
using PopForums.Services;
using PopForums.Web;

namespace PopForums.Configuration
{
	public class PopForumsOwinStartup
	{
		public void Configuration(IAppBuilder app)
		{
			var setupService = PopForumsActivation.ServiceLocator.GetInstance<ISetupService>();
			if (!setupService.IsDatabaseSetup())
				return;

			var settings = PopForumsActivation.ServiceLocator.GetInstance<ISettingsManager>().Current;

			app.SetDefaultSignInAsAuthenticationType(ExternalAuthentication.ExternalCookieName);

			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AuthenticationType = ExternalAuthentication.ExternalCookieName,
				AuthenticationMode = AuthenticationMode.Passive,
				CookieName = CookieAuthenticationDefaults.CookiePrefix + ExternalAuthentication.ExternalCookieName,
				ExpireTimeSpan = TimeSpan.FromMinutes(60)
			});

			if (settings.UseTwitterLogin)
				app.UseTwitterAuthentication(
				   consumerKey: settings.TwitterConsumerKey,
				   consumerSecret: settings.TwitterConsumerSecret);

			if (settings.UseMicrosoftLogin)
				app.UseMicrosoftAccountAuthentication(
					clientId: settings.MicrosoftClientID,
					clientSecret: settings.MicrosoftClientSecret);

			if (settings.UseFacebookLogin)
				app.UseFacebookAuthentication(
				   appId: settings.FacebookAppID,
				   appSecret: settings.FacebookAppSecret);

			if (settings.UseGoogleLogin)
				app.UseGoogleAuthentication(settings.GoogleClientId, settings.GoogleClientSecret);
		}
	}
}