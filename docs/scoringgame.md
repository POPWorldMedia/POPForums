---
layout: default
title: The Scoring Game
nav_order: 8
---
# The Scoring Game

Let me tell you a story of HR-discouraged workplace fun. Back in the day, prior to the crash-and-burn of Insurance.com, we had this thing in the development part of the company called the Scoring Game. [I wrote about it](https://jeffputz.com/blog/the-scoring-game) a couple of years ago on my personal blog. The long and short of it is that we kept a running total of +/-1’s for virtually anything you can think of, for each participant. This was back in 2006, before it became trendy to do it for everything else on the Internets.

Later, Digg started doing all kinds of voting, and it was really the first active example that I can think of that I used in terms of measuring value of content (yes, slashdot did it, but I never went there). Various forums started doing it. StackOverflow based much of its value on a scoring system, along with achievements. When I worked at Microsoft, I worked on the reputation system that feeds the various MSDN properties. It seems inevitable that I’d have to add something like this to POP Forums.

Originally, I was thinking just in terms of voting up posts, but then I realized that there were actually two things to build. The voting mechanism was one part, but the actual scoring was a second part that should be decoupled from the voting. So the workflow goes like this:

Process Event –> Publish to user profile (optional) –> Get associated awards –> Qualify awards –> Give award

To use the system, you only need only a few lines of code. Use the dependency injection to get the implementation of `PopForums.ScoringGame.IEventPublisher`. If you're using the typical constructor injection, you'll probably have a reference to it in your class called `_eventPublisher`. The user can be obtained from an instance of `PopForums.Mvc.Areas.Forums.Services.IUserRetrievalShim` in the controller layer.

```c#
_eventPublisher.ProcessEvent("message for feed", user, "TestEventID", false);
```

Pretty simple, eh? The first string is the text that will be published to the user’s feed (if the event is set to publish), the second is the `PopForums.Models.User` object to associate with the event, and the third is the actual event ID. Event definitions are really simple.

There are three events that are static, permanently built into the system. These are wired into the post voting, and the creation of new topics and posts. So for example, when someone votes up a post, a string of HTML is passed in to the ProcessEvent() method, with the user object associated with the post, and the event ID PostVote.

Events don’t have to be published to the user’s profile, and they don’t even need to assign points. New posts and topic events fall into this category. So what’s the point then? Awards! POP Forums leaves that up to you. Award definitions are super simple as well. We can assign any combination of events to the award.

That’s really all there is to it. You can set up stuff anywhere in your app to record events, and publish them to the user profile. Give points, give awards. Knock yourself out!