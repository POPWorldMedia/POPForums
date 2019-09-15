using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopForums.Models;
using PopForums.Mvc.Areas.Forums.Authorization;

namespace PopForums.Mvc.Areas.Forums.Controllers
{
	[Authorize(Policy = PermanentRoles.Admin, AuthenticationSchemes = PopForumsAuthorizationDefaults.AuthenticationScheme)]
	[Area("Forums")]
    public class AdminController : Controller
	{
		public static string Name = "Admin";

		public ViewResult App(string vue = "")
		{
			return View();
		}
	}
}
