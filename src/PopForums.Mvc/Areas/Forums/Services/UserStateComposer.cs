namespace PopForums.Mvc.Areas.Forums.Services;

public interface IUserStateComposer
{
	Task<UserState> GetState();
}

public class UserStateComposer : IUserStateComposer
{
	private readonly IUserRetrievalShim _userRetrievalShim;
	private readonly IPrivateMessageService _privateMessageService;
	private readonly ISettingsManager _settingsManager;
	private readonly INotificationManager _notificationManager;

	public UserStateComposer(IUserRetrievalShim userRetrievalShim, IPrivateMessageService privateMessageService, ISettingsManager settingsManager, INotificationManager notificationManager)
	{
		_userRetrievalShim = userRetrievalShim;
		_privateMessageService = privateMessageService;
		_settingsManager = settingsManager;
		_notificationManager = notificationManager;
	}

	public async Task<UserState> GetState()
	{
		var state = new UserState();
		var user = _userRetrievalShim.GetUser();
		if (user != null)
		{
			var profile = _userRetrievalShim.GetProfile();
			state.IsPlainText = profile.IsPlainText;
			state.NewPmCount = await _privateMessageService.GetUnreadCount(user);
			state.IsImageEnabled = _settingsManager.Current.AllowImages;
			state.NotificationCount = await _notificationManager.GetUnreadNotificationCount(user.UserID);
		}
		return state;
	}
}