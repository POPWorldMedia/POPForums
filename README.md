![POP Forums logo](https://avatars2.githubusercontent.com/u/8217691?s=200&v=4)

POP Forums
=========

A forum and Q&A application with real-time updating in multiple languages.

The commercially hosted version appears at [popforums.com](https://popforums.com/). This is the open source version.

The main branch is now the work-in-progress for future versions running on .NET 6+. The v18.x branch is v18.x, running on .NET 6. If you're looking for the version that works on .NET Framework 4.5+ with MVC 5, check out v13.0.2.

Roadmap:
The v18 release concentrates on performance and optimization, along with significant refactoring to use modern frameworks and bug fixes. Forthcoming releases will focus on UI refinement and modernization.

For the latest information and documentation, and how to get started, check the pages (also in markdown in /docs of source):  
https://popworldmedia.github.io/POPForums/

Try it out and make test posts here:  
https://meta.popforums.com/Forums

CI build of master runs here:  
https://popforumsdev.azurewebsites.net/Forums

[![Build status](https://popw.visualstudio.com/POP%20Forums/_apis/build/status/popforumsdev)](https://popw.visualstudio.com/POP%20Forums/_build/latest?definitionId=2)

Latest release:  
https://github.com/POPWorldMedia/POPForums/releases/tag/v18.0.0  
Packages available on NuGet and npm.

The latest CI build packages can be found with these feeds on MyGet:  
https://www.myget.org/F/popforums/api/v3/index.json   
https://www.myget.org/F/popforums/npm/  

## Prerequisites:
* .NET v6.
* npm and Node.js to build the front-end.
* AzureKit optionally requires Redis for two-level cache, Azure Search for Search.
* ElasticKit optionally requires ElasticSearch for search.
* Works great on Windows and Linux.
