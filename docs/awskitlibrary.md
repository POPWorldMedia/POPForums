---
layout: default
title: Using AWS Kit Library
nav_order: 7
---
# Using AwsKit Library
The `PopForums.AwsKit` library makes it possible to wire up the following scenarios:
* Use ElasticSearch for search instead of the built-in search indexing. _Important: The client library referenced in v15.x is designed to work against v6.x of ElasticSearch, while v16.x uses v7.x of ElasticSearch._

You don't need to use the AwsKit components to run in AWS. You also don't need to run in AWS to use ElasticSearch. ElasticSearch can run quite literally anywhere in a docker container. Also keep in mind that the implementation that AWS uses is actually a fork, so there are some differences about how the managed service is, uh, managed.

## Configuration with Azure App Services and Azure Functions

The POP Forums configuration system uses the PopForums.json file, but adhere's to the overriding system implemented via environment variables. This by extension means that you can set these values in the Application Settings section of the Azure portal for App Services and Functions. It uses the colon notation that you may be familiar with. For example, use `PopForums:Queue:ConnectionString` to correspond to the hierarchy of the PopForums.json file. (For Linux-based App Services and Functions in Azure, use a double underscore instead of colons in the portal settings, i.e., `PopForums__Queue__ConnectionString`.)

## Using ElasticSearch for search
ElasticSearch is a search engine you can run on your own or in managed services from AWS, Elastic and others. To use this service instead of the internal POP Forums search indexing, you'll need to configure this line in your Startup.cs if you're using web in-process search processing:

```
using PopForums.AwsKit;
...
namespace YourWebApp
{
	public class Startup
	{
	...
		public void ConfigureServices(IServiceCollection services)
		{
			...
			services.AddPopForumsElasticSearch();
```

For use in the Azure functions, you'll need to set the `PopForums:Search:Provider` setting in the portal blade for the functions to `elasticsearch`.

You'll also need to setup the right configuration values if you're running web in-process:

```
{
  "PopForums": {
    "Search": {
      "Url": "https://myelasticsearchindex",
      "Key": "",
      "Provider": ""
    },
```
* `Url`: The base URL for the ElasticSearch endpoints
* `Key`: After v16.x, this will optionally set an API key, using the format `id|key` (that's a pipe separating the ID and API key). Make sure that you use the ID, not the name.
* `Provider`: This is optional and not actually implemented anywhere other than in our Azure Functions example project, where it's used to switch between `elasticsearch`, `azuresearch` and the default bits in the `PopForums.Sql` library.

Configuring ElasticSearch and setting up security rules for it are beyond the scope of this wiki.
