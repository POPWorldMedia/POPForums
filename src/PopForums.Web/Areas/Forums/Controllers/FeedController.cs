using Microsoft.AspNetCore.Mvc;
using PopForums.Feeds;

namespace PopForums.Web.Areas.Forums.Controllers
{
	[Area("Forums")]
	public class FeedController : Controller
    {
	    private readonly IFeedService _feedService;

	    public static string Name = "Feed";

	    public FeedController(IFeedService feedService)
	    {
		    _feedService = feedService;
		}

		public ViewResult Index()
		{
			var feed = _feedService.GetFeed();
			return View(feed);
		}
	}
}
