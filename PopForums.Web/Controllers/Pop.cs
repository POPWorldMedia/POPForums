using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace PopForums.Web.Controllers
{
    public class Pop : Controller
    {
        public IActionResult Index()
        {
		    var user = this.User;
            return View();
        }

	    public IActionResult Login()
	    {
		    return View();
	    }

	    [HttpPost]
	    public async Task<IActionResult> Login(string name)
	    {
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, name)
			};

		    var props = new AuthenticationProperties
		    {
			    IsPersistent = true,
			    ExpiresUtc = DateTime.UtcNow.AddYears(1)
		    };

		    var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			await Context.Authentication.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id), props);
			return RedirectToAction("Index");
	    }

		[Authorize()]
	    public IActionResult Test()
	    {
		    return Content(User.Identity.Name);
	    }
    }
}
