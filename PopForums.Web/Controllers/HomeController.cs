using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
