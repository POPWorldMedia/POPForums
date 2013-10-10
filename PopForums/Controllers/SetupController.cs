using System;
using System.Web.Mvc;
using Ninject;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Services;
using PopForums.Web;

namespace PopForums.Controllers
{
	public class SetupController : Controller
	{
		public SetupController()
		{
			var container = PopForumsActivation.Kernel;
			_setupService = container.Get<ISetupService>();
			_userService = container.Get<IUserService>();
		}

		protected internal SetupController(ISetupService setupService, IUserService userService)
		{
			_setupService = setupService;
			_userService = userService;
		}

		private readonly ISetupService _setupService;
		private readonly IUserService _userService;

		public static string Name = "Setup";

		[PopForumsAuthorizationIgnore]
		public ActionResult Index()
		{
			if (!_setupService.IsConnectionPossible())
				return View("NoConnection");
			if (_setupService.IsDatabaseSetup())
				return this.Forbidden("Forbidden", null);
			var setupVariables = new SetupVariables
			                     	{
			                     		SmtpPort = 25,
										ServerDaylightSaving = true,
										ServerTimeZone = -5
									};
			ViewData[AdminController.TimeZonesKey] = DataCollections.TimeZones();
			return View(setupVariables);
		}

		[PopForumsAuthorizationIgnore]
		[HttpPost]
		public ActionResult Index(SetupVariables setupVariables)
		{
			if (_setupService.IsDatabaseSetup())
				return this.Forbidden("Forbidden", null);
			Exception exc;
			var user = _setupService.SetupDatabase(setupVariables, out exc);
			if (exc != null)
				return View("Exception", exc);
			_userService.Login(user, HttpContext);
			return View("Success");
		}
	}
}
