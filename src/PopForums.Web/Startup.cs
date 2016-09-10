using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PopForums.Configuration;
using PopForums.Data.Sql;
using PopForums.Extensions;
using PopForums.ExternalLogin;
using PopForums.Web.Areas.Forums.Authorization;
using PopForums.Web.Areas.Forums.Extensions;

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
			services.AddAuthentication(options => options.SignInScheme = ExternalUserAssociationManager.AuthenticationContextName);

			services.Configure<AuthorizationOptions>(options =>
			{
				options.AddPopForumsPolicies();
			});

			// Add MVC services to the services container.
			services.AddMvc(options =>
			{
				options.Filters.Add(typeof(PopForumsUserAttribute));
			});

			// TODO: go to primary nuget
			services.AddSignalR();

			services.AddPopForumsBase();
			services.AddPopForumsSql();
			services.AddPopForumsWeb();

			services.AddPopForumsBackgroundServices();
		}

		// Configure is called after ConfigureServices is called.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();
			loggerFactory.AddPopForumsLogger(app);

			if (env.IsDevelopment())
			{
				//app.UseBrowserLink();
				app.UseDeveloperExceptionPage();
				app.UseDatabaseErrorPage();
			}

			app.UseStaticFiles();

			app.UseCookieAuthenticationForPopForums();

			//app.UseCookieAuthentication(new CookieAuthenticationOptions
			//{
			//	AuthenticationScheme = ExternalUserAssociationManager.AuthenticationContextName,
			//	CookieName = ExternalUserAssociationManager.AuthenticationContextName,
			//	ExpireTimeSpan = TimeSpan.FromMinutes(5)
			//});

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

			app.UseSignalR();

			// Add MVC to the request pipeline.
			app.UseMvc(routes =>
			{

				// forum routes
				routes.AddPopForumsRoutes(app);

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
