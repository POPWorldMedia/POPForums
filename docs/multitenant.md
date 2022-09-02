---
layout: default
title: Multi-tenant options
nav_order: 8
---
# Multi-tenant options

POP Forums has some plumbing for multi-tenancy, originally created to facilitate the [cloud-hosted version of POP Forums](https://popforums.com/). However, there are some tricks here that you can rely on for shared resource scenarios.

# Using `ITenantService`

The core library defines `ITenantService` and provides a basic implementation. It has two methods, `SetTenant(string tenantID)` and `GetTenant()`. The former throws a `NotImplementedException` and is not called by any of the code in this repository. It's there for you to use in a true multi-tenant environment. The latter is used all over the place, and the default implementation returns an empty string.

# Example: Sharing an instance of ElasticSearch

Let's say that we have three different sites running their own copy of POP Forums, each with their own database, in a shared pool in Azure. Having multiple databases doesn't cost you anything extra in this scenario, but if you're using managed ElasticSearch hosted by Elastic (also in Azure), you may want to share a single instance. Like many of the resources in POP Forums, the code in [`PopForums.ElasticKit`](elastickitlibrary.md) makes sure to store and query data with a `TenantID`. By default, this doesn't matter, because the ID is just a blank string.

To share this resource, create an implementation for each of your apps. You'll implement just the `GetTenant()` method:
```
public class TenantService : ITenantService
{
    public void SetTenant(string tenantID)
    {
        throw new System.NotImplementedException();
    }

    public string GetTenant()
    {
        return "mytenantid"; // unique for every app
    }
}
```
Then, in your `Program.cs` file, swap out the default implementation for your own. If you're using Azure Functions with [`PopForums.AzureKit`](azurekitlibrary.md), be sure to do it in the function project's `Program.cs` as well:
```
services.Replace(ServiceDescriptor.Transient<ITenantService, MyApp.TenantService>());
```
In our ElasticSearch scenario, the indexer will store the ID with every document, and searches will filter by it.

# What uses `TenantID`?

It's a long list that is best explored in the source code by finding usages of the `GetTenant()` method, but here's a non-exhaustive list:

Core libraries:
* All of the queue messaging. That means you'll find a TenantID in the code that dequeues the messages (primarily Azure Functions).
* SignalR plumbing, so clients listening for notifications, for example, are unique to the tenant.

In `PopForums.AzureKit`:
* `IPostImageRepository` (used in naming blobs for image storage)
* All of the Redis bits, so you can share a Redis instance.

In `PopForums.ElasticKit`:
* All of the ElasticSearch bits, see example above.
