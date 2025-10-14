---
layout: default
title: Start Here
nav_order: 2
---
# Start Here

POP Forums attempts to not get in the way of your application, by working as an MVC area. All of the front-end dependencies are embedded in the Nuget packages, so there's no need to npm packages and build and copy stuff.

How to use [The Scoring Game](scoringgame.md) in your own application.

## Upgrading?

v21 has breaking changes for using `IForumAdapter`. See [Cusstomization](customization.md) for more information.

This version has data changes. From v20.x, run the `PopForums20to21.sql` script included in the `PopForums.Sql` project. If you need to upgrade from v16.x to v20.x, _first_ run the `PopForums16to20.sql` script against your database, which is found in the `PopForums.Sql` project. It's safe to run this script more than once. IMPORTANT: Going from v18 forward, because of the changes to private messages, you must first also delete all of the existing history by running `DELETE FROM pf_PrivateMessage` against your database. The reason that this isn't included in the upgrade script is because you should know it's necessary and do it on your own.

Updating your app from the legacy ASP.NET MVC world to ASP.NET Core is non-trivial, and well beyond the scope of this documentation.

## Prerequisites
You'll need the following locally:
* Visual Studio 2022 or later, with the Azure workload (community version is fine), Visual Studio for Mac or Jetbrains Rider
* Node.js (comes with npm)
* SQL Server Developer, or SQL Server running in a Docker container
* A mail sending service that supports SMTP
* Optionally, Docker if you intend to run Azurite, Redis, ElasticSearch, etc. (instructions below)

## Build vs. reference

You should definitely get to know the installation information below to understand the project structure, but understand that you can also use POP Forums by way of Nuget package references. The [POPWorldMedia/POPForums.Sample](https://github.com/POPWorldMedia/POPForums.Sample) project shows how you can do this without having to build this project, and it's literally adding a Nuget package reference and adding some config stuff in `Program.cs`.

## Reference

* Again, [POPWorldMedia/POPForums.Sample](https://github.com/POPWorldMedia/POPForums.Sample) is a good starting point when using POP Forums via reference, but the `PopForums.Web` project in the source code works similarly, with project references instead of NuGet references.
* Reference `PopForums.Mvc` and `PopForums.Sql` from Nuget. If you want to use the scale-out kits (`PopForums.AzureKit` and `PopForums.ElasticKit`), add those as well.
* Starting with v19, `PopForums.Mvc` includes all of the front-end goodies, the Javascript and CSS, right in the package.
* You'll need a layout view for the forum to live in.
* Set up the various options in `Program.cs` as described in its comments and this documentation.
* `appsettings.json` will have your forum configuration.
* There is no package for the Azure Functions, because it's currently hard to make them work from a shared library in certain situations. However, you can deploy the project from the main repo with ease given the tooling in VS or Azure DevOps Pipelines. Just be sure to set the right values up in the application configuration in the Azure portal.
* POP Forums uses ASP.NET Data Protection in multi-node or external login scenarios. Actually, the basic anti-forgery code baked into the framework does as well, so when you deploy, or swap deployment slots in Azure, you need to persist the underlying key somewhere. This is also true if you run multiple nodes (scale out). You can persist the underlying keys in a number of different ways (I prefer Azure Blob Storage). In your `Program.cs`, use `services.AddDataProtection()` and the appropriate extension method. If you don't do this for multi-node, things like social logins and anti-forgery will fail and fill your error logs with stuff about broken things. If you use slots in Azure App Services, you'll also want the Data Protection setup, otherwise the swap will cause everyone to be logged out.

For the bleeding edge, latest build from `main`, the CI build packages can be obtained by a MyGet feed:
* https://www.myget.org/F/popforums/api/v3/index.json (Nuget package includes the server application and front-end assets)

## Build

* Clone the latest source code from GitHub, or use the production packages as described above. Build it. If your IDE doesn't automatically build Javascript or Typescript, be sure to `npm install` in the `PopForums.Mvc` project, and then run the `gulpfile.js`.
* The project files require an up-to-date version of Visual Studio 2022 or later. It also works great with Jetbrains' Rider on Mac or Windows.
* This project is built on ASP.NET v9. Make sure you have the required SDK installed (v9.0.101).
* The `PopForums.Web` project is the template to use to include the forum in your app. It references `PopForums.Mvc`, which contains all of the web app-specific code, including script and CSS. `PopForums.Sql` concerns itself only with data, while `PopForums` works entirely with business logic and defines interfaces used in the upstream projects. `PopForums.AzureKit` contains a number of items to facilitate using various Azure services. `PopForums.ElasticKit` contains an ElasticSearch implementation. `PopForums.AzureKit.Functions` is an implementation of functions, used if you're not using in-app context background services (see below).
* The `main` branch is using Azure Functions by default to run background processes. Run the [Azurite](https://github.com/azure/azurite) container in Docker (works on Windows and Mac). If not, you can run the background things in-process by uncommenting `services.AddPopForumsBackgroundServices()` in `Program.cs` and commenting out or removing `services.AddPopForumsAzureFunctionsAndQueues()`. This causes all of the background things to run in the context of the web app itself.

> Running the background services in the web context can cause some wild variations in CPU and RAM usage on a busy forum, especially in the code associated with updating the search index. If you are running in Azure, using Functions is a much better choice for consistent and predictable app performance.

## Installation

* Once you've completed one of the above scenarios, reference or build, it's time to fire it up, starting with the configuration file.
* `appsettings.json`, in the root of the web project, is the basic configuration file for POP Forums. It works like any other config file in ASP.NET Core, so when you're running in Azure, you can use the colon notation in the App Service application settings to set these values (i.e., `PopForums:Cache:Seconds` as the key).

> If you run the app in a Linux App Service or container, your settings notation should replace `:` with a double underscore, `__`. So the above would be `PopForums__Cache__Seconds`.

```js
{
    "PopForums": {
        "IpLookupUrlFormat": "https://whatismyipaddress.com/ip/{0}", // used on Recent Users screen of admin to lookup IP addresses
        "BaseImageBlobUrl": "http://127.0.0.1:10000/devstoreaccount1", // if using AzureKit to host images, points to the base URL of images uploaded to blob storage (you should really alias the storage to a domain you own)
        "Storage": {
            "ConnectionString": "UseDevelopmentStorage=true" // if using AzureKit to host images, typically the same as the Queue:ConnectionString, but the place where images are uploaded to blob storage
        },
        "Database": {
            "ConnectionString": "server=localhost;Database=popforums21;Trusted_Connection=True;TrustServerCertificate=True;"
        },
        "Cache": {
            "Seconds": 180,
            "ConnectionString": "127.0.0.1:6379,abortConnect=false", // used for Redis cache in AzureKit
            "ForceLocalOnly": false // used for Redis cache in AzureKit
        },
        "Search": { // used for Elastic or Azure Search (see docs)
            "Url": "popforumsdev",
            "Key": "99011A70D3D50D251B0A6141A97B40E7",
            "Provider": ""
        },
        "Queue": { // used for queues with Azure Functions
            "ConnectionString": "UseDevelopmentStorage=true"
        },
        "LogTopicViews": true, // optional, records topic views for future analytics
        "ReCaptcha": { // Google ReCaptcha on signup (the key/secret below works on localhost)
            "UseReCaptcha": true,
            "SiteKey": "6Lc2drIUAAAAAPaa1iHozzu0Zt9rjCYHhjk4Jvtr",
            "SecretKey": "6Lc2drIUAAAAADXBXpTjMp67L-T5HdLe7OoKlLrG"
        },
        "WebAppUrlAndArea": "https://somehost/forums", // used only by Azure Functions to find endpoint of your web app
        "RenderBootstrap": true, // optional, defaults to true, put false here if your host page will have its own build of Bootstrap CSS
        "OAuthOnly": {
            // this section is detailed in the OAuth-Only Mode section
        }
    }
}
```

* Attempt to run the app locally via Kestrel, and go to the URL `/Forums` to see an error page about not finding the settings table. It will fail either because the database isn’t set up, or because it can’t connect to it. The biggest reason for failure is an incorrect connection string. If you change nothing locally, by default it's looking for a local database on the default SQL Server instance called `popforums21`.
* If you want to use the setup page (and you should), don’t run the SQL script. Once the POP Forums tables exist in the database, the setup page will tell you that you’re prohibited from going there.
* Point the browser to `/Forums/Setup` now, and if your connection string is correct, you should see a page with some of the basic fields to set up.
> If you're running in OAuth-Only Mode, there is no setup for the fields below. The forum will attempt to set up the database, and that's it. That mode has no email functionality, and user creation and roles are delegated to the external identity provider. See [OAuth-Only Mode](oauthonly.md) for more information.
* The `PopForums.Mvc` package includes Bootstrap, which is used as the base style for the entire app. To give it your own look, you can add your own CSS to override Bootstrap in your `_Layout.cshtml`, or do your own build of Bootstrap with whatever variables you like. If you prefer your own build, make sure _both_ the Javascript and CSS tags appear _before_ the `RenderSection` in your header, and set the `RenderBootstrap` setting in `appconfig.json` to `false`. Learn more in [customization](customization.md).
* If you're using Azure functions in the background, instead of embedding the background work in the web app (see [Using AzureKit](azurekitlibrary.md)), you'll want to run multiple startup projects, specifically the `PopForums.Web` and `PopForums.AzureKit.Functions`.

Here’s what each field on the setup page does: 
* **Forum title:** This is what your forum will be called at the root, in an h1 tag. You can edit this (and everything else) later.
* **SMTP Server:** The host name of the server you’ll connect to for sending e-mail. Enabling this functionality on your server is beyond the scope of this document, but we usually use SendGrid to send email.
* **Port:** Typically 25, though some services (like Gmail) use others.
* **From e-mail address:** When a user receives e-mail from the forum, it will be “from” this address.
* **Use SSL:** Check if your server uses or requires SSL.
* **Use ESMTP for credentials:** Check this box if you have to authenticate with your server (this is almost always the case). Checking this makes the two boxes below it editable.
* **SMTP User:** User name (often the e-mail address) to authenticate with. Not editable unless the “Use ESMTP” box is checked.
* **SMTP Password:** Password to authenticate with. Not editable unless the “Use ESMTP” box is checked.
* **Display name:** How you want your name to appear in the forum.
* **E-mail:** The e-mail address you’ll use to login with.
* **Password:** The password you’ll use to login with.

You're almost there!

* If you typed everything you need correctly, you should see a happy result, otherwise you’ll see a stack trace and exception.
* Restart the app.
* From here, you can follow the link to the admin home page and add categories and forums. You’ll be logged in as the user you created, and that account will be part of the Admin and Moderator roles.
* Once you’ve added some forums on the “Forums” admin page, you can go to `/Forums` to start posting.
* If you want to test your e-mail setup, go to `/Forums/Account/Forgot` and enter your e-mail address. Failures are also logged in the error log, which is found in the admin area.
* For future reference, you can revisit the admin area at `/Forums/Admin`, and when you're logged in as an admin, a link appears in the user dropdown from the navigation menu.

## Integration

The `PopForums.Web` project is the template you can use as the basis for your own POP Forums apps. If you want to build via the most recent stable builds, the [POPWorldMedia/POPForums.Sample](https://github.com/POPWorldMedia/POPForums.Sample) project is an example of how to do that (see above). The app uses the standard claims-based authentication, but it does not use Identity or Entity Framework. When you're logged in, you'll find the identity of the user on the User property of the controller as expected. The `PopForumsAuthorizationMiddleware` loads user and profile data into the request pipeline, so it can be loaded once and used throughout the request lifecycle.

Accessing the user data can be achieved via an instance of `IUserRetrievalShim`, which you can inject into your dependency chain. From a controller, simply call `_userRetrievalShim.GetUser()` to get the fully hydrated POP Forums `User`, or `_userRetrievalShim.GetProfile()` to get the profile. Under the hood, these are stored in the `Items` collection of the current `HttpContext`.

The easiest way to integrate with an existing set of users is to connect via an OAuth2 provider. Read more about [OAuth-Only Mode](oauthonly.md).

## Running third-party services in Docker containers

If you want to run locally with some of the "kits" described in the documentation, you'll need to fire them up using Docker. Here are the commands for the most common things. These sometimes change, because of new names, versions and such, but they're current as of early 2023.  
  
* SQL Server (keep in mind that `mcr.microsoft.com/azure-sql-edge` is the ARM versoin of SQL)  
`docker run --cap-add SYS_PTRACE -e 'ACCEPT_EULA=1' -e 'MSSQL_SA_PASSWORD=P@ssw0rd' -p 1433:1433 --name sqledge -d mcr.microsoft.com/azure-sql-edge`  
* Azureite, for storage and queues  
`docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite`  
* Redis, for distributed cache and SignalR backplane  
`docker run --name some-redis -p 6379:6379 -d redis`  
* ElasticSearch, for better search  
`docker run --name es-01 -p 9200:9200 -e discovery.type=single-node -it docker.elastic.co/elasticsearch/elasticsearch:8.11.1`  

You may want to have your databases be more durable in the event you trash the SQL container or update to a new one. To do that, first create a new volume, either in Docker Desktop or on the command line:
```
docker volume create sqldata
```
Then fire up the container and associate it with the volume, and tell it to use that volume for all of the data.
```
docker run -e 'ACCEPT_EULA=1' -e 'MSSQL_SA_PASSWORD=P@ssw0rd' -p 1433:1433 --name sqledge -v sqldata:/DATA -d mcr.microsoft.com/azure-sql-edge
```
If you would like to host the data files in your own file system, you can start the container like this, replacing the approprirate paths to your local spots, where `<host directory>` is your spot:
```
docker run -e 'ACCEPT_EULA=1' -e 'MSSQL_SA_PASSWORD=P@ssw0rd' -p 1433:1433 --name sql2022 -v <host directory>:/var/opt/mssql/data -d mcr.microsoft.com/mssql/server:2022-latest
```
And if you need to copy files out of an existing container, you can do that too. `~/sqlvolumes` in this case points to a folder in my user folders on a Mac:
```
docker cp containerID3bed54c7734b:/var/opt/mssql ~/sqlvolumes
```

## Running Azure Functions on a Mac

This isn't the most straightfoward thing, and it's hard to find the information, but you need to install the Azure Functions Core tools via Homebrew. [Microsoft explains how to do this.](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Cmacos%2Ccsharp%2Cportal%2Cbash#install-the-azure-functions-core-tools)

## Customization

To make POP Forums look the way you want, or with extra functionality, read up on [customization](customization.md).
