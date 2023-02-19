namespace PopForums.Services;

public interface ISetupService
{
	bool IsRuntimeConnectionAndSetupGood();
	bool IsConnectionPossible();
	bool IsDatabaseSetup();
	Task<Tuple<User, Exception>> SetupDatabase(SetupVariables setupVariables);
	Exception SetupDatabaseWithoutSettingsOrUser();
}

public class SetupService : ISetupService
{
	public SetupService(ISetupRepository setupRepository, IUserService userService, ISettingsManager settingsManager, IProfileService profileService)
	{
		_setupRepository = setupRepository;
		_userService = userService;
		_settingsManager = settingsManager;
		_profileService = profileService;
	}

	private readonly ISetupRepository _setupRepository;
	private readonly IUserService _userService;
	private readonly ISettingsManager _settingsManager;
	private readonly IProfileService _profileService;

	private static bool _isConnectionSetupGood;

	private static readonly object _locker = new();

	public bool IsRuntimeConnectionAndSetupGood()
	{
		if (_isConnectionSetupGood)
			return true;
		lock (_locker)
		{
			var canConnect = _setupRepository.IsConnectionPossible();
			var isSetup = _setupRepository.IsDatabaseSetup();
			_isConnectionSetupGood = canConnect && isSetup;
		}
		return _isConnectionSetupGood;
	}

	public bool IsConnectionPossible()
	{
		return _setupRepository.IsConnectionPossible();
	}

	public bool IsDatabaseSetup()
	{
		return _setupRepository.IsDatabaseSetup();
	}

	public Exception SetupDatabaseWithoutSettingsOrUser()
	{
		try
		{
			_setupRepository.SetupDatabase();
		}
		catch (Exception exception)
		{
			return exception;
		}

		return null;
	}

	public async Task<Tuple<User, Exception>> SetupDatabase(SetupVariables setupVariables)
	{
		Exception exception = null;
		try
		{
			_setupRepository.SetupDatabase();
		}
		catch (Exception exc)
		{
			exception = exc;
			return Tuple.Create<User, Exception>(null, exception);
		}

		var settings = _settingsManager.Current;
		settings.ForumTitle = setupVariables.ForumTitle;
		settings.SmtpServer = setupVariables.SmtpServer;
		settings.SmtpPort = setupVariables.SmtpPort;
		settings.MailerAddress = setupVariables.MailerAddress;
		settings.UseSslSmtp = setupVariables.UseSslSmtp;
		settings.UseEsmtp = setupVariables.UseEsmtp;
		settings.SmtpUser = setupVariables.SmtpUser;
		settings.SmtpPassword = setupVariables.SmtpPassword;
		_settingsManager.SaveCurrent();

		var user = await _userService.CreateUser(setupVariables.Name, setupVariables.Email, setupVariables.Password, true, "");
		user.Roles = new List<string> {PermanentRoles.Admin, PermanentRoles.Moderator};
		var profile = new Profile { UserID = user.UserID, IsTos = true, IsSubscribed = true, ShowDetails = true, IsAutoFollowOnReply = true };
		await _profileService.Create(profile);
		var edit = new UserEdit(user, profile);
		await _userService.EditUser(user, edit, false, false, null, null, "", user);
		//PopForumsActivation.StartServicesIfRunningInstance();
		return Tuple.Create(user, exception);
	}
}