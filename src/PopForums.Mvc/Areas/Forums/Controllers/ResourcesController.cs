using PopForums.Composers;

namespace PopForums.Mvc.Areas.Forums.Controllers;

[Area("Forums")]
public class ResourcesController : Controller
{
	private readonly IResourceComposer _resourceComposer;

	public ResourcesController(IResourceComposer resourceComposer)
	{
		_resourceComposer = resourceComposer;
	}

	[ResponseCache(Duration = 600, Location = ResponseCacheLocation.Client)]
	public JsonResult Index()
	{
		var result = _resourceComposer.GetForCurrentThread();
		return Json(result);
	}
}