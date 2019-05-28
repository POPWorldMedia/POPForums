POPForums
=========

A forum application with real-time updating and multiple languages.

The master branch is now the work-in-progress for future versions. The v15.0.0 branch is v15.0, for ASP.NET Core 2.2. If you're looking for the version that works on .NET 4.5.x with MVC 5, check out v13.0.2.

Roadmap:
The v15 release brings mostly scale-out features and support, along with significant refactoring and bug fixes. It also features a rebuilt admin area, a kind of rough-in using Vue.js to explore its broader suitability (hint: it's solid!). Forthcoming releases will focus on UI refinement and more refactoring.

CI build of master, running on .NET Core is demo'ing here:
https://popforumsdev.azurewebsites.net/Forums

[![Build status](https://popw.visualstudio.com/POP%20Forums/_apis/build/status/popforumsdev)](https://popw.visualstudio.com/POP%20Forums/_build/latest?definitionId=2)

Latest release:
https://github.com/POPWorldMedia/POPForums/releases/tag/15.0.0

For the latest information and documentation, and how to get started, check the wiki:
https://github.com/POPWorldMedia/POPForums/wiki

The latest CI build packages can be found on MyGet:
https://www.myget.org/feed/Packages/popforums

## Prerequisites:
* .NET Core v2.2.
* AzureKit optionally requires Redis for two-level cache, Azure Search for Azure Search.
