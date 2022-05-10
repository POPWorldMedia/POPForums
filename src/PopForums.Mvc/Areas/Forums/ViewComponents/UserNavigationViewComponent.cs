#pragma warning disable CS1998
namespace PopForums.Mvc.Areas.Forums.ViewComponents;

public class UserNavigationViewComponent : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync()
	{
		return View();
	}
}