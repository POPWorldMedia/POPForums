namespace PopForums.Mvc.Areas.Forums.Services;

public interface IPMStateComposer
{
	Task<PMState> GetState();
}

public class PMStateComposer : IPMStateComposer
{
	private readonly IUserRetrievalShim _userRetrievalShim;
	private readonly IPrivateMessageService _privateMessageService;

	public PMStateComposer(IUserRetrievalShim userRetrievalShim, IPrivateMessageService privateMessageService)
	{
		_userRetrievalShim = userRetrievalShim;
		_privateMessageService = privateMessageService;
	}

	public async Task<PMState> GetState()
	{
		var state = new PMState();
		var user = _userRetrievalShim.GetUser();
		if (user != null)
		{
			state.NewPmCount = await _privateMessageService.GetUnreadCount(user);
		}
		return state;
	}
}