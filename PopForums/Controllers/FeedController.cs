using System.Web.Mvc;
using PopForums.Feeds;
using PopForums.Web;

namespace PopForums.Controllers
{
	public class FeedController : Controller
	{
		public FeedController()
		{
			var serviceLocator = PopForumsActivation.ServiceLocator;
			_feedService = serviceLocator.GetInstance<IFeedService>();
		}

		protected internal FeedController(IFeedService feedService)
		{
			_feedService = feedService;
		}

		private readonly IFeedService _feedService;

		public static string Name = "Feed";

		public ViewResult Index()
		{
			var feed = _feedService.GetFeed();
			 return View(feed);
		 }
	}
}