![POP Forums logo](https://avatars2.githubusercontent.com/u/8217691?s=200&v=4)

POP Forums
=========

A forum and Q&A application with real-time updating, image uploading and private message chat in multiple languages.

The main branch is now the work-in-progress for future versions running on .NET 9+. The v21.x branch is v21.x, running on .NET 9. If you're looking for the version that works on .NET Framework 4.5+ with MVC 5, check out v13.0.2.

Roadmap:
v21 is another iterative release, leaning hard into refactoring and a few performance improvements. I've observed fast page rendering, average 20ms on Azure App Service P0v3 and SQL elastic pool at 50 eDTUs and 900k posts. Future versions will consider issues in the backlog.

For the latest information and documentation, and how to get started, check the pages (also in markdown in `/docs` of source):  
https://popworldmedia.github.io/POPForums/

Try it out and make test posts here:  
https://meta.popforums.com/Forums

CI build of main runs here:  
https://popforumsdev.azurewebsites.net/Forums

[![Build status](https://dev.azure.com/popw/POP%20Forums/_apis/build/status/popforumsdev)](https://dev.azure.com/popw/POP%20Forums/_build/latest?definitionId=13)

Latest release:  
https://github.com/POPWorldMedia/POPForums/releases/tag/v20.0.0  
Packages available on NuGet.

The latest CI build packages can be found with these feeds on MyGet:  
https://www.myget.org/F/popforums/api/v3/index.json   

Sample app using only the packages:  
https://github.com/POPWorldMedia/POPForums.Sample  

## Prerequisites:
* .NET v9.
* npm and Node.js to build the front-end.
* AzureKit optionally requires Redis for two-level cache, Azure Search for Search.
* AzureKit optionally requires an Azure Storage account for image storage, queues and Azure Functions.
* ElasticKit optionally requires ElasticSearch for search.
* Works great on Windows, Mac and Linux.
* Build with Visual Studio or JetBrains Rider.
