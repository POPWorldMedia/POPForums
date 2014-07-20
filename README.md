POPForums
=========

An MVC forum application with real-time updating and multiple languages.

*** Note: We moved from CodePlex recently, so documentation and such is still a bit in transition. ***

POP Forums v12 is a forum app for ASP.NET MVC used as the core for several sites maintained by the author. It is not a science project, but a long-term commitment to great community. 

The project goals include are: 
Use ASP.NET MVC, including a mobile interface.
Make the project open source under Ms-PL.
Be the best ASP.NET-based forum.
Not duplicate UBB's 1998 UI for the Nth time.
Localize: Now available in English, Spanish, German, Ukrainian, Dutch and Taiwanese Mandarin.

Setup:

To set it up, check the installation instructions in the wiki.

Update: 4/25/14

v12.1 has been released! The details:

- Added Taiwanese Mandarin translation
- Fixed HtmlHelper to remove reference to DependencyResolver. #122: HtmlHelper for role checkboxes referenced static DependencyResolver
- Disable submit buttons on new topics/replies for mobile views. #121: Port the submit button disable for posts in mobile view
- Fixed mobile view of edit doesn't parse text for overridden mobile mode. #123: Mobile post edit view has HTML, save doesn't persist line breaks

v12 has been released! Here's what's new:

- Updated to use .NET 4.5.1 and MVC 5, including the latest library code (jQuery, SignalR, OWIN, etc.)
- User passwords now include per-user salt, backward compatible to existing user data. #120: Salt passwords by user
- External logins implemented via OWIN, for Google, Facebook, Twitter and Microsoft accounts. #117:Integrate the OWIN external auth stuff
- YouTube URL's not formatted as hyperlinks converted to embedded video, provided images are allowed in posts. #116: Parse YouTube URL's to convert into YouTube iframes
- Controller dependencies converted to private members. #115: Refactor controller dependencies to private members

POP Forums v12 for ASP.NET MVC 5 is available right now. Want to see it running in production? Check out CoasterBuzz, a community for roller coaster enthusiasts. The forum is skinned to the look of the site, while the entire site uses the mobile view framework (with color changes to the skin).

Do you speak English and another language? We want to make POP Forums globally useful. Starting with v9.2, the app is easily localized. We have volunteers translating to Spanish, Dutch, Ukrainian, Taiwanese Mandarin and German, and we'd love help for additional languages. Drop Jeff an e-mail to jeff@popw.com for more information.

Found a bug? Add it to the issue tracker.
