---
layout: default
title: Architecture
nav_order: 2.5
---
# POP Forums Architecture

Forums are a text-driven medium intended to enable communication between people. To that end, the design philosophy behind POP Forums has always been to keep the interface simple, and not fill the screen with interface elements that don't serve that philosophy. Things that you don't need are hidden by default. Conversation comes first. Peripheral concerns include search engine optimisation, performance and maintainability.

As an open source project, intended first to be used in community sites like [CoasterBuzz](https://coasterbuzz.com/), its evolution is not perfect, or even ideal, but it does represent two decades of refinement. Some parts are pretty cool, others desperately need to be refactored. No pull request will be ignored!

## Data structure

POP Forums was originally written using SQL Server as a data store, but the data access bits are contained entirely in the `PopForums.Sql` project. An enterprising developer could easily port this to any of the technologies supported by the .Net ecosystem (which is to say, all of them). This library also contains a basic caching layer, leveraging in-memory cache. It is seeded by a single SQL script, `PopForums.sql`. This is the basic configuration, and it works fine for light duty use on cheap hardware, virtual or otherwise.

To facilitate scaling, caching can be delegated to Redis, using the `PopForums.AzureKit` library. (Instructions are found in the [Using AzureKit Library](azurekitlibrary.md) section.) This employs a two-level cache that combines the use of Redis and local memory. The app will first attempt to retrieve data from local memory, and if it's not available, it will attempt to retrieve it from Redis. If it's not available there, it will fetch from SQL and then cache that data as apporopriate. This also enables the use of multiple nodes by using the message bus in Redis. When the local cache needs to be invalidated on the other nodes, a message is sent to them via the bus. I didn't invent this pattern, but saw it on a Stack Overflow blog post.

The basic search functionality is performed by building an enormous table of words, then matching and scoring them for results. The algorithm to index the words is not efficient, but this too works fine for light duty use. Using the `PopForums.ElasticKit` library enables, wait for it, the use of ElasticSearch. No need to invent something, because Elastic does this really well. (Instructions are found in the [Using ElasticKit Library](elastickitlibrary.md) section.) This is a more robust solution that can scale to larger forums, and it can be used in conjunction with the Redis caching layer.

There is a bit of what I call "precomputing" of statistics, because it doesn't make sense to be doing aggregate counts via SQL queries. For example, the post count on a topic is incremented or decremented at the time a post is added or (soft) deleted.

## Background Processing

A number of different tasks are necessarily performed asynchronously:
* Award Calculation
* Close Aged Topics
* Email
* Post Image Cleanup
* Search Indexing
* Subscribe Notifications
* User Session Cleanup

The default implementation uses jobs registered as derivatives of `Microsoft.Extensions.Hosting.BackgroundService`. This works fine in a single-node environment, and most of the actions are not resource intensive, save for the search indexing (regardless of using the base search or Elastic).

The solution for that is to encapsulate the jobs as Azure Functions. These are fantastic in a production situation because you'll literally spend pennies a month on them, while not impacting the resources of your web nodes. If you run on Azure, this is a no-brainer.

## Image Handling

Out of the box, POP Forums stores post images in the database. Streaming those bytes out of SQL Server all the way to the browser is reasonably efficient, but the memory cost is not zero. The advantage of this arrangement is that the forum is very portable, as you can export the database to a `.bacpac` file and restore it, with images, anywhere. User avatars and images are also stored this way.

For scale, especially if you pay for your database by the bit, it may be preferable to store post images in Azure Blob Storage. If you're using Azure Functions, you'll already need a storage account for the background queues. This too is configured using the `PopForums.AzureKit` library, saving images to a public blob container. This means that you could also use a CDN, if you're really nuts about performance.

Because a user might upload an image but not submit the post, a cleanup job is run to remove images that are not associated with a post.

## Scaling POP Forums

The above options allow you to greatly scale the application. Because the app uses ASP.NET's authorization and authentication, running multiple nodes requires a shared data protection key. This is outlined elsewhere and set in the `Program.cs` startup.

Using the CoasterBuzz database as a reference, three Azure Web App instances, running on P0v3 Linux machines, backed by a 50 DTU SQL database, can handle 1,000 requests per minute without issue. I haven't tested with higher loads and configurations, but it's not clear that either would be a bottleneck running at higher levels.

## The Web Application

All of the web app code is contained in the `PopForums.Mvc` project, which also references the base `PopForums` and `PopForums.Sql` libraries, and the above-mentioned libraries as necessary for scale. The assets, including all of the CSS and transpiled TypeScript, are also shipped in the `Mvc` library. This makes it really easy to update the forum bits in your own application, without having to replace a bunch of loose files. Update the package, and you're done.

The app leverages ASP.NET's SignalR for real-time communication with the server via web sockets, including notifications of new posts, updating the forum and topic grids, new private messages, etc.

In accordance with the simple design philosophy, the web app does not use any specific front-end library, aside from Vue.js, which is used for the admin interface. Again, the intent is to produce search engine friendly markup without a web of dependencies and npm packages. That doesn't mean that there isn't any rich interactivity, because a number of small, raw elements are written in TypeScript. They live in `PopForums.Mvc/Client`. Along with a few small service classes and a simple stage engine, "reactive" elements are updated when a notification comes in via web sockets.

While server-side localization is straight forward enough, the client-side bits use a small JSON payload apply the right language to the interface. For example, the use of time words varies by language, so the `FormattedTime.ts` component uses those strings for "5 minutes ago" or whatever the right variant is. 

## Unit Testing

The unit test suite is rough, because there are parts of the app that have not been refactored from an earlier state. A few of the service classes look like dumping grounds, and many do too many things. That makes writing tests for those retroactively difficult, and likely unnecessary if they'll eventually be refactored anyway. You'll also find code in the controllers that likely doesn't belong there. Again, this is after two decades of change. Like any good project, it will never be "done."