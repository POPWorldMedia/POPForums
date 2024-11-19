using PopForums.Mvc.Areas.Forums.Authentication;

namespace PopForums.Mvc.Areas.Forums.Extensions;

public static class ApplicationBuilders
{
	/// <summary>
	/// Enables the POP Forums middleware to identify PF users.
	/// </summary>
	public static IApplicationBuilder UsePopForumsAuth(this IApplicationBuilder app)
	{
		app.UseMiddleware<PopForumsAuthenticationMiddleware>();
		return app;
	}

	/// <summary>
	/// Enables the localization (languages) for POP Forums. Call this before UseMvc.
	/// </summary>
	public static IApplicationBuilder UsePopForumsCultures(this IApplicationBuilder app)
	{
		var supportedCultures = new List<CultureInfo> { new CultureInfo("en"), new CultureInfo("de"), new CultureInfo("es"), new CultureInfo("nl"), new CultureInfo("uk"), new CultureInfo("zh-TW") };
		app.UseRequestLocalization(new RequestLocalizationOptions
		{
			DefaultRequestCulture = new RequestCulture("en", "en"),
			SupportedCultures = supportedCultures,
			SupportedUICultures = supportedCultures
		});
		return app;
	}
}