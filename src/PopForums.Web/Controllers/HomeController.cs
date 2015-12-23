using Microsoft.AspNet.Mvc;

namespace PopForums.Web.Controllers
{
    public class HomeController : Controller
    {
	    public IActionResult Index()
	    {
            return View();
        }
	}
}
