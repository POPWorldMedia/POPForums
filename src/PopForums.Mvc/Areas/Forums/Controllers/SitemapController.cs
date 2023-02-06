namespace PopForums.Mvc.Areas.Forums.Controllers;

[Area("Forums")]
[TypeFilter(typeof(PopForumsPrivateForumsFilter))]
public class SitemapController : Controller
{
	private readonly ISitemapService _sitemapService;

	public static string Name = "Sitemap";

	public SitemapController(ISitemapService sitemapService)
	{
		_sitemapService = sitemapService;
	}

	[HttpGet("/Forums/Sitemap.xml")]
	[ResponseCache(Duration = 900)]
	public async Task<IActionResult> Index()
	{
		string SitemapPageLinkGenerator(int page) => Url.Action("Page", Name, new { page }, Request.Scheme);
		var sitemapIndex = await _sitemapService.GenerateIndex(SitemapPageLinkGenerator);
		return Content(sitemapIndex, "text/xml");
	}

	[HttpGet("/Forums/Sitemap.{page}.xml")]
	[ResponseCache(Duration = 900)]
	public async Task<IActionResult> Page(int page)
	{
		string TopicLinkGenerator(string id) => Url.Action("Topic", ForumController.Name, new { id }, Request.Scheme);
		var sitemap = await _sitemapService.GeneratePage(TopicLinkGenerator, page);
		return Content(sitemap, "text/xml");
	}
}