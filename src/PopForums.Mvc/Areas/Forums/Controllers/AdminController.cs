using PopForums.Mvc.Areas.Forums.Authentication;

namespace PopForums.Mvc.Areas.Forums.Controllers;

[Authorize(Policy = PermanentRoles.Admin, AuthenticationSchemes = PopForumsAuthenticationDefaults.AuthenticationScheme)]
[Area("Forums")]
public class AdminController : Controller
{
	public static string Name = "Admin";

	public ViewResult App(string vue = "")
	{
		return View();
	}
}