using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PopForums.Feeds;

namespace PopForums.Mvc.Areas.Forums.Controllers
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

		public async Task<ViewResult> Index()
		{
			var feed = await _feedService.GetFeed();
			return View(feed);
		}
	}
}
