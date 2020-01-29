![POP Forums logo](https://avatars2.githubusercontent.com/u/8217691?s=200&v=4)

POP Forums
=========

A forum application with real-time updating and multiple languages.

The commercially hosted version appears at [popforums.com](https://popforums.com/). This is the open source version.

The master branch is now the work-in-progress for future versions. The v16.x branch is v16.x, for ASP.NET Core v3.1. v15 targeted Core v2.2. If you're looking for the version that works on .NET Framework 4.5+ with MVC 5, check out v13.0.2.

Roadmap:
The v16 release concentrates on performance and optimization, along with significant refactoring and bug fixes. Forthcoming releases will focus on UI refinement and modernization.

Try it out and make test posts here:  
https://meta.popforums.com/Forums

CI build of maste runs here:  
https://popforumsdev.azurewebsites.net/Forums

[![Build status](https://popw.visualstudio.com/POP%20Forums/_apis/build/status/popforumsdev)](https://popw.visualstudio.com/POP%20Forums/_build/latest?definitionId=2)

Latest release:  
https://github.com/POPWorldMedia/POPForums/releases/tag/v16.0.1
Packages available on NuGet and npm.

For the latest information and documentation, and how to get started, check the wiki:  
https://popworldmedia.github.io/POPForums/

The latest CI build packages can be found with these feeds on MyGet:  
https://www.myget.org/F/popforums/api/v3/index.json  
https://www.myget.org/F/popforums/npm/

## Prerequisites:
* .NET Core v3.1.
* npm and Node.js to build the front-end.
* AzureKit optionally requires Redis for two-level cache, Azure Search for Search.
* AwsKit optionally requires ElasticSearch for search.
