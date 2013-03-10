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
			SetupService = container.Get<ISetupService>();
			UserService = container.Get<IUserService>();
		}

		protected internal SetupController(ISetupService setupService, IUserService userService)
		{
			SetupService = setupService;
			UserService = userService;
		}

		public ISetupService SetupService { get; private set; }
		public IUserService UserService { get; private set; }

		public static string Name = "Setup";

		[PopForumsAuthorizationIgnore]
		public ActionResult Index()
		{
			if (!SetupService.IsConnectionPossible())
				return View("NoConnection");
			if (SetupService.IsDatabaseSetup())
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
			if (SetupService.IsDatabaseSetup())
				return this.Forbidden("Forbidden", null);
			Exception exc;
			var user = SetupService.SetupDatabase(setupVariables, out exc);
			if (exc != null)
				return View("Exception", exc);
			UserService.Login(user, HttpContext);
			return View("Success");
		}
	}
}
