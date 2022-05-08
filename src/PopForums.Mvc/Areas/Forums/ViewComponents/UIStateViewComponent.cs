namespace PopForums.Mvc.Areas.Forums.ViewComponents;

public class UIStateViewComponent : ViewComponent
{
	private readonly IUIStateComposer _uiStateComposer;

	public UIStateViewComponent(IUIStateComposer uiStateComposer)
	{
		_uiStateComposer = uiStateComposer;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var container = await _uiStateComposer.GetState();
		return View(container);
	}
}