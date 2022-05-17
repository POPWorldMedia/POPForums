namespace PopForums.Mvc.Areas.Forums.ViewComponents;

public class UserStateViewComponent : ViewComponent
{
	private readonly IUserStateComposer _userStateComposer;

	public UserStateViewComponent(IUserStateComposer userStateComposer)
	{
		_userStateComposer = userStateComposer;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var container = await _userStateComposer.GetState();
		return View(container);
	}
}