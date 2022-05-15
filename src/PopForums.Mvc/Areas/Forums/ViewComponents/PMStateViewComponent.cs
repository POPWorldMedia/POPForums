namespace PopForums.Mvc.Areas.Forums.ViewComponents;

public class PMStateViewComponent : ViewComponent
{
	private readonly IPMStateComposer _pmStateComposer;

	public PMStateViewComponent(IPMStateComposer pmStateComposer)
	{
		_pmStateComposer = pmStateComposer;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var container = await _pmStateComposer.GetState();
		return View(container);
	}
}