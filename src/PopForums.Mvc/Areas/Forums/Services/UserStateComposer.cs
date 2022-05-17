namespace PopForums.Mvc.Areas.Forums.Services;

public interface IUserStateComposer
{
	Task<UserState> GetState();
}

public class UserStateComposer : IUserStateComposer
{
	private readonly IUserRetrievalShim _userRetrievalShim;
	private readonly IPrivateMessageService _privateMessageService;

	public UserStateComposer(IUserRetrievalShim userRetrievalShim, IPrivateMessageService privateMessageService)
	{
		_userRetrievalShim = userRetrievalShim;
		_privateMessageService = privateMessageService;
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
		}
		return state;
	}
}