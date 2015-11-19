using System;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.Google;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using PopForums.Configuration;
using PopForums.Data.Sql;
using PopForums.Extensions;
using PopForums.ExternalLogin;
using PopForums.Repositories;
using PopForums.Web.Areas.Forums;
using PopForums.Web.Areas.Forums.Controllers;
using PopForums.Web.Areas.Forums.Services;

namespace PopForums.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.
            var builder = new ConfigurationBuilder()
				.SetBasePath(appEnv.ApplicationBasePath)
				.AddJsonFile("config.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();

			Config.SetPopForumsAppEnvironment(appEnv.ApplicationBasePath);
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime.
        public void ConfigureServices(IServiceCollection services)
        {
	        services.AddAuthentication(options => options.SignInScheme = ExternalExternalUserAssociationManager.AuthenticationContextName);

			// Add MVC services to the services container.
			services.AddMvc();

			services.AddPopForumsBase();
			services.AddPopForumsSql();
			// TODO: how to package mappings in web project
	        services.AddTransient<IUserRetrievalShim, UserRetrievalShim>();
	        services.AddTransient<ITopicViewCountService, TopicViewCountService>();
	        services.AddTransient<IMobileDetectionWrapper, MobileDetectionWrapper>();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Information;
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

			// Configure the HTTP request pipeline.

			// Add the platform handler to the request pipeline.
			app.UseIISPlatformHandler();

			// Add static files to the request pipeline.
			app.UseStaticFiles();

			app.UseDeveloperExceptionPage();

			app.UseCookieAuthentication(options =>
	        {
				options.AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.AutomaticAuthentication = true;
	        });
	        app.UseCookieAuthentication(options =>
	        {
		        options.AuthenticationScheme = ExternalExternalUserAssociationManager.AuthenticationContextName;
		        options.CookieName = ExternalExternalUserAssociationManager.AuthenticationContextName;
		        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
	        });

	        app.UseFacebookAuthentication(options =>
	        {
		        options.AppId = "1932812290276605";
		        options.AppSecret = "d2cfaebc3b565bdcfc7782b72aab98d8";
	        });

	        app.UseGoogleAuthentication(options =>
	        {
		        options.ClientId = "319472452413-ojveqkrcdfufi84le0sm7044sqfqkhfj.apps.googleusercontent.com";
		        options.ClientSecret = "n_ZhNUpEaGJyhnfIHBSWyhbx";
			});

			app.UseMiddleware<PopForumsMiddleware>();
			
            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "areaRoute",
					template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

				// forum routes

				routes.MapRoute(
					"pfsetup",
					"Forums/Setup",
					new { controller = "Setup", action = "Index" }
					);
				routes.MapRoute(
					"pfrecent",
					"Forums/Recent/{page}",
					new { controller = ForumController.Name, action = "Recent", page = 1 }
					);
	            var forumRepository = app.ApplicationServices.GetService<IForumRepository>();
	            var forumConstraint = new ForumRouteConstraint(forumRepository);
				routes.MapRoute(
					"pfroot",
					"Forums/{urlName}/{page?}",
					new { controller = ForumController.Name, action = "Index", page = 1 },
					new { forum = forumConstraint }
					);
				routes.MapRoute(
					"pftopic",
					"Forums/Topic/{id}/{page?}",
					new { controller = ForumController.Name, action = "Topic", page = 1 }
					);
				routes.MapRoute(
					"pflink",
					"Forums/PostLink/{id}",
					new { controller = ForumController.Name, action = "PostLink" }
					);
				routes.MapRoute(
					"pfsubtopics",
					"Forums/Subscription/Topics/{page?}",
					new { controller = SubscriptionController.Name, action = "Topics", page = 1 }
					);
				routes.MapRoute(
					"pffavetopics",
					"Forums/Favorites/Topics/{page?}",
					new { controller = FavoritesController.Name, action = "Topics", page = 1 }
					);
				routes.MapRoute(
					"pfpagedudertopics",
					"Forums/Account/Posts/{id}/{page?}",
					new { controller = AccountController.Name, action = "Posts", page = 1 }
					);
				routes.MapRoute(
					"pftopicunsub",
					"Forums/Subscription/Unsubscribe/{topicID}/{authKey}",
					new { controller = SubscriptionController.Name, action = "Unsubscribe" }
					);
				routes.MapRoute(
					"pfpagedadmin",
					"Forums/Admin/ErrorLog/{page?}",
					new { controller = AdminController.Name, action = "ErrorLog" }
					);
				routes.MapRoute(
					"pfdefault",
					"Forums/{controller=Home}/{action=Index}/{id?}");

				// app routes

				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
        }
    }
}
