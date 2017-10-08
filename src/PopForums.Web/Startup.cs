using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using PopForums.AzureKit;
using PopForums.Configuration;
using PopForums.Data.Sql;
using PopForums.Extensions;
using PopForums.ExternalLogin;
using PopForums.Mvc.Areas.Forums.Extensions;
using PopForums.Mvc.Areas.Forums.Authorization;
using PopForums.Services;

namespace PopForums.Web
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
			//services.AddAuthentication(options => options.SignInScheme = ExternalUserAssociationManager.AuthenticationContextName);

			services.Configure<AuthorizationOptions>(options =>
			{
				// sets claims policies for admin and moderator functions in POP Forums
				options.AddPopForumsPolicies();
			});

			services.AddMvc(options =>
			{
				// identifies users on POP Forums actions
				options.Filters.Add(typeof(PopForumsUserAttribute));
			})
			// TODO: Remove once bug is fixed: https://github.com/aspnet/Mvc/issues/6660
			.AddRazorOptions(options =>
			{
				options.ViewLocationExpanders.Remove(options.ViewLocationExpanders.First(f => f is Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure.PageViewLocationExpander));
			});
			
			services.AddSignalR();

			// sets up the dependencies for the base, SQL and web libraries in POP Forums
			services.AddPopForumsBase();
			services.AddPopForumsSql();
			// this adds dependencies from the MVC project and sets up authentication for the forum
			services.AddPopForumsMvc();

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

			// Not unique to POP Forums, but required.
			app.UseAuthentication();

			// TODO: refactor to extension method, moved out of PopForumsUserAttribute so SignalR hubs have user
			app.Use(async (context, next) =>
			{
				var authResult = context.AuthenticateAsync(PopForumsAuthorizationDefaults.AuthenticationScheme).Result;
				var identity = authResult?.Principal?.Identity as ClaimsIdentity;
				if (identity != null)
				{
					var userService = app.ApplicationServices.GetService<IUserService>();
					var user = userService.GetUserByName(identity.Name);
					if (user != null)
					{
						foreach (var role in user.Roles)
							identity.AddClaim(new Claim(PopForumsAuthorizationDefaults.ForumsClaimType, role));
						context.Items["PopForumsUser"] = user;
						var profileService = context.RequestServices.GetService<IProfileService>();
						var profile = profileService.GetProfile(user);
						context.Items["PopForumsProfile"] = profile;
						context.User = new ClaimsPrincipal(identity);
					}
				}
				await next.Invoke();
			});




			app.UsePopForumsSignalR();

			app.UseDeveloperExceptionPage();

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
            app.UseRequestLocalization(new RequestLocalizationOptions {
                DefaultRequestCulture = new RequestCulture("en", "en"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });
			//CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("es");
			//CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("es");
		}
	}
}
