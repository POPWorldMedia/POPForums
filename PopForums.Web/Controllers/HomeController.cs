using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using PopForums.Services;

namespace PopForums.Web.Controllers
{
    public class HomeController : Controller
    {
	    private readonly IForumService _forumService;

	    public HomeController(IForumService forumService)
	    {
		    _forumService = forumService;
	    }

	    public IActionResult Index()
	    {
		    var model = _forumService.GetCategorizedForumContainer();
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
