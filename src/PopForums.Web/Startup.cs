using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PopForums.Configuration;
using PopForums.Data.Sql;
using PopForums.Extensions;
using PopForums.ExternalLogin;
using PopForums.Messaging;
using PopForums.Repositories;
using PopForums.Web.Areas.Forums;
using PopForums.Web.Areas.Forums.Controllers;
using PopForums.Web.Areas.Forums.Messaging;
using PopForums.Web.Areas.Forums.Services;

namespace PopForums.Web
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			// Setup configuration sources.
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("config.json")
				.AddEnvironmentVariables();
			Configuration = builder.Build();

			Config.SetPopForumsAppEnvironment(env.ContentRootPath);
		}

		public IConfigurationRoot Configuration { get; set; }

		// This method gets called by the runtime.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddAuthentication(options => options.SignInScheme = ExternalExternalUserAssociationManager.AuthenticationContextName);

			// Add MVC services to the services container.
			services.AddMvc();

			// TODO: go to primary nuget
			services.AddSignalR();

			services.AddPopForumsBase();
			services.AddPopForumsSql();
			// TODO: how to package mappings in web project
			services.AddTransient<IUserRetrievalShim, UserRetrievalShim>();
			services.AddTransient<ITopicViewCountService, TopicViewCountService>();
			services.AddTransient<IMobileDetectionWrapper, MobileDetectionWrapper>();
			services.AddTransient<IBroker, Broker>();
		}

		// Configure is called after ConfigureServices is called.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			if (env.IsDevelopment())
			{
				app.UseBrowserLink();
				app.UseDeveloperExceptionPage();
				app.UseDatabaseErrorPage();
			}
			else
			{
				//app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme,
				AutomaticAuthenticate = true
			});
			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AuthenticationScheme = ExternalExternalUserAssociationManager.AuthenticationContextName,
				CookieName = ExternalExternalUserAssociationManager.AuthenticationContextName,
				ExpireTimeSpan = TimeSpan.FromMinutes(5)
			});

			//app.UseFacebookAuthentication(new FacebookOptions
			//{
			//	AppId = "1932812290276605",
			//	AppSecret = "d2cfaebc3b565bdcfc7782b72aab98d8"
			//});

			//app.UseGoogleAuthentication(new GoogleOptions
			//{
			//	ClientId = "319472452413-ojveqkrcdfufi84le0sm7044sqfqkhfj.apps.googleusercontent.com",
			//	ClientSecret = "n_ZhNUpEaGJyhnfIHBSWyhbx"
			//});

			app.UseMiddleware<PopForumsMiddleware>();

			app.UseSignalR();

			// Add MVC to the request pipeline.
			app.UseMvc(routes =>
			{

				// forum routes

				routes.MapRoute(
					"pfsetup",
					"Forums/Setup",
					new { controller = "Setup", action = "Index", Area = "Forums" }
					);
				routes.MapRoute(
					"pfrecent",
					"Forums/Recent/{page?}",
					new { controller = ForumController.Name, action = "Recent", page = 1, Area = "Forums" }
					);
				var forumRepository = app.ApplicationServices.GetService<IForumRepository>();
				var forumConstraint = new ForumRouteConstraint(forumRepository);
				routes.MapRoute(
					"pfroot",
					"Forums/{urlName}/{page?}",
					new { controller = ForumController.Name, action = "Index", page = 1, Area = "Forums" },
					new { forum = forumConstraint }
					);
				routes.MapRoute(
					"pftopic",
					"Forums/Topic/{id}/{page?}",
					new { controller = ForumController.Name, action = "Topic", page = 1, Area = "Forums" }
					);
				routes.MapRoute(
					"pflink",
					"Forums/PostLink/{id}",
					new { controller = ForumController.Name, action = "PostLink", Area = "Forums" }
					);
				routes.MapRoute(
					"pfsubtopics",
					"Forums/Subscription/Topics/{page?}",
					new { controller = SubscriptionController.Name, action = "Topics", page = 1, Area = "Forums" }
					);
				routes.MapRoute(
					"pffavetopics",
					"Forums/Favorites/Topics/{page?}",
					new { controller = FavoritesController.Name, action = "Topics", page = 1, Area = "Forums" }
					);
				routes.MapRoute(
					"pfpagedudertopics",
					"Forums/Account/Posts/{id}/{page?}",
					new { controller = AccountController.Name, action = "Posts", page = 1, Area = "Forums" }
					);
				routes.MapRoute(
					"pftopicunsub",
					"Forums/Subscription/Unsubscribe/{topicID}/{authKey}",
					new { controller = SubscriptionController.Name, action = "Unsubscribe", Area = "Forums" }
					);
				routes.MapRoute(
					"pfpagedadmin",
					"Forums/Admin/ErrorLog/{page?}",
					new { controller = AdminController.Name, action = "ErrorLog", Area = "Forums" }
					);

				// app routes

				routes.MapRoute(
					name: "areaRoute",
					template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
