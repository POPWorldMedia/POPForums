---
layout: default
title: Start Here
nav_order: 2
---
# Start Here

POP Forums attempts to not get in the way of your application, by working as an MVC area.

How to use [The Scoring Game](scoringgame.md) in your own application.

## Upgrading?

This release has data changes. Run the PopForums15to16.sql script against your database, which is found in the `PopForums.Sql` project.

Updating your app from the legacy ASP.NET MVC world to ASP.NET Core is non-trivial, and well beyond the scope of this documentation.

## Prerequisites
You'll need the following locally:
* Visual Studio 2019 or later, with the Azure workload (community version is fine)
* Node.js (comes with npm)
* SQL Server Developer
* Optionally, Docker if you intend to run Redis, ElasticSearch, etc.

## Build vs. reference

You should definitely get to know the installation information below to understand the project structure, but understand that you can also use POP Forums by way of Nuget and npm pagackage references. The [POPWorldMedia/POPForums.Sample](https://github.com/POPWorldMedia/POPForums.Sample) project shows how you can do this without having to build this project. The short story is this:
* Reference `PopForums.Mvc` and `PopForums.Sql` from Nuget. If you want to use the scale-out kits, add those as well.
* Reference `@popworldmedia/popforums` from npm.
* Use gulp or some other package to copy or pack the various scripts and CSS. The sample project uses gulp and you can see how it's done in the gulpfile.js.
* You'll need a layout view for the forum to live in.
* Set up the various options in `Startup` as described in its comments and this documentation.
* `PopForums.json` will have your forum configuration.
* There is no package for the Azure Functions, because the SDK references are kind of whacked and it's currently hard to make them work from a shared library. However, you can deploy the project from this repo with ease given the tooling in VS or Azure DevOps Pipelines. Just be sure to set the right values up in the application configuration in the Azure portal.
* POP Forums uses ASP.NET Core Data Protection. Actually, the basic anti-forgery code baked into the framework does as well, so when you deploy, or swap deployment slots in Azure, you need to persist the underlying key somewhere. This is also true if you run multiple nodes (scale out). You can persist the underlying keys in a number of different ways (I prefer Azure Blob Storage). In your startup, in the `ConfigureServices()` method, use `services.AddDataProtection()` and the appropriate extension method. If you don't do this for multi-node, things like social logins and anti-forgery will fail and fill your error logs with stuff about broken things.

For the bleeding edge, latest build from master, the CI build packages can be obtained by a MyGet feed:
* https://www.myget.org/F/popforums/api/v3/index.json
* https://www.myget.org/feed/popforums/package/npm/@popworldmedia/popforums

## Installation

* Download the latest source code from GitHub, or use the production packages as described above. Build it.
* The project files require an up-to-date version of Visual Studio 2019 or later. It also appears to build in Visual Studio for Mac and Jetbrains' Rider.
* This project is built on ASP.NET Core v3.1.0. Make sure you have the required SDK installed (v3.1.100).
* The `PopForums.Web` project is the template to use to include the forum in your app. It references `PopForums.Mvc`, which contains all of the web app-specific code. `PopForums.Sql` concerns itself only with data, while `PopForums` works entirely with business logic and defines interfaces used in the upstream projects. `PopForums.AzureKit` contains a number of items to facilitate using various Azure services. `PopForums.AwsKit` contains an ElasticSearch implementation.
* The `master` branch is using Azure Functions by default to run background processes. A recent build of Visual Studio 2019 probably has all of the SDK's and storage emulators in place to host these. If not, you can run the background things in-process by uncommenting `services.AddPopForumsBackgroundServices()` in `Starup` and commenting out or removing `services.AddPopForumsAzureFunctionsAndQueues()`.
* `PopForums.json`, in the root of the web project, is the basic configuration file for POP Forums. It works like any other config file in ASP.NET Core, so when you're running in Azure, you can use the colon notation in the App Service application settings to set these values (i.e., `PopForums:Cache:Seconds` as the key).

> If you run the app in a Linux App Service or container, your settings notation should replace `:` with a double underscore, `__`. So the above would be `PopForums__Cache__Seconds`.
```js
{
	"PopForums": {
		"Database": {
			"ConnectionString": "server=localhost;Database=popforums14;Trusted_Connection=True;"
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
		"LogTopicViews": true, // records topic views for future analytics
		"ReCaptcha": { // Google ReCaptcha on signup (the key/secret below works on localhost)
			"UseReCaptcha": true,
			"SiteKey": "6Lc2drIUAAAAAPaa1iHozzu0Zt9rjCYHhjk4Jvtr",
			"SecretKey": "6Lc2drIUAAAAADXBXpTjMp67L-T5HdLe7OoKlLrG"
		}
	}
}
```
* Attempt to run the app either locally, or in IIS, and go to the URL /Forums to see an error page. It will fail either because the database isn’t set up, or because it can’t connect to it. The biggest reason for failure is an incorrect connection string.
* If you want to use the setup page (and you should), don’t run the SQL script. Once the POP Forums tables exist in the database, the setup page will tell you that you’re prohibited from going there.
* Point the browser to /Forums/Setup now, and if your connection string is correct, you should see a page with some of the basic fields to set up.
* Building requires that you have Node.js (and therefore npm) installed to get the client side references and run Gulp tasks to copy them to the wwwroot folder. If you run the app and the scripts and CSS are broken, it's because you don't have this. To troubleshoot Gulp action, right-click `gulpfile.js` in the web project and choose "Task Runner Explorer."
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
* Once you’ve added some forums on the “Forums” admin page, you can go to /Forums to start posting.
* If you want to test your e-mail setup, go to /Forums/Account/Forgot and enter your e-mail address. Failures are also logged in the error log, which is found in the admin area.
* For future reference, you can revisit the admin area at /Forums/Admin

## Integration

The `PopForums.Web` project is the template you can use as the basis for your own POP Forums apps. If you want to build via the most recent stable builds, the [POPWorldMedia/POPForums.Sample](https://github.com/POPWorldMedia/POPForums.Sample) project is an example of how to do that (see above). The app uses the standard claims-based authentication, but it does not use Identity or Entity Framework. When you're logged in, you'll find the identity of the user on the User property of the controller as expected. The `PopForumsAuthorizationMiddleware` loads user and profile data into the request pipeline, so it can be loaded once and used throughout the request lifecycle.

Accessing the user data can be achieved via an instance of `IUserRetrievalShim`, which you can inject into your dependency chain. From a controller, simply call `_userRetrievalShim.GetUser(HttpContext)` to get the fully hydrated POP Forums `User`, or `_userRetrievalShim.GetProfile(HttpContext)` to get the profile. Under the hood, these are stored in the `Items` collection of the current `HttpContext`.
