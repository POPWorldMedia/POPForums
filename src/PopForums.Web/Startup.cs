using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PopForums.ElasticKit;
using PopForums.AzureKit;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.Mvc.Areas.Forums.Authorization;
using PopForums.Mvc.Areas.Forums.Extensions;
using PopForums.Sql;
using PopForums.Mvc.Areas.Forums.Services;

namespace PopForums.Web
{
	public class Startup
	{
		public Startup(IWebHostEnvironment env)
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
			services.Configure<AuthorizationOptions>(options =>
			{
				// sets claims policies for admin and moderator functions in POP Forums
				options.AddPopForumsPolicies();
			});

			services.AddMvc(options =>
			{
				// identifies users on POP Forums actions
				options.Filters.Add(typeof(PopForumsUserAttribute));
			}).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

			// It's unfortunately necessary to use the Json.NET serializer for API requests because System.Text.Json doesn't handler enums correctly
			services.AddControllers().AddNewtonsoftJson();

			// set up the dependencies for the SQL library in POP Forums
			services.AddPopForumsSql();
			// this adds dependencies from the MVC project (and base dependencies) and sets up authentication for the forum
			services.AddPopForumsMvc();

			// Add the service to auto provision accounts from external logins when ExternalLoginOnly is enabled
			services.AddTransient<IAutoProvisionAccountService, AutoProvisionAccountService>();

			// use Redis cache for POP Forums using AzureKit
			//services.AddPopForumsRedisCache();

			// required for real-time updating of POP Forums
			services.AddSignalR();
			// use this instead of previous line if you need to route SignalR messages
			// over a Redis backplane for multi-instance host
			//services.AddSignalR().AddRedisBackplaneForPopForums();

			// use Azure Search for POP Forums using AzureKit
			//services.AddPopForumsAzureSearch();

			// use ElasticSearch for POP Forums using ElasticKit
			//services.AddPopForumsElasticSearch();

			// use Azure Functions queues for POP Forums using AzureKit for background tasks...
			// do NOT call AddPopForumsBackgroundServices()
			services.AddPopForumsAzureFunctionsAndQueues();

			// creates an instance of the background services for POP Forums... call this last in forum setup,
			// but don't use if you're running these in functions
			//services.AddPopForumsBackgroundServices();
		}

		public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
		{
			// Records exceptions and info to the POP Forums database.
			loggerFactory.AddPopForumsLogger(app);

			// Enables languages
			app.UsePopForumsCultures();

			app.UseStaticFiles();

			// Not unique to POP Forums, but required. Call before UsePopForumsAuth().
			app.UseAuthentication();

			// Populate the POP Forums identity in every request.
			app.UsePopForumsAuth();

			app.UseDeveloperExceptionPage();

			// Add MVC to the request pipeline. The order of the next three lines matters:
			app.UseRouting();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				// POP Forums routes
				endpoints.AddPopForumsEndpoints(app);

				// need this if you have lots of routing and/or areas
				endpoints.MapAreaControllerRoute(
					"forumroutes", "forums",
					"Forums/{controller=Home}/{action=Index}/{id?}");

				// app routes
				endpoints.MapControllerRoute(
					"areaRoute",
					"{area:exists}/{controller=Home}/{action=Index}/{id?}");
				endpoints.MapControllerRoute(
					"default",
					"{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}