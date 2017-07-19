using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace PopForums.Mvc.Areas.Forums.Controllers
{
	[Area("Forums")]
	public class ErrorController : Controller
	{
		public static string Name = "Error";

		public ActionResult Index(int code)
		{
			if (code == 401 || code == 403)
			{
				return View("/Areas/Forums/Views/Shared/Forbidden.cshtml");
			}
			else if (code == 404)
			{
				return View("/Areas/Forums/Views/Shared/NotFound.cshtml");
			}

			// Throw error below
			return View("/Areas/Forums/Views/Shared/UnexpectedError.cshtml");
		}

		public ActionResult ThrowError()
		{
			throw new Exception("Manually generated 500 error");
		}
	}
}
