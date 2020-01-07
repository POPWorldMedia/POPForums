---
layout: default
title: Version History
nav_order: 4
---
# POP Forums Version History

Here's a partial version history that shows how POP Forums has evolved over the years. It's fun to look back at some of the things we now take for granted in a forum app.

## Version 16.0.1 (12/27/19)

* BUG: Redis cache helper can't serialize forum view/post graphs. #168

## Version 16.0.0 (12/17/19)

* Update to .Net Core v3.1. #151
* Redis connection failures recorded as "information" instead of "error." #162
* Optimize user objects and caching. #44
* Using "page" in routes can potentially interfere with Razor pages. #150
* Close dormant threads on a background job. #67
* Allow topic author to edit title. #80
* Refactor all the things to async/await. #132
* Refactor external logins to decouple from Identity (uses [POP Identity](https://github.com/POPWorldMedia/POPIdentity)). #140
* Cleanup startup configuration. #129
* Refactor the posting service classes. #121
* Cleanup unused methods and tests. #133
* Add unique constraints for user name and email. #144
* Add ReCAPTCHA option for login. #143
* Update to a stronger hash algorithm for passwords. #142
* Reintroduce profile caching. #148
* BUG: Failed reply doesn't wire up error message and attempts to redirect. #160
* BUG: Delete, undelete, hard delete doesn't remove from search index. #154
* BUG: Give background job to delete indexed words in self-rolled search more time. #149
* BUG: Creation of Redis connection not thread safe, possible race condition. #135
* BUG: Delete/undelete does not trigger a search reindex. #136
* BUG: Search indexer Azure function does not respect settings for provider setting. #137
* BUG: External login list has duplicate entries for description. #139

## Version 15.0.0 (5/27/19)

* General update of dependencies.
* Rewrite of admin to use Vue.js. #120
* Scaffolding for recording view data, correlating between users and topics (for future analytic use). #104
* Optionally run background processes as Azure functions. #76
* Social icons in profile. #119
* Redis backplane for SingalR in multi-node hosting. #64
* Migrated to Bootstrap v4.x. #94
* Abandon decade-old constructors in models. #96
* Remove first post preview from topic lists (no one used it). #97
* Truncate error log instead of delete all. #103
* Adopt Dapper usage in `PopForums.Sql` library. #105
* Improve performance for IP history and security log. #106
* Realign social links in profile to modern services. #107
* Provide a standard way to fail distributed events. #117
* Add support for ElasticSearch. #116
* BUG: Over-zealous regex hangs unparse of client HTML. #118
* BUG: Fix Favorite & Subscription Topic pages. #111
* BUG: It's possible to submit empty posts with just returns. #82
* BUG: TinyMCE is mangling hyperlinks. #125
* BUG: TinyMCE is inserting extra attributes in image tags, breaking parsing. #128

## Version 14.1.0 (12/9/18)

* Update to .Net Core v2.2.
* General package updates.
* Move all MVC views to a project that can be published as a Razor class library package.
* Remove ability to resize images in TinyMCE editor, since attributes are ignored anyway.
* Bug: for pager links in faves and subs (#90 and #91).

## Version 14.0.0 (7/15/18)

* This is a port of v13 to ASP.NET Core v2.1.0. It's mostly intended to achieve feature parity.
* Experimental AzureKit allows for multi-instance use and scaling.

## Version 13.0.0 (2/14/15)

* Completely revised UI uses Bootstrap, replaces separate mobile views.
* New Q&A style forums.
* Preview your posts for formatting.
* Social logins using OWIN 2.x.
* StructureMap replaces Ninject for dependency injection.
* Admins can permanently delete a topic.
* Facebook and Twitter links added to profiles.
* IP ban works on partial matches.
* Bug: Initial user creation didn't salt passwords ([Codeplex 131](http://popforums.codeplex.com/workitem/131))
* Bug: Replies not triggering reindex for topics (#4).
* Bug: LastReadService often called from Post action of ForumController without user, throws null ref (#1).
* Bug: Reply and quote buttons appear in posts even when the topic is closed (#8).
* Bug: When email verify is on, changing email does not set IsApproved to false (#10).
* Bug: Image controller throws when Googlebot sends a weird If-Modified-Since header (#13).
* Bug: Reply or new topic can be added to archived forum via direct POST outside of UI (#15).
* Bug: Multiple entries to last forum and topic view tables causing exception when reading values into dictionary (#17).
* Experimental: Support for multiple instances in Azure with shared Redis cache (not production ready).

## Version 12.1.0 (4/25/14)

* Added Taiwanese Mandarin translation
* Fixed HtmlHelper to remove reference to DependencyResolver. #122: HtmlHelper for role checkboxes referenced static DependencyResolver
* Disable submit buttons on new topics/replies for mobile views. #121: Port the submit button disable for posts in mobile view
* Fixed mobile view of edit doesn't parse text for overridden mobile mode. #123: Mobile post edit view has HTML, save doesn't persist line breaks

## Version 12.0.0 (12/8/13)

* Updated to use .NET 4.5.1 and MVC 5, including the latest library code (jQuery, SignalR, OWIN, etc.)
* User passwords now include per-user salt, backward compatible to existing user data. #120: Salt passwords by user
* External logins implemented via OWIN, for Google, Facebook, Twitter and Microsoft accounts. #117:Integrate the OWIN external auth stuff
* YouTube URL's not formatted as hyperlinks converted to embedded video, provided images are allowed in posts. #116: Parse YouTube URL's to convert into YouTube iframes
* Controller dependencies converted to private members. #115: Refactor controller dependencies to private members

## Version 11.1.0 (9/4/13)

* Added Ukrainian to supported languages. Also includes English, Spanish, German and Dutch.
* Fixed issue #115: Mobile view allows double post.

## Version 11.0.1 (5/15/13)

* Fix for issue #113: User can post in closed topic via mobile views.

## Version 11.0.0 (4/17/13)

* Updated to use v4.5 of .NET.
* External references now use NuGet.
* Adding an award definition in admin now bounces you to its edit page.
* Fixed: Show more posts updates topic context with updated page counts.
* Activities and awards restyled.
* User profiles are tabbed.
* Activity feed shows real-time view of activity sent via the scoring game API.
* Times are updated every minute, formatted to current culture.
* More posts are loaded on scroll (a la Facebook), but pager links are maintained for search engine discoverability.
* New posts appear inline at end of post stream as they're made.
* Forum home and individual topic lists updated in real time.
* Breadcrumb/navigation floats at top of browser.
* .forumGrid CSS removes outline, so it's more Metro-y.

## Version 10.0.1 (9/15/12)

This update has no UI component or data changes. It only addresses the following bug:
* ServiceModule has potential race condition in ASP.NET v4.5
* A bug in the MVC 4 framework requires this package for mobile views: http://nuget.org/packages/Microsoft.AspNet.Mvc.FixedDisplayModes

## Version 10.0.0 (8/16/12)

* Uses a very light weight CSS and Javascript package to provide a touch-friendly interface for mobile devices.
* Numbers are formatted (sensitive to culture) when 1,000 or higher.
* CSS is more integration friendly, and specific to the ForumContainer element.
* Mail delivery from queue is now parallel, so you can specify a sending interval, and the number of messages to process on each interval.
* Background "services" refactored, and will only run with a call on app start to PopForumsActivation.StartServices(). This is partly to facilitate future use in Web farms/multiple Web roles in Azure.
* Update to jQuery v1.7.1.
* Replaced use of .live() with .on() in script, pursuant to jQuery update, which deprecates .live().
* Renamed HomeController to ForumHomeController, to make lives easier when integrating into an MVC app.
* Dependency resolution no longer requires that you set Ninject as the container for the entire MVC app. The controllers now resolve their dependencies in their constructors, so you're free to set up any DI container in your global.asax.
* The included single-server SQL data layer now uses the base classes and interfaces for (DbConnection, DbCommand, etc.) instead of the specific SQL flavors, for easier refactoring in case you want to build an Oracle version or something.
* FIX: Bug in topic repository around caching keys for single-server data layer.
* FIX: Pager links on recent topics pointed to incorrect route.
* FIX: Deleting a post didn't update last user/post time.
* FIX: Ditched attempt at writing to event log with super failures, since almost no one has permission in production.
* FIX: Bug in grayed-out fields in admin mail setup.
* FIX: Weird color profiles would break loading of images for resize.
* FIX: TOS text on account sign-up was double encoded.

## Version 9.2.1 (1/26/12)

* Added Spanish (es) translation.

## Version 9.2.0 (1/23/12)

* Localization: The app can be easily translated using .resx files. Initially includes English (en), German (de) and Dutch (nl).
* Vote up posts: Give credit to people who make good posts, see who voted up each one.
* The scoring game: Extensible system that allows you to give users points, and issue awards based on repeated events. For example, you can set up awards based on the number of new posts or topics a user makes (both of which are recorded).
* Fix: Weird line breaks in lists when posting from Firefox.

## Version 9.1.1 (12/18/11)

* Corrects a bug that prevented a topic from being marked viewed when a user looked at it, resulting in "new post" indicators that didn't go away until the user marked the entire forum as read.

## Version 9.1.0 (12/15/11)

* New "adapter" interface for forums. Using the IForumAdapter interface, a developer can plug-in code that alters the model and/or resulting view on topic lists and the actual threads. For example, you might add to or alter the model, then present a different view to display the data. See the comments on the IForumAdapter interface for more information.
* Also new, users starting a reply will see a button indicating that they can load any new posts that have occurred since they started writing their apply, so they don't miss any of the conversation.
* Fix: Moderating topic title doesn't update the UrlName.
* SEO enhancement: Page links in topics and forums include rel="next" and rel="prev" to tell search engines there's more to look at.
* Fix: User post list had broken markup, preventing topic preview.
* Fix: Added missing permission checking on action methods to preview or load individual posts.
* User name in top nav now acts as a link to the user's profile.
* Fix: Cache key for caching view post roles was incorrect.

## Version 9.0.0 (4/24/11)

* Rewritten from scratch for ASP.NET MVC3, with reasonable test coverage
* Posts can be loaded inline
* Avatars and user images resized on server

## Version 8.5.0 (not publicly released)

* Added plug-in infrastructure to allow changes to UI. Made for photo forum on CoasterBuzz

## Version 8.0.0 (11/10/08)

* Added AJAX features, takes advantage of ASP.NET v3.5.

## Version 7.5.1 (2/9/05)

* Significantly altered the TextParser class
* Changed text in RegisterLogin.ascx to indicate e-mail is used to send activation code
* Topic titles now parsed for naughty words and HTML

## Version 7.5.0 (10/25/04)

* New text parsing engine
* New online user engine
* RichText control displays in Windows 2000
* Fixed member post paging
* Online user stats now draw from PopForums.OnlineUsers
* Added RequiredFieldValidator to SendPrivateMessage.ascx to check for a subject
* PM reply doesn't add endless string of "re:"
* Fixed PagerLinks.cs to correctly display tool tips
* Added PagedPagerLinks.cs to display paged results from new PopForums.Forum.GetTopics() overload
* Made the member mailer text box in the admin bigger
* Can't reply to closed topics, even if reply box was visible before submitting

## Version 7.0.3 (1/24/04)

* Fixed URL and image detection in TextParser.

## Version 7.0.2 (11/11/03)

* Updated the RichText class to make it compatible with a recent update to Internet Explorer.

## Version 7.0.1 (11/4/03)

* Removed license scheme to make POP Forums free.

## Version 7.0.0 (10/22/03)

* Total rewrite with separation of data, logic and user interface.
* Several user interfaces available at launch.
* Data caching can reduce database activity by 60% or more.
* A RichText server control that renders in Internet Explorer.
* Role-based security you can use anywhere on your site, based on ASP.NET's forms authentication.
* Discreet data class, so you can write your own to access any database. (SQL Server used by default.)
* A TimeAdjust class to convert times to local time zones, and even adjust for daylight savings.
* Text parsing engine takes care of HTML generated by RichText as well as traditional "forum code."

## Version 6.0.0 (7/1/02)

* Original ASP application ported to ASP.NET.
* Significant improvements to HTML parsing engine.
* Forum exists as a user control, just drop it in your .aspx page!
* E-mail notification for anyone who replies to a topic.
* All-CSS interface shipped along side "admin-able" formatting.
* Asynchronous mailing to opt-in members (no need to tie up your browser with mailing process).
* Last post indicated by member name.
* Scroll to new post bounces you to page with newest post.
* Role-based security. Only those in certain roles can access or post in the forums you designate.
* Robust forum property and ordering in admin area.
* Error logging to database.

## Version 5.1 (3/4/02)

* New HTML/forum code parsing engine.

## Version 5.0 (8/19/01)

* Support for SQL Server Full-Text Indexing built-in for much faster searching.
* WYSIWYG post editor for Internet Explorer users, normal for other browsers.
* Favorite topic list.
* Terms of Service agreement with first post, reset all members' agreement.
* Member post quotas, to limit "noisy" members.
* Support for Persits and Dimac mail components.
* Post preview.
* "Jump to" added to topic and thread pages to jump to another forum.
* Public stats shown optionally (number of sessions, topics, members, etc.)
* Client-side form validation of new topics or replies to challenge empty fields.
* Admin edit member accounts.
* Recent topics shows links to multi-paged topics.
* Error page for URL's of deleted topics and forums.
* Fixed edit page hang for long posts.
* Search only returns private forum results to those who have access.
* Removed "requires AspMail" from admin page.
* Moderator IP view spans all pages in thread.
* Manage e-mail notification page reformatted.
* Double carriage returns now parsed as p tags.
* Fixed CDONTS mailer for compatibility with settings of other components.
* Fixed br unparsing with forum code off.

## Version 4.0 (1/8/01)

* Added support for CDO, CDONTS and AspQMail components.
* Post new topic and reply links added to topic and thread pages.
* COPPA check defeat.
* Support for Spellchecker.net.
* Scroll to newest post link.
* Addition of pre and quote forum tags.
* Paged threads. Posts per page are admin-defined. Links to each page appear in topic list.
* Auto-login after first post.
* AIM and ICQ hyperlinks in member info.
* Non-logged-in forums indicates "register to track new posts."
* COPPA (Children's Online Privacy Protection Act) compliance.
* Moderators can view IP addresses of posters.
* E-mail notification of new posts when member starts thread.
* Member area to remove e-mail notification of new posts.
* Opt-out or delete account option built into special URL sent with mass mailings to members.
* Mail to friend now uses system e-mail contact as from address, member as replyto address (to handle SMTP authentication issues).
* Moving lone topic from forum no longer results in an error.

## Version 3.2 (11/12/00)

* Included explanation of smilies in help file.

## Version 3.1 (9/20/00)

* One-click close/open/delete threads for moderators.
* Parse ftp:// URL's.
* Post-and-close thread check box for moderators,
* Next/previous thread.
* Mail thread to a friend.
* Recent topics.
* Fixed left column text color in search results.

## Version 3.0 (8/22/00)

* Major rewrite of most code.
* Dozens of files consolidated.
* Made to be portable between systems.
* Extensive administration area.
* Custom formatting added to instantly change fonts, colors, etc.
* Search engine added.
* Private and archived forums added.

## Version 2.0 (1/31/00)

* Written for CoasterBuzz.
* Cleaner format.
* Forum off/on.
* "Mark posts read" time stored in profile instead of browser.
* Signature block added.
* Profile photo upload added.

## Version 1.0 (11/23/99)

* Written for Guide to The Point.
* Basic features only.