POPForums
=========

A forum application with real-time updating and multiple languages.

The master branch is now the work-in-progress for future versions. The v14.1.0 branch is v14.1, for ASP.NET Core 2.2. If you're looking for the version that works on .NET 4.5.x with MVC 5, check out v13.0.2.

Roadmap:
The v14 release is a port of v13, bringing the app to .Net Core. It doesn't add any substantial features over the previous version (less bugs), as the intention was largely feature parity to the "old" MVC version. Going forward, I'll be looking at modernizing the app, but requirements are largely driven by the need to make https://coasterbuzz.com work. It's due for a refresh.

CI build of master, running on .NET Core is demo'ing here:
https://popforumsdev.azurewebsites.net/Forums

[![Build status](https://popw.visualstudio.com/POP%20Forums/_apis/build/status/popforumsdev)](https://popw.visualstudio.com/POP%20Forums/_build/latest?definitionId=2)

Latest release:
https://github.com/POPWorldMedia/POPForums/releases/tag/v14.0.0

For the latest information and documentation, check the wiki:
https://github.com/POPWorldMedia/POPForums/wiki

The latest CI build packages can be found on MyGet:
https://www.myget.org/feed/Packages/popforums

## Prerequisites:
* .NET Core v2.2.
* AzureKit optionally requires Redis for two-level cache, Azure Search for Azure Search. This is experimental.
