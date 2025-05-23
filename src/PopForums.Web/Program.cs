﻿using Microsoft.AspNetCore.Authorization;
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
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// the following block is for use when you have multiple nodes running
// (think Azure App Services) so they can all decode the auth cookie
var configuration = builder.Configuration;
if (configuration["DataProtectBlobConnectionString"] != null)
{
	services.AddDataProtection()
		.SetApplicationName("popforumsdev")
		.PersistKeysToAzureBlobStorage(configuration["DataProtectBlobConnectionString"], "keys", "antiforge");
}

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

// use Azure table storage for logging instead of database
//services.AddPopForumsTableStorageLogging();

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
// do NOT call AddPopForumsBackgroundJobs()
services.AddPopForumsAzureFunctionsAndQueues();

// persist image uploads to Azure blob storage, see configuration
services.AddPopForumsAzureBlobStorageForPostImages();

// creates an instance of the background services for POP Forums... call this last in forum setup,
// but don't use if you're running these in functions with AddPopForumsAzureFunctionsAndQueues()
//services.AddPopForumsBackgroundJobs();

// send fewer bits
services.AddResponseCompression(options =>
{
	options.Providers.Add<BrotliCompressionProvider>();
	options.Providers.Add<GzipCompressionProvider>();
	options.EnableForHttps = true;
});
services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	// send fewer bits
	app.UseResponseCompression();
}

// Records exceptions and info to the POP Forums database.
var loggerFactory = app.Services.GetService<ILoggerFactory>();
loggerFactory.AddPopForumsLogger(app);

app.UseStaticFiles();

// Enables languages
app.UsePopForumsCultures();

// Not unique to POP Forums, but required. Call before UsePopForumsAuth().
app.UseAuthentication();

app.UseDeveloperExceptionPage();

// Add MVC to the request pipeline. The order of the next three lines matters:
app.UseRouting();

// Populate the POP Forums identity in every request.
// Possible breaking change starting in v21. This must be called after UseRouting()
// but before UseAuthorization() and endpoint mapping.
app.UsePopForumsAuth();

app.UseAuthorization();

// POP Forums routes
app.AddPopForumsEndpoints();

// app routes
app.MapControllerRoute(
	"areaRoute",
	"{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
	"default",
	"{controller=Home}/{action=Index}/{id?}");

app.Run();