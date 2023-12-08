---
layout: default
title: Using Azure Kit Library
nav_order: 6
---
# Using AzureKit Library
The `PopForums.AzureKit` library makes it possible to wire up the following scenarios:
* Using Redis for caching (not dependent on Azure specifically... Redis runs everywhere!)
* Using Azure Storage queues and Functions to queue work for search indexing, emailing and scoring game award calculation
* Using Azure Storage for image uploads
* Using Azure Search
* Using Azure Storage for hosting uploaded images in posts
* Using Azure Table Storage for error logging

You don't need to use the AzureKit components to run in an Azure App Service. These components are intended for making scale-out possible. The web app can run self-contained in a single node on an Azure App Service without these optional bits.

## Configuration with Azure App Services and Azure Functions

The POP Forums configuration system uses the typical configuration files, but adhere's to the overriding system implemented via environment variables. This by extension means that you can set these values in the Application Settings section of the Azure portal for App Services and Functions. It uses the colon notation that you may be familiar with. For example, use `PopForums:Queue:ConnectionString` to correspond to the hierarchy of the `appsettings.json file`. (For Linux-based App Services and Functions in Azure, use a double underscore instead of colons in the portal settings, i.e., `PopForums__Queue__ConnectionString`.)

## Irrelevant settings when using Azure Functions

Once you get the background stuff out of the web app context, some of the configuration options in the admin are no longer applicable.
* In email: The sending interval and mailer quantity no longer matter, because the functions only respond when there's something in the queue, and scale as necessary. You may need to limit the number of instances via host.json or the Azure portal if your email service provider throttles your email delivery.
* In search: The search indexing interval only reacts when something is queued (like email). Furthermore, if you use Azure Search or ElasticSearch, the junk words no longer apply, as these indexing strategies are handled by the appropriate service.
* In scoring game: The interval is again irrelevant because of the queue.

## Running locally
You can almost run everything in this stack locally. Here's the breakdown:
* Redis is easy to run locally using a Docker container: `docker run -p 6379:6379 -d redis`
* Azure Storage (for queues) can be simulated locally running [Azurite](https://github.com/azure/azurite) on Windows or Mac (the Azure Storage Emulator has been deprecated). Run this in a Docker container with `docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite`
* Azure Functions CLI runs on Windows and Mac.
* Azure Search only runs in Azure.
* ElasticSearch can run in a Docker container.

## Setting locale in Azure Functions
Since Azure Functions do not run as a normal web app, listening to the locale of the user's web browser, it defaults to whatever Azure decides is default, probably `en-US` in a lot of places. For some of the functions that are generating notifications, this matters, because you might be serving a Spanish-speaking audience and want them to get notifications in that language.

To set the language in a function, add the following to those function methods in one of the supported languages:
```
Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("es");
```

## Using Redis for caching
Redis is a great tool for caching data for a stand-alone instance of the app or between many nodes. The default caching provided by the `PopForms.Sql` implementation uses in-memory cache in the app instance itself, which doesn't work when you have many nodes (that is, several web heads running behind a load balancer, like a scaled-out Azure App Service). Redis helps by caching data in a "neutral" location between these nodes.

To use Redis (which is available all over the place, and _not_ just in Azure), use the following configuration lines in your ASP.NET `Program.cs`:

```
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
...
services.AddPopForumsRedisCache();
services.AddSignalR().AddRedisBackplaneForPopForums();
...
```

The first line configures the app to use the Redis caching mechanism. Under the hood, this replaces the `PopForums.Sql` implementation of `ICacheHelper` with the one found in `PopForums.AzureKit`. The second line adds `AddRedisBackplaneForPopForums()` to the configuration of SignalR, so that websocket messages back to the browser are funneled to clients regardless of which web node they're connected to. For example, a user can make a post connected to one node, and the message to update the recent topics page will be signaled to update for users on every node.

You'll also need to setup some values in the `appsettings.json` configuration file (or equivalents in your Azure App Service configuration):

```
{
  "PopForums": {
    "Cache": {
      "Seconds": 180,
      "ConnectionString": "127.0.0.1:6379,abortConnect=false",
      "ForceLocalOnly": false
    },
...
```

* `Seconds`: The number of seconds to persist data in the cache.
* `ConnectionString`: The connection string to the Redis instance.
* `ForceLocalOnly`: When set to true, the app will not use Redis, just local memory. This might be useful if you scale down to one node and no longer need Redis, but don't want to redeploy, for example. Obviously don't set to true if you're running more than one node.

This version of `ICacheHelper` is actually a two-level cache. The data is stored locally in the instance memory of the web app, as well as Redis, because it's still faster if it can avoid calling over the wire. Invalidation of data is handled over Redis' built-in communication channels. So if you update something with a particular cache key, Redis will notify all nodes to invalidate any local value they have. Because it's a two-level cache, you might find that your Redis stats seem not to be active, and when it is being called, it's usually a miss. That's because it doesn't have to go to the Redis instance itself, since the value is already in local memory.

The Azure functions do _not_ use caching. The data that is fetched by the functions is typically transient, not cached or not likely to be.

If you want to prefix cache keys with a specific string, you can do that by using the dependency injection to replace the default `ITenantService` and implementing its `GetTenant()` method. The default implementation simply returns an empty string. See [Multi-tenant options](multitenant.md) for more information.

To run Redis locally, consider using Docker. It only takes a few minutes to setup. Use the Google to figure that out.

## Instrumenting Redis and cache usage

Most managed Redis services have ways to generally observe the behavior and health of the service, but you might be interested in going deeper. For example, the default TTL for caching on all of POP Forums is 90 seconds, but that might not be the "right" amount of time. Also, because this implementation is a two-level cache, monitoring Redis alone doesn't give you the complete picture. POP Forums has an interface called `ICacheTelemetry` in this library, with a default interface that is just an event sink. If you use an external monitoring service like Azure Insights, you may want to replace this with your own implementation. It's super easy! The interface only has two members:

```
void Start();
void End(string eventName, string key);
```

The Redis implementation of `CacheHelper` wraps each call to the memory cache and Redis with the above methods. It includes the cache key and the type of event (`SetRedis`, `GetRedisHit`, `GetRedisMiss`, etc.) for you to persist in whatever your monitoring solution is. In the [hosted forums](https://popforums.com/), we use the following to write the events to Azure Insights. The `TelemetryClient` comes in via dependency injection:

```
public class WebCacheTelemetry : ICacheTelemetry
{
	private readonly TelemetryClient _telemetryClient;
	private Stopwatch _stopwatch;

	public WebCacheTelemetry(TelemetryClient telemetryClient)
	{
		_telemetryClient = telemetryClient;
	}

	public void Start()
	{
		_stopwatch = new Stopwatch();
		_stopwatch.Start();
	}

	public void End(string eventName, string key)
	{
		_stopwatch.Stop();
		var dependencyTelemetry = new DependencyTelemetry();
		dependencyTelemetry.Name = eventName;
		dependencyTelemetry.Properties.Add("Key", key);
		dependencyTelemetry.Duration = new TimeSpan(_stopwatch.ElapsedTicks);
		dependencyTelemetry.Type = "CacheOp";
		_telemetryClient.TrackDependency(dependencyTelemetry);
	}
}
```

Then, to wire up this new implementation, we swap out the event sink for our code in `Program.cs`:

`services.Replace(ServiceDescriptor.Transient<ICacheTelemetry, WebCacheTelemetry>());`

## Using Azure Storage queues and Functions
Azure Storage queues can be used instead of using SQL tables. Using SQL for this is not inherently bad, and honestly the volume of queued things in POP Forums probably never gets huge even on a busy forum, but with queues you get some of the magic of triggering Azure Functions, for example. These are most logically used when you have functions.

To enable queue usage, use this in your `Program.cs` config:

```
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
...
services.AddPopForumsAzureFunctionsAndQueues();
...
```

It's important to _not_ have `services.AddPopForumsBackgroundServices();` in your `Program.cs`, because this would run the background services in the context of the web app. You don't want that, because you're going to run them in Azure Functions.

You'll also need to add a connection string to your Azure Storage account and web app service base. These values must appear in the configuration of your web app _and_ Azure Functions.

```
{
  "PopForums": {
    "WebAppUrlAndArea": "https://somehost/Forums",
    "Queue": {
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=youraccountname;AccountKey=xxxYourAccountKeyxxx=="
...
```
Look at the Azure documentation to see how to provision and deploy Azure Functions, and apply that new knowledge to deploy the `PopForums.AzureKit.Functions` project. (Defining Azure Functions is beyond the scope of this documentation.) You should avoid committing any connection secrets to configuration in source control. See the section above about configuration, and make sure that your Functions have the same settings as your web app.

The `WebAppUrlAndArea` is used to point the functions back at your web app to notify them as necessary and have them in turn notify users in real-time. The URL should end without a slash, and probably ends in `/Forums` unless you changed the name of the area throughout the code. Behind the scenes, the award calculator uses this to call an endpoint on the web app and let it know that a user has received an award. For security, it uses a hash of the queue connection string, _which must be the same for the web app and the functions_.

The connection string for using the local Azure storage emulator is `UseDevelopmentStorage=true`.

## Using Azure Search

_Note: v18+ breaks compatibility with previous indexes using Azure Search._

Use this in your `Program.cs` configuration if you're using web in-process search indexing:

```
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
...
services.AddPopForumsAzureSearch();
...
```
Under the hood, this replaces the `PopForums.Sql` implementation of the search interfaces with those used for Azure Search.

For use in the Azure functions, you'll need to set the `PopForums:Search:Provider` (or `PopForums__Search__Provider` if it's Linux-based) setting in the portal blade for the functions to `azuresearch`.

You'll also need to setup the right configuration values:

```
{
  "PopForums": {
    "Search": {
      "Url": "https://somesearchservice.search.windows.net",
      "Key": "99011A70D3D50D251B0A6141A97B40E7",
      "Provider": ""
    },
```
* `Url`: The URL for Azure Search, typically `https://{nameOfSearchService}.search.windows.net` with the name set in the Azure portal
* `Key`: A key provisioned by the portal to connect to Azure Search
* `Provider`: This is only used in `PopForums.AzureKit.Functions`, where it's used to switch between `elasticsearch`, `azuresearch` and the default bits in the `PopForums.Sql` library. _Important: If the value is left blank, the Azure Functions will use the SQL-based search provider._

## Using Azure storage for hosting uploaded images in posts
The default implementation for uploading images into forum posts is to upload them into the database. While this is convenient and super portable, it may not be the least expensive option, since database storage is typically more expensive than other means. To that end, you can use `AzureKit` to upload and host the images in an Azure storage container.

There are a few configuration values you'll need:
```
 "PopForums": {
    "BaseImageBlobUrl": "http://127.0.0.1:10000/devstoreaccount1",
    "Storage": {
      "ConnectionString": "UseDevelopmentStorage=true"
    },
```
* `BaseImageBlobUrl`: The base URL for the storage where images are uploaded. For local development, using the Azurite storage emulator, this is `http://127.0.0.1:10000/devstoreaccount1`. For a typical Azure storage account, it's probably something like `https://mystorageaccount.blob.core.windows.net`. It should *not* end with a slash, and it shouldn't end with the container name, since that's added in the repository code. In the event you have to move the images for some reason, it's ideal if you could alias a domain name that you own to the storage account.
* `ConnectionString`: It's assumed that you're going to use the same storage account as your queues, but regardless, you need to specify the connection string here. You need this in the web app *and* the functions app.

The code will create a container called `postimage` in your storage account, but if you plan to use the public endpoints of the storage account (instead of a CDN), make sure that `Allow blob public access` is enabled for the account, and then when the container is created, it will be created with the `Blob` access level, meaning anyone can access the images, but they can't see the directory or otherwise manipulate the storage account.

Your web app will need to register the right implementation for the `IPostImageRepository`, and this is achieved in your startup/program with this line in the service configuration:
```
using PopForums.AzureKit;
...
services.AddPopForumsAzureBlobStorageForPostImages();
```
It's important to note that `PopForums.AzureKit.Functions` is already wired to use the blob storage `IPostImageRepository` version, because it's assumed that if you're already using an Azure queue, you also have a storage account.

Another thing to keep in mind is that if you're working locally, and your Azurite instance doesn't have `https` configured, it will break because most browsers do not allow non-`https` images to appear in a secure page. The simplest work around for this is not to install a local certificate, but to change your launch settings for the web app to not run on `https`.

## Using Azure Table Storage for error logging

You may prefer to do your error logging to Azure Table Storage instead of the SQL database. You can do this by adding one line to your `Program` file, which swaps out the SQL error repository for the table storage:
```
services.AddPopForumsTableStorageLogging();
```
The one limitation here is that you can't use the admin UI to look at errors, since paging and ordering is fairly crude in table storage. For the connection string, it will use whatever is found in `PopForums:Storage:ConnectionString`, the same setting used by the image uploads.