---
layout: default
title: Customization
nav_order: 2.5
---
# Customization

POP Forums is fairly easy to customize to make it your own. And please, make it your own, because the default Bootstrap style is pretty boring.

## Style

POP Forums uses [Bootstrap](https://getbootstrap.com/) for its base style. We're big fans of the library because it really does provide a solid starting point and mature components that feel like the _lingua franca_ of web user interfaces. It's also super easy to customize it to your liking with relatively little effort.

### The basic layout that hosts the forum
The basic page that we include in the main and sample repositories is fairly sparse:
```
<!DOCTYPE html>
<html>
    <head>
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <title>@ViewBag.Title</title>

        @await RenderSectionAsync("HeaderContent", false)
    </head>
    <body>
        <div class="container">
            @RenderBody()
        </div>
    </body>
</html>
```
You'll notice that after the `title` tags, we render a section called `HeaderContent`. This is where the forum drops in all of its `script` and `link` references for Javascript and CSS. If you want to go deeper, look in the `PopForums.Mvc` project at `/Areas/Forums/Views/Shared/PopForumsMaster.cshtml`.

### Style overrides
One simple approach to customizing the look is to include a style sheet, linked in the template header _after_ Bootstrap. So given the template shown above, you would include your `link` tag to your CSS after the `RenderSectionAsync` call. Mostly, your CSS will consist of things like font selection and colors on high-level elements. For example, given that you've imported fonts elsewhere:
```
body {
	font-family: 'Roboto Slab', 'Times New Roman', serif;
	font-size: 16px;
}

nav, h1, h2, h3, h4, h5, small, .small, .btn, .breadcrumb {
	font-family: 'Open Sans Condensed', Helvetica, Arial, sans-serif;
	font-weight: 700;
}

a, a:visited, a:hover, .btn-link {
	color: red;
	text-decoration: underline;
}
```
If you wanted to override something specific in Bootstrap, like adding a big border to buttons, you would have to make sure you apply an `!important` designation.
```
.btn {
	border: solid 2px #000000 !important;
}
```

### Bootstrap replacement
You can also use your own Bootstrap build and go crazy with customization on the variables. There are great pre-built themes to download from [Bootswatch](https://bootswatch.com/), and [Bootstrap Build](https://bootstrap.build/) has a hassle-free way to customize everything and output a ready-to-use build of Bootstrap. Of course, you can get the [Bootstrap source](https://github.com/twbs/bootstrap) and modify any of the variables or `SCSS` files and build your own, if you like.

To use your own Bootstrap build from any of the methods above, first you have to turn off the rendering of the default Bootstrap included in the `PopForums.Mvc` package. To do so, you'll need to change the `appsettings.json`:
```
{
  "PopForums": { 
    "RenderBootstrap": false, 
    ...
```
With that taken care of, considering the template above, add references to your built Bootstrap CSS _and_ script _before_ you render the header content:
```
<script src="~/pathToMyBootstrap/bootstrap.bundle.js" asp-append-version="true"></script>
<link href="~/pathToMyBootstrap/bootstrap.min.css" rel="stylesheet" asp-append-version="true" />

@await RenderSectionAsync("HeaderContent", false)
```

## Forum adapters

The MVC project has an interface called `IForumAdapter`, which allows you to generate your own view model for a topic, typically with new or augmented data. When a forum adapter is configured in the admin of the app (on a per-forum basis), it uses the code in that configured adapter to render a specific view (typically in the `Views/Shared` folder of your app) and the model the adapter specifies. Consider the following:
```
public class TestAdapter : IForumAdapter
{
    public Task AdaptForum(Controller controller, ForumTopicContainer forumTopicContainer)
    {
        // not changing anything in the forum (topic list), just set the model as the existing container
        Model = forumTopicContainer;
        return Task.CompletedTask;
    }

    public async Task AdaptTopic(Controller controller, TopicContainer topicContainer)
    {
        // for the topic (thread) view, let's use the existing model, but add something to the `ViewBag` for the view
        Model = topicContainer;
        
        // now get the "extra"
        var resolver = controller.HttpContext.RequestServices;
        var scoreService = (IScoreService)resolver.GetService(typeof(IScoreService));
        var highScore = scoreService.GetHighScore();
        controller.ViewBag.HighScore = highScore;
        // use Shared/ScoreTopic.cshtml for the view
        ViewName = "ScoreTopic";
    }
    ...
```
Now a different view will be rendered, and it might look something like this:
```
@model PopForums.Models.TopicContainer

<h1>High score: @ViewBag.HighScore</h1>

@foreach(var post in Model.Posts) {
    // render posts from the model
```

This is super flexible, but not without a lot of work. You're highjacking the model from a page like `/Forums/Topic/some-topic`, replacing it with your own, then rendering your own view.

To light one of these up, you'll have to go to the admin -> Forums -> Edit, and in the bottom field, labeled `Forum Adapter (optional, use "Namespace.Type, AssemblyName")`, you should enter what it's asking for. So if the fully qualified name of your adapter is `MyLibrary.MyForumAdapter` in the `MyLibrary` assembly, you would put `MyLibrary.MyForumAdapter, MyLibrary` here.

## Swapping out anything with an interface

Almost everything in POP Forums is an interface, and uses dependency injection to get the right thing running. In `PopForums.Extensions.ServiceCollections.AddPopForumsBase(this IServiceCollection services)`, you'll see how various classes are registered with the service collection, to be instantiated at runtime. Other libraries, like the `Mvc` library (in `AddPopForumsMvc()`), register additional classes, some of which cross into the domain of the web-based app, keeping web dependencies out of the core library.

In the `AzureKit` project, you can see how we replace certain implementations to use the alternate functionality. `ServiceCollectionExtensions.AddPopForumsAzureFunctionsAndQueues(this IServiceCollection services)` replaces a bunch of implementations to shift the background processes and queues to the `AzureKit` bits:

```
services.Replace(ServiceDescriptor.Transient<IEmailQueueRepository, PopForums.AzureKit.Queue.EmailQueueRepository>());
services.Replace(ServiceDescriptor.Transient<IAwardCalculationQueueRepository, PopForums.AzureKit.Queue.AwardCalculationQueueRepository>());
services.Replace(ServiceDescriptor.Transient<ISearchIndexQueueRepository, PopForums.AzureKit.Queue.SearchIndexQueueRepository>());
services.Replace(ServiceDescriptor.Transient<ISubscribeNotificationRepository, PopForums.AzureKit.Queue.SubscribeNotificationRepository>());
services.Replace(ServiceDescriptor.Transient<IRenewalQueueRepository, PopForums.AzureKit.Queue.RenewalQueueRepository>());
```
The part in the generic portion of the function call first names the interface, followed by the concrete implementation to be used when another class calls for that interface. You could, in theory, replace most of the code in any of these cases to do something you would prefer. Most of the code base isn't written with this in mind, as the primary intention is to be able to test classes in as isolated a fashion as possible. But there are certain areas, like the above queueing example, where swapping out the parts was always a design intention.

Importantly, replacing implementations requires that they were registered in the first place. These must happen in order in your `Program.cs` (or library) after the core bits are registered.

Perhaps the biggest example is the entire `Sql` library. If you wanted to write a persistence layer for another database, or even to write to text files if that's your thing (and you want to go back to 1997), you can do that. It might be more useful to port that project for something useful, like PostgreSQL. You would also need a new implementation of `ICacheHelper`. That would be a pretty great pull request.