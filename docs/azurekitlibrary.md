---
layout: default
title: Using Azure Kit Library
nav_order: 6
---
# Using AzureKit Library
The `PopForums.AzureKit` library makes it possible to wire up the following scenarios:
* Using Redis for caching (not dependent on Azure specifically... Redis runs everywhere!)
* Using Azure Storage queues and Functions to queue work for search indexing, emailing and scoring game award calculation
* Using Azure Search

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

If you want to prefix cache keys with a specific string, you can do that by using the dependency injection to replace the default `ITenantService` and implementing its `GetTenant()` method. The default implementation simply returns an empty string.

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

Then, to wire up this new implemenation, we swap out the event sink for our code in `Program.cs`:

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

You'll also need to add a connection string to your Azure Storage account:

```
{
  "PopForums": {
    "Queue": {
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=youraccountname;AccountKey=xxxYourAccountKeyxxx=="
...
```
Look at the Azure documentation to see how to provision and deploy Azure Functions, and apply that new knowledge to deploy the `PopForums.AzureKit.Functions` project. (Defining Azure Functions is beyond the scope of this documentation.) You should avoid committing any connection secrets to configuration in source control. See the section above about configuration, and make sure that your Functions have the same settings as your web app.

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
* `Provider`: This is only used in `PopForums.AzureKit.Functions`, where it's used to switch between `elasticsearch`, `azuresearch` and the default bits in the `PopForums.Sql` library