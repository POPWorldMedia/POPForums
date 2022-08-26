using System.Linq;

namespace PopForums.Composers;

public interface IPrivateMessageStateComposer
{
	Task<PrivateMessageState> GetState(PrivateMessage pm);
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

	public async Task<PrivateMessageState> GetState(PrivateMessage pm)
	{
		var state = new PrivateMessageState();
		var user = _userRetrievalShim.GetUser();
		var messages = await _privateMessageService.GetMostRecentPosts(pm.PMID, pm.LastViewDate);
		state.NewestPostID = messages.Any() ? messages.First().PMPostID : 0;
		var bufferMessages = await _privateMessageService.GetPosts(pm.PMID, pm.LastViewDate);
		messages.InsertRange(0, bufferMessages);
		state.PmID = pm.PMID;
		dynamic[] clientMessages = messages.Select(x => new { pmPostID = x.PMPostID, x.UserID, x.Name, PostTime = x.PostTime.ToString("o"), x.FullText }).ToArray();
		state.Messages = clientMessages;
		state.Users = pm.Users;
		return state;
	}
}