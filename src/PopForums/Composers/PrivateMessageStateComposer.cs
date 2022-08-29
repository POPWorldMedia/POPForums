namespace PopForums.Composers;

public interface IPrivateMessageStateComposer
{
	Task<PrivateMessageState> GetState(PrivateMessage pm);
}

public class PrivateMessageStateComposer : IPrivateMessageStateComposer
{
	private readonly IPrivateMessageService _privateMessageService;

	public PrivateMessageStateComposer(IPrivateMessageService privateMessageService)
	{
		_privateMessageService = privateMessageService;
	}

	public async Task<PrivateMessageState> GetState(PrivateMessage pm)
	{
		var state = new PrivateMessageState();
		var messages = await _privateMessageService.GetMostRecentPosts(pm.PMID, pm.LastViewDate);
		state.NewestPostID = messages.Any() ? messages.First().PMPostID : 0;
		var bufferMessages = await _privateMessageService.GetPosts(pm.PMID, pm.LastViewDate);
		messages.InsertRange(0, bufferMessages);
		state.PmID = pm.PMID;
		var clientMessages = ClientPrivateMessagePost.MapForClient(messages);
		state.Messages = clientMessages;
		state.Users = pm.Users;
		return state;
	}
}