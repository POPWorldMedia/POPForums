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
* Running background work in the web app can cause big swings in CPU and RAM use on a busy forum, especially when it updates the search index. In Azure, Functions give you more consistent, predictable performance.
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

The web app reads its settings from `appsettings.json`, and the reference functions host reads them from `local.settings.json`. Both hosts also read environment variables, which override the files. In Azure, that means the App Service or Functions application settings. Both hosts use the same keys, all under the `PopForums` section.

As environment variables, the keys use colons to show the hierarchy, for example `PopForums:Cache:Seconds`. On Linux App Services, Functions and containers, use a double underscore instead: `PopForums__Cache__Seconds`.

### Example

```json
{
  "PopForums": {
    "Database": {
      "ConnectionString": "server=localhost;Database=popforums21;Trusted_Connection=True;TrustServerCertificate=True;"
    },
    "Cache": {
      "Seconds": 180,
      "ConnectionString": "127.0.0.1:6379,abortConnect=false",
      "ForceLocalOnly": false
    },
    "Search": {
      "Provider": "elasticsearch",
      "Url": "https://localhost:9200",
      "Key": "99011A70D3D50D251B0A6141A97B40E7"
    },
    "Queue": {
      "ConnectionString": "UseDevelopmentStorage=true"
    },
    "Storage": {
      "ConnectionString": "UseDevelopmentStorage=true"
    },
    "BaseImageBlobUrl": "http://127.0.0.1:10000/devstoreaccount1",
    "WebAppUrlAndArea": "https://localhost:5091/Forums",
    "IpLookupUrlFormat": "https://whatismyipaddress.com/ip/{0}",
    "LogTopicViews": true,
    "ReCaptcha": {
      "UseReCaptcha": true,
      "SiteKey": "6Lc2drIUAAAAAPaa1iHozzu0Zt9rjCYHhjk4Jvtr",
      "SecretKey": "6Lc2drIUAAAAADXBXpTjMp67L-T5HdLe7OoKlLrG"
    },
    "RenderBootstrap": true,
    "OAuthOnly": {
      "IsOAuthOnly": false
    }
  }
}
```

The connection strings point at the local Docker containers described in [Start Here](starthere.md). The reCAPTCHA keys only work on `localhost`.

### Settings

All keys are under `PopForums:`.

| Key | Required when | Default | What it's for |
|---|---|---|---|
| `Database:ConnectionString` | Always | — | The SQL Server database. |
| `Cache:Seconds` | Optional | `90` | How long cached data is kept. |
| `Cache:ConnectionString` | Using `AddPopForumsRedisCache()` or `AddRedisBackplaneForPopForums()` | — | The Redis instance. |
| `Cache:ForceLocalOnly` | Optional, with the Redis cache | `false` | Set it to `true` to cache in local memory only and ignore Redis. This is useful if you scale down to one node and don't want to redeploy. Don't use it with more than one node. |
| `Search:Provider` | Using ElasticSearch, or using Azure AI Search in the reference functions host | — | `elasticsearch` or `azuresearch`. ElasticSearch ignores `Url` and `Key` unless this is `elasticsearch`. The reference functions host uses it to choose its search provider. |
| `Search:Url` | Using Azure AI Search or ElasticSearch | — | The search service's endpoint URL. |
| `Search:Key` | Using Azure AI Search or ElasticSearch | — | The search service's API key. |
| `Queue:ConnectionString` | Using `AddPopForumsAzureFunctionsAndQueues()` | — | The Azure Storage account for the background work queues. It also secures the notifications that the functions send to the web app, so it must be identical in both hosts. |
| `WebAppUrlAndArea` | Functions host | — | The forum's base URL, including the area (for example `https://example.com/Forums`). The functions use it to send notifications to the web app. |
| `Storage:ConnectionString` | Using `AddPopForumsAzureBlobStorageForPostImages()` or `AddPopForumsTableStorageLogging()` | — | The Azure Storage account for post images or error logs. It's often the same account as the queues. |
| `BaseImageBlobUrl` | Using `AddPopForumsAzureBlobStorageForPostImages()` | — | The base URL that post images are served from. Ideally, use a domain you own that's aliased to the storage account. |
| `IpLookupUrlFormat` | Optional | — | A URL with `{0}` in place of the IP address, used for IP lookup links in the admin's Recent Users page. |
| `LogTopicViews` | Optional | `false` | Records topic views for future analytics. |
| `ReCaptcha:UseReCaptcha` | Optional | `false` | Turns on Google reCAPTCHA when people sign up. |
| `ReCaptcha:SiteKey`, `ReCaptcha:SecretKey` | Using reCAPTCHA | — | Your reCAPTCHA keys. |
| `RenderBootstrap` | Optional | `true` | Set it to `false` if your layout includes its own build of Bootstrap. See [customization](customization.md). |
| `OAuthOnly:*` | Using OAuth-only mode | `IsOAuthOnly` is `false` | Delegates sign-in and roles to an external identity provider. See [OAuth-Only Mode](oauthonly.md) for these keys. |

### Outside the `PopForums` section

`PopForums.Web`'s `Program.cs` also reads `DataProtectBlobConnectionString`. If it's set, the app persists ASP.NET Data Protection keys to that blob storage account. You need this when you run more than one node or use Azure deployment slots, or else the auth cookie and anti-forgery tokens break across nodes and swaps. This setting belongs to the template rather than to POP Forums, so you can use any persistence mechanism Data Protection supports.
