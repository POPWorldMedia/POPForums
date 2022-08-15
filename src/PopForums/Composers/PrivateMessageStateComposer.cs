namespace PopForums.Composers;

public interface IPrivateMessageStateComposer
{
	Task<PrivateMessageState> GetState(int pmID);
}

public class PrivateMessageStateComposer : IPrivateMessageStateComposer
{
	private readonly IUserRetrievalShim _userRetrievalShim;
	private readonly IPrivateMessageService _privateMessageService;

	public PrivateMessageStateComposer(IUserRetrievalShim userRetrievalShim, IPrivateMessageService privateMessageService)
	{
		_userRetrievalShim = userRetrievalShim;
		_privateMessageService = privateMessageService;
	}

	public async Task<PrivateMessageState> GetState(int pmID)
	{
		var state = new PrivateMessageState();
		var user = _userRetrievalShim.GetUser();
		var pm = await _privateMessageService.Get(pmID, user.UserID);
		// TODO: paging
		var messages = await _privateMessageService.GetPosts(pmID);
		state.PmID = pm.PMID;
		dynamic[] clientMessages = messages.Select(x => new { pmPostID = x.PMPostID, x.UserID, x.Name, PostTime = x.PostTime.ToString("o"), x.FullText }).ToArray();
		state.Messages = clientMessages;
		state.Users = pm.Users;
		state.NewestPostID = await _privateMessageService.GetFirstUnreadPostID(pm.PMID, pm.LastViewDate);
		return state;
	}
}