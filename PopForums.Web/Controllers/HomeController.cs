using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc;

namespace PopForums.Web.Controllers
{
    public class HomeController : Controller
    {
	    public IActionResult Index()
	    {
            return View();
        }

	    public async Task<IActionResult> Login()
		{
			var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, "Jeff")
				};

			var props = new AuthenticationProperties
			{
				IsPersistent = true,
				ExpiresUtc = DateTime.UtcNow.AddYears(1)
			};

			var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			await HttpContext.Authentication.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id), props);
			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Logout()
		{
			await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction("Index");
		}
	}
}
