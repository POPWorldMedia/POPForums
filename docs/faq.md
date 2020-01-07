---
layout: default
title: FAQ
nav_order: 3
---
# Frequently Asked Questions

These are a few of the questions people ask me about the project. Feel free to ping me with other questions at jeff@popw.com. If you're thinking it, you're probably not the only one! If you find a defect or want to request a feature, use the issues here on GitHub for that, please.

## Do I have to pay for this or not?
Not. POP Forums is an open source software project hosted on GitHub and for use under the MIT license. There is a commercially hosted version available, yes, for people who don't write code or don't want to mess with managing their own software. Everything on GitHub continues to be open source.

## Another forum app? For real?
Yeah, I know. I'd like to think that this one is a little different, because it doesn't exist to fit some generalized needs, it exists to fit the needs of real communities, like [CoasterBuzz](https://coasterbuzz.com/). The design goal of the app, from its early days in 1999, has always been to design for users, and not be a science project. This app lives because it has been required for sites like CoasterBuzz for more than two decades, and it will continue to evolve because those sites will evolve. It just makes sense to share it with others.

## Sounds like you've been doing this a long time.
Yes, I sometimes feel cursed to rewrite it for all eternity. The Webforms versions were really kind of a mess, and no version was a true rewrite. Once MVC came along, it gave me great incentive to start fresh. Dotnet Core and the evolving front-end frameworks give plenty of new opportunities for refactoring.

## Is this project the basis for the commercial hosted product?
Yes, it's the very same code, though obviously decorated with additional code to facilitate multi-tenancy and provisioning.

## What languages are supported?
Currently we have English, Spanish (es), Dutch (nl), Ukrainian (uk), Taiwanese Mandarin (zh-TW) and German (de). If you'd like to translate, the .resx file has between 350 and 400 entries. Contact me to learn more, and we can talk about a pull request to add another language.

## You used to work on the forums for MSDN and TechNet. Is this that forum?
Not at all. That app served a great many different functions and was integrated with Microsoft ID's, a centralized profiling system, etc. It was/is huge. This app has its roots in the web sites I've been running for fun and profit for years, to the extent that you can find old posts on those sites from the turn of the century with all kinds of formatting failures. Those were the ASP.old days.

## I noticed you're not using [some ORM framework]. Why not?
One of the requirements back in the day was to simply work with the existing data structures of v8.x, a Webforms app. In that sense, the data plumbing was already pretty well established and known to work, and it has followed all the way up through the Core version. My opinion is that ORM's tend to be a leaky abstraction that never work in the black box way that you would hope. I have adopted Dapper though, which covers the core use case that you're really after anyway: Mapping parameters to queries and results to objects. One doesn't have to write actual SQL all that often.

## You don't name your async methods with the `Async` suffix. Just who do you think you are?
Look, when almost all of your methods are async what's the point? The only place I use it is when there are both synchronous and asynchronous methods. Your fancy IDE knows what the return type is, and the compiler lets you know when you're not awaiting. You'll be fine.

## What external frameworks are you using, and why?
I wanted to keep external binaries to a minimum, but I'm using MailKit for email functions, ImageSharp for photo resizing, Moq for test mocking, and xUnit for unit testing. On the front end, it's still using jQuery (for now), along with Bootstrap and TinyMCE. The admin area uses Vue.js. Github has that handy dependency graph now that you can look at for more information.

## The unit tests suck.
In porting to Core, much of the controller-level unit testing didn't come along, because it's likely that it will change significantly when the front-end becomes more modern. See next item...

## What's the release roadmap?
It has generally been my intention to keep up with the latest .NET framework versions, but getting to Core took way longer than expected. You can check the issue tracker for stuff currently in flight. I have strong desire to modernize the front end, though not in a "heavy" way. Forums attract Google juice because they're textually dense, and the "app-ness" of a forum is not complex as it's little more than a few simple forms. To that end, sure, you could go nuts and rewrite the whole thing to use a SPA framework, but at great indexability cost. So while there will be changes to bring it forward, they'll be targeted.

## Can I contribute?
I very much welcome translations of the .resx files, so send a pull request for those immediately! If someone really digs into the source code and understands it in a non-trivial way, then yes, I'll happily accept pull requests. If you can find a bug to squish from the issue log, that would be a great PR to see!