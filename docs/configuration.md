---
layout: default
title: Configuration
nav_order: 2.2
---
# Configuration

POP Forums has three layers of configuration:
* Service component registration in `Program.cs`: These are a series of extention methods that define which implementations to use in the web app and optionally in the functions host. This is how you choose to enable Azure queues and functions, for example.
* `json` config files/environment variables: These are the static values loaded at runtime to control values that rarely, if ever change, like connection strings.
* Admin settings: These are more operational and often data driven, for example, setting up forums.

## Service component registration

POP Forums is wired up through a set of extension methods. The reference implementations are `src/PopForums.Web/Program.cs` and `src/PopForums.FunctionsHost/Program.cs`.

### General ordering rule

Register the required defaults first, then the optional features. The optional features override the defaults, so a default registered after one of them would undo it.

### Required

| Method | Library | Used in | What it enables |
|---|---|---|---|
| `AddPopForumsSql()` | `PopForums.Sql` | Web app, functions host | SQL Server data storage, plus the in-memory cache that a single node uses. |
| `AddPopForumsMvc()` | `PopForums.Mvc` | Web app | The forum web app itself: its services, plus sign-in through the POP Forums authentication cookie. Includes `AddPopForumsBase()`. |
| `AddPopForumsBase()` | `PopForums` | Functions host | The core forum services. The web app gets these through `AddPopForumsMvc()`, so don't call this in the web app as well. |

* Call `AddPopForumsSql()` before `AddPopForumsMvc()`.
* In the functions host, call `AddPopForumsBase()` and `AddPopForumsSql()` before anything else.

### Web app request pipeline

| Method | Library | What it enables |
|---|---|---|
| `AddPopForumsPolicies()` | `PopForums.Mvc` | The admin and moderator authorization policies. Call it inside `services.Configure<AuthorizationOptions>(...)`. |
| `AddPopForumsLogger(app)` | `PopForums.Mvc` | Error logging to the POP Forums error log. Call it after `builder.Build()`. |
| `UsePopForumsCultures()` | `PopForums.Mvc` | Localization into the supported languages. |
| `UsePopForumsAuth()` | `PopForums.Mvc` | Identifying the forum user on each request. |
| `AddPopForumsEndpoints()` | `PopForums.Mvc` | The forum routes, the admin and setup pages, and the real-time (SignalR) hub. |

These must run in this order:
1. `UsePopForumsCultures()`
2. `UseAuthentication()`
3. `UseRouting()`
4. `UsePopForumsAuth()`
5. `UseAuthorization()`
6. `AddPopForumsEndpoints()`, before you map your app's own routes

On a new install, the forum routes and the error logger only activate once the database is set up. Restart the app after running `/Forums/Setup`.

### Optional features

| Method | Library | Used in | What it enables |
|---|---|---|---|
| `AddPopForumsBackgroundJobs()` **or** `AddPopForumsAzureFunctionsAndQueues()` | `PopForums.Mvc` / `PopForums.AzureKit` | Web app | Where background work (email, search indexing, awards, cleanup, etc.) runs. Use exactly one in the web app. `AddPopForumsBackgroundJobs()` runs the work inside the web app, which suits a single node or local development. `AddPopForumsAzureFunctionsAndQueues()` queues the work to Azure Storage so that Azure Functions process it. |
| `AddPopForumsAzureFunctionsAndQueues()` | `PopForums.AzureKit` | Functions host | Registers the various asynchronous services in a functions host. The same method run from the web app works in concert to defer these jobs to the functions. |
| `AddPopForumsRedisCache()` | `PopForums.AzureKit` | Web app | A Redis-backed cache that stays consistent across multiple web nodes. |
| `AddRedisBackplaneForPopForums()` | `PopForums.AzureKit` | Web app | Real-time updates that reach users on every web node. Chain it onto SignalR: `services.AddSignalR().AddRedisBackplaneForPopForums()`. |
| `AddPopForumsAzureSearch()` | `PopForums.AzureKit` | Web app, functions host | Search powered by Azure AI Search. |
| `AddPopForumsElasticSearch()` | `PopForums.ElasticKit` | Web app, functions host | Search powered by ElasticSearch. |
| `AddPopForumsAzureBlobStorageForPostImages()` | `PopForums.AzureKit` | Web app, functions host | Storing images uploaded in posts in Azure Blob Storage. |
| `AddPopForumsTableStorageLogging()` | `PopForums.AzureKit` | Web app, functions host | Error logging to Azure Table Storage instead of the database. |
| `AddPopForumsFunctionsHost()` | `PopForums.AzureKit.Functions` | Functions host | The services that a functions host needs in order to run the background work and notify the web app. Also turns off caching, because functions work with transient data. |

* Call all of these after the required methods.
* Call exactly one of `AddPopForumsBackgroundJobs()` or `AddPopForumsAzureFunctionsAndQueues()` in the web app. If you call neither, background work doesn't run. If you call both, the last one registered takes precedence, so don't do that.
* Don't call both `AddPopForumsAzureSearch()` and `AddPopForumsElasticSearch()`. If you call neither, search uses SQL Server.
* In the functions host, call `AddPopForumsFunctionsHost()` after `AddPopForumsSql()`, and don't call `AddPopForumsRedisCache()` there. Either mistake would turn caching back on for the functions.
* See [Using AzureKit Library](azurekitlibrary.md) and [Using ElasticKit Library](elastickitlibrary.md) for the configuration values these features need.

### Keep the web app and functions host in sync

When background work runs in Azure Functions, both hosts must make the same choices:
* **Queues:** call `AddPopForumsAzureFunctionsAndQueues()` in both.
* **Search:** use the same search provider in both.
* **Post images:** if one host uses blob storage, both must.
* **Error logging:** use table storage logging in both or in neither.

### Example

Web app:
```csharp
services.Configure<AuthorizationOptions>(options => options.AddPopForumsPolicies());

services.AddPopForumsSql();
services.AddPopForumsMvc();

services.AddPopForumsRedisCache();
services.AddSignalR().AddRedisBackplaneForPopForums();
services.AddPopForumsElasticSearch();
services.AddPopForumsAzureFunctionsAndQueues();
services.AddPopForumsAzureBlobStorageForPostImages();

var app = builder.Build();
app.Services.GetService<ILoggerFactory>().AddPopForumsLogger(app);

app.UsePopForumsCultures();
app.UseAuthentication();
app.UseRouting();
app.UsePopForumsAuth();
app.UseAuthorization();
app.AddPopForumsEndpoints();
```

Functions host:
```csharp
s.AddPopForumsBase();
s.AddPopForumsSql();

s.AddPopForumsAzureFunctionsAndQueues();
s.AddPopForumsFunctionsHost();
s.AddPopForumsElasticSearch();
s.AddPopForumsAzureBlobStorageForPostImages();
```


## `json` config files/environment variables

