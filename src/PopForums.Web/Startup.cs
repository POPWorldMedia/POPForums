using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PopForums.AzureKit;
using PopForums.Configuration;
using PopForums.Data.Sql;
using PopForums.Extensions;
using PopForums.ExternalLogin;
using PopForums.Mvc.Areas.Forums.Extensions;
using System.Collections.Generic;
using System.Globalization;
using PopForums.Mvc.Areas.Forums.Authorization;

namespace PopForums.Mvc
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			// Setup configuration sources.
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json")
				.AddEnvironmentVariables();
			Configuration = builder.Build();

			// setup PopForums.json config file
			Config.SetPopForumsAppEnvironment(env.ContentRootPath);
		}

		public IConfigurationRoot Configuration { get; set; }

		public void ConfigureServices(IServiceCollection services)
		{
			// needed for social logins in POP Forums
			services.AddAuthentication(options => options.SignInScheme = ExternalUserAssociationManager.AuthenticationContextName);

			services.Configure<AuthorizationOptions>(options =>
			{
				// sets claims policies for admin and moderator functions in POP Forums
				options.AddPopForumsPolicies();
			});

			services.AddMvc(options =>
			{
				// identifies users on POP Forums actions
				options.Filters.Add(typeof(PopForumsUserAttribute));
			});

			// TODO: go to primary nuget
			services.AddSignalR();

			// sets up the dependencies for the base, SQL and web libraries in POP Forums
			services.AddPopForumsBase();
			services.AddPopForumsSql();
			services.AddPopForumsWeb();

			// use Redis cache for POP Forums using AzureKit
			//services.AddPopForumsRedisCache();

			// use Azure Search for POP Forums using AzureKit
			//services.AddPopForumsAzureSearch();

			// creates an instance of the background services for POP Forums... call this last in forum setup
			services.AddPopForumsBackgroundServices();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();
			// records exceptions and info to the POP Forums database
			loggerFactory.AddPopForumsLogger(app);

			app.UseStaticFiles();

			// sets up POP Forums auth and includes social logins if setup in admin
			app.UseCookieAuthenticationForPopForums();

			app.UseSignalR();

			// Error page config has to come before the Authentication has been setup, as some of 
			// options will double execute population of HttpContext.User.Identities.
			// see: https://github.com/POPWorldMedia/POPForums/issues/54
			// The error handling also has to come before the routing allowing us to navigate to
			// controller which handles it.
			if (env.IsDevelopment())
			{
				//app.UseBrowserLink();
				app.UseDeveloperExceptionPage();
				app.UseDatabaseErrorPage();
				app.UseStatusCodePages();
			}
			else
			{
				app.UseStatusCodePagesWithReExecute("/Error/{0}");
				app.UseExceptionHandler("/Error");
			}

			// Add MVC to the request pipeline.
			app.UseMvc(routes =>
			{
				// POP Forums routes
				routes.AddPopForumsRoutes(app);

				// app routes

				routes.MapRoute(
					name: "areaRoute",
					template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});

			// TODO: abstract this
			var supportedCultures = new List<CultureInfo> { new CultureInfo("en"), new CultureInfo("de"), new CultureInfo("es"), new CultureInfo("nl"), new CultureInfo("uk"), new CultureInfo("zh-TW") };
			app.UseRequestLocalization(new RequestLocalizationOptions
			{
				DefaultRequestCulture = new RequestCulture("en", "en"),
				SupportedCultures = supportedCultures,
				SupportedUICultures = supportedCultures
			});
			//CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("es");
			//CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("es");
		}
	}
}
