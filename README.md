POPForums
=========

An MVC forum application with real-time updating and multiple languages.

The master branch is now the work-in-progress of v14, which will be for ASP.NET Core 2.x. If you're looking for the version that works on .NET 4.5.x with MVC 5, check out v13.0.2.

Roadmap:
Yeah, so you might ask why this project gets updated infrequently, and there hasn't been a release for ASP.NET Core. The answer is that Core has been too much of a moving target, plus day jobs and stuff. The release criteria at this point is at least beta status for external dependencies, and we're getting close now. The goal is mostly feature parity (and fewer bugs) with v13. After the release of v14, I'll be looking at modernizing the app, but requirements are largely driven by the need to make https://coasterbuzz.com work. It's due for a refresh.

CI build of v14, running on .NET Core is demo'ing here:
https://popforumsdev.azurewebsites.net/Forums

Latest release:
https://github.com/POPWorldMedia/POPForums/releases/tag/v13.0.2

For the latest information and documentation, check the wiki (admittedly needs a lot of work):
https://github.com/POPWorldMedia/POPForums/wiki

## Prerequisites:
* .NET Core v2.
* AzureKit optionally requires Redis for two-level cache, Azure Search for Azure Search.
