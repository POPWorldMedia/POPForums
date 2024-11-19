using PopForums.Mvc.Areas.Forums.Authentication;

namespace PopForums.Mvc.Areas.Forums.Controllers;

[Area("Forums")]
public class SetupController : Controller
{
	public SetupController(ISetupService setupService, IConfig config)
	{
		_setupService = setupService;
		_config = config;
	}

	private readonly ISetupService _setupService;
	private readonly IConfig _config;

	public static string Name = "Setup";

	[PopForumsAuthenticationIgnore]
	public ActionResult Index()
	{
		if (_config.IsOAuthOnly)
			return RedirectToAction(nameof(OAuthOnlySetup));
		if (!_setupService.IsConnectionPossible())
			return View("NoConnection");
		if (_setupService.IsDatabaseSetup())
			return StatusCode(403);
		var setupVariables = new SetupVariables
		{
			SmtpPort = 25
		};
		return View(setupVariables);
	}

	public IActionResult OAuthOnlySetup()
	{
		if (!_setupService.IsConnectionPossible())
			return View("NoConnection");
		if (_setupService.IsDatabaseSetup())
			return StatusCode(403);
		var exception = _setupService.SetupDatabaseWithoutSettingsOrUser();
		if (exception != null)
			return View("Exception");
		return View("Success");
	}

	[PopForumsAuthenticationIgnore]
	[HttpPost]
	public async Task<ActionResult> Index(SetupVariables setupVariables)
	{
		if (_setupService.IsDatabaseSetup())
			return StatusCode(403);
		var result = await _setupService.SetupDatabase(setupVariables);
		if (result.Item2 != null)
			return View("Exception", result.Item2);
		// can't login here because all of the normal app startup was skipped
		//await AuthorizationController.PerformSignInAsync(true, user, HttpContext);
		return View("Success");
	}
}