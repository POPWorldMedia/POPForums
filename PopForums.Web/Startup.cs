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
				routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
