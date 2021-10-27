using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PopForums.AzureKit;
using PopForums.Mvc.Areas.Forums.Authorization;
using PopForums.Mvc.Areas.Forums.Extensions;
using PopForums.Sql;
using System.Text.Json.Serialization;
using PopForums.ElasticKit;
using PopForums.Extensions;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllersWithViews();
services.AddRazorPages();

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

services.AddControllers().AddJsonOptions(options =>
{
	// Use this to make sure enums are serialized correctly
	options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
	options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

// set up the dependencies for the SQL library in POP Forums
services.AddPopForumsSql();
// this adds dependencies from the MVC project (and base dependencies) and sets up authentication for the forum
services.AddPopForumsMvc();

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



var app = builder.Build();

// Records exceptions and info to the POP Forums database.
var loggerFactory = app.Services.GetService<ILoggerFactory>();
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

app.Run();