---
layout: default
title: Start Here
nav_order: 2
---
# Start Here

POP Forums attempts to not get in the way of your application, by working as an MVC area. All of the front-end dependencies are embedded in the Nuget packages, so there's no need to npm packages and build and copy stuff.

How to use [The Scoring Game](scoringgame.md) in your own application.

## Upgrading?

v19 includes substantial architectural changes. The biggest change is that the front-end code, including Javascript and CSS, is delivered with the `PopForums.Mvc` package or project. The front-end parts are now written in TypeScript, and utilize plain vanilla web components (the admin still uses Vue.js). Private messages have been converted into real-time chat.

This version has big data changes. If you need to upgrade from v16.x, v.17x or v18.x, run the `PopForums16to19.sql` script against your database, which is found in the `PopForums.Sql` project. It's safe to run this script more than once. IMPORTANT: Because of the changes to private messages, you must first also delete all of the existing history by running `DELETE FROM pf_PrivateMessage` against your database. The reason that this isn't included in the upgrade script is because you should know it's necessary and do it on your own.

Updating your app from the legacy ASP.NET MVC world to ASP.NET Core is non-trivial, and well beyond the scope of this documentation.

## Prerequisites
You'll need the following locally:
* Visual Studio 2022 or later, with the Azure workload (community version is fine), Visual Studio for Mac or Jetbrains Rider
* Node.js (comes with npm)
* SQL Server Developer, or SQL Server running in a Docker container
* Optionally, Docker if you intend to run Azurite, Redis, ElasticSearch, etc.

## Build vs. reference

You should definitely get to know the installation information below to understand the project structure, but understand that you can also use POP Forums by way of Nuget package references. The [POPWorldMedia/POPForums.Sample](https://github.com/POPWorldMedia/POPForums.Sample) project shows how you can do this without having to build this project, and it's literally adding a Nuget package reference and adding some config stuff.

## Reference

* Reference `PopForums.Mvc` and `PopForums.Sql` from Nuget. If you want to use the scale-out kits (`PopForums.AzureKit` and `PopForums.ElasticKit`), add those as well.
* Starting with v19, `PopForums.Mvc` includes all of the front-end goodies, the Javascript and CSS, right in the package.
* You'll need a layout view for the forum to live in.
* Set up the various options in `Program.cs` as described in its comments and this documentation.
* `appsettings.json` will have your forum configuration.
* There is no package for the Azure Functions, because it's currently hard to make them work from a shared library in certain situations. However, you can deploy the project from this repo with ease given the tooling in VS or Azure DevOps Pipelines. Just be sure to set the right values up in the application configuration in the Azure portal.
* POP Forums uses ASP.NET Data Protection in multi-node or external login scenarios. Actually, the basic anti-forgery code baked into the framework does as well, so when you deploy, or swap deployment slots in Azure, you need to persist the underlying key somewhere. This is also true if you run multiple nodes (scale out). You can persist the underlying keys in a number of different ways (I prefer Azure Blob Storage). In your `Program.cs`, use `services.AddDataProtection()` and the appropriate extension method. If you don't do this for multi-node, things like social logins and anti-forgery will fail and fill your error logs with stuff about broken things.

For the bleeding edge, latest build from `main`, the CI build packages can be obtained by a MyGet feed:
* https://www.myget.org/F/popforums/api/v3/index.json (Nuget for the backend)

## Build installation

* Download the latest source code from GitHub, or use the production packages as described above. Build it.
* The project files require an up-to-date version of Visual Studio 2022 or later. It also appears to build in Visual Studio for Mac and Jetbrains' Rider.
* This project is built on ASP.NET v6. Make sure you have the required SDK installed (v6.0.400).
* The `PopForums.Web` project is the template to use to include the forum in your app. It references `PopForums.Mvc`, which contains all of the web app-specific code, including script and CSS. `PopForums.Sql` concerns itself only with data, while `PopForums` works entirely with business logic and defines interfaces used in the upstream projects. `PopForums.AzureKit` contains a number of items to facilitate using various Azure services. `PopForums.ElasticKit` contains an ElasticSearch implementation. `PopForums.AzureKit.Functions` is an implementations of functions, used if you're not using in-app context background services (see below).
* The `main` branch is using Azure Functions by default to run background processes. A recent build of Visual Studio 2022 probably has all of the SDK's but storage emulation is required, so run the [Azurite](https://github.com/azure/azurite) container in Docker (works on Windows and Mac). If not, you can run the background things in-process by uncommenting `services.AddPopForumsBackgroundServices()` in `Program.cs` and commenting out or removing `services.AddPopForumsAzureFunctionsAndQueues()`. This causes all of the background things to run in the context of the web app itself.
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
			"ConnectionString": "server=localhost;Database=popforums14;Trusted_Connection=True;TrustServerCertificate=True;"
		},
		"Cache": {
			"Seconds": 180,
			"ConnectionString": "127.0.0.1:6379,abortConnect=false", // used for Redis cache in AzureKit
			"ForceLocalOnly": false // used for Redis cache in AzureKit
		},
		"Search": { // used for Azure Search in AzureKit
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
		"RenderBootstrap": true // optional, defaults to true, put false here if your host page will have its own build of Bootstrap CSS
	}
}
```
* Attempt to run the app locally via Kestrel, and go to the URL /Forums to see an error page. It will fail either because the database isn’t set up, or because it can’t connect to it. The biggest reason for failure is an incorrect connection string. If you change nothing, by default it's looking for a local database on the default SQL Server instance called `popforums19`.
* If you want to use the setup page (and you should), don’t run the SQL script. Once the POP Forums tables exist in the database, the setup page will tell you that you’re prohibited from going there.
* Point the browser to /Forums/Setup now, and if your connection string is correct, you should see a page with some of the basic fields to set up.
* The `PopForums.Mvc` package includes Bootstrap, which is used as the base style for the entire app. To give it your own look, you can add your own CSS to override Bootstrap in your `_Layout.cshtml`, or do your own build of Bootstrap with whatever variables you like. If you prefer your own build, make sure _both_ the Javascript and CSS tags appear _before_ the `RenderSection` in your header, and set the `RenderBootstrap` setting in `appconfig.json` to `false`.
* If you're using Azure functions in the background, instead of embedding the background work in the web app (see "Using AzureKit"), you'll want to run multiple startup projects, specifically the `PopForums.Web` and `PopForums.AzureKit.Functions`.

Here’s what each field on the setup page does: 
* **Forum title:** This is what your forum will be called at the root, in an h1 tag. You can edit this (and everything else) later.
* **SMTP Server:** The host name of the server you’ll connect to for sending e-mail. Enabling this functionality on your server is beyond the scope of this document, but we usually use SendGrid to send email.
* **Port:** Typically 25, though some services (like Gmail) use others.
* **From e-mail address:** When a user receives e-mail from the forum, it will be “from” this address.
* **Use SSL:** Check if your server uses or requires SSL.
* **Use ESMTP for credentials:** Check this box if you have to authenticate with your server. Checking this makes the two boxes below it editable.
* **SMTP User:** User name (often the e-mail address) to authenticate with. Not editable unless the “Use ESMTP” box is checked.
* **SMTP Password:** Password to authenticate with. Not editable unless the “Use ESMTP” box is checked.
* **Time zone and Use daylight saving:** Determines how your forum should handle time. This will also be the time adjustment your admin account will use. Both can be changed later.
* **Display name:** How you want your name to appear in the forum.
* **E-mail:** The e-mail address you’ll use to login with.
* **Password:** The password you’ll use to login with.
* If you typed everything you need correctly, you should see a happy result, otherwise you’ll see a stack trace and exception.
* Restart the app.
* From here, you can follow the link to the admin home page and add categories and forums. You’ll be logged in as the user you created, and that account will be part of the Admin and Moderator roles.
* Once you’ve added some forums on the “Forums” admin page, you can go to `/Forums` to start posting.
* If you want to test your e-mail setup, go to `/Forums/Account/Forgot` and enter your e-mail address. Failures are also logged in the error log, which is found in the admin area.
* For future reference, you can revisit the admin area at `/Forums/Admin`, and when you're logged in as an admin, a link appears in the user dropdown from the navigation menu.

## Integration

The `PopForums.Web` project is the template you can use as the basis for your own POP Forums apps. If you want to build via the most recent stable builds, the [POPWorldMedia/POPForums.Sample](https://github.com/POPWorldMedia/POPForums.Sample) project is an example of how to do that (see above). The app uses the standard claims-based authentication, but it does not use Identity or Entity Framework. When you're logged in, you'll find the identity of the user on the User property of the controller as expected. The `PopForumsAuthorizationMiddleware` loads user and profile data into the request pipeline, so it can be loaded once and used throughout the request lifecycle.

Accessing the user data can be achieved via an instance of `IUserRetrievalShim`, which you can inject into your dependency chain. From a controller, simply call `_userRetrievalShim.GetUser()` to get the fully hydrated POP Forums `User`, or `_userRetrievalShim.GetProfile()` to get the profile. Under the hood, these are stored in the `Items` collection of the current `HttpContext`.

## Customization

To make POP Forums look the way you want, or with extra functionality, read up on [customization](customization.md).