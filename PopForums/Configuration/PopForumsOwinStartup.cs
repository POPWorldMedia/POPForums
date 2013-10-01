using System;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Ninject;
using Owin;
using PopForums.Configuration;
using PopForums.ExternalLogin;
using PopForums.Web;

[assembly: OwinStartup(typeof (PopForumsOwinStartup))]
namespace PopForums.Configuration
{
	public class PopForumsOwinStartup
	{
		public void Configuration(IAppBuilder app)
		{
			var settings = PopForumsActivation.Kernel.Get<ISettingsManager>().Current;

			app.SetDefaultSignInAsAuthenticationType(ExternalAuthentication.ExternalCookieName);

			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AuthenticationType = ExternalAuthentication.ExternalCookieName,
				AuthenticationMode = AuthenticationMode.Passive,
				CookieName = CookieAuthenticationDefaults.CookiePrefix + ExternalAuthentication.ExternalCookieName,
				ExpireTimeSpan = TimeSpan.FromMinutes(5),
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
				app.UseGoogleAuthentication();

			app.MapHubs();
		}
	}
}