using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PopForums.Feeds;
using PopForums.Mvc.Areas.Forums.Authorization;

namespace PopForums.Mvc.Areas.Forums.Controllers
{
	[Area("Forums")]
	[TypeFilter(typeof(PopForumsPrivateForumsFilter))]
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
