namespace PopForums.Mvc.Areas.Forums.Services;

public interface ITopicStateComposer
{
	Task<TopicState> GetState(int topicID, int pageIndex, int pageCount, int lastVisiblePostID);
}

public class TopicStateComposer : ITopicStateComposer
{
	private readonly IUserRetrievalShim _userRetrievalShim;
	private readonly ISettingsManager _settingsManager;
	private readonly ISubscribedTopicsService _subscribedTopicsService;
	private readonly IFavoriteTopicService _favoriteTopicService;

	public TopicStateComposer(IUserRetrievalShim userRetrievalShim, ISettingsManager settingsManager, ISubscribedTopicsService subscribedTopicsService, IFavoriteTopicService favoriteTopicService)
	{
		_userRetrievalShim = userRetrievalShim;
		_settingsManager = settingsManager;
		_subscribedTopicsService = subscribedTopicsService;
		_favoriteTopicService = favoriteTopicService;
	}

	public async Task<TopicState> GetState(int topicID, int pageIndex, int pageCount, int lastVisiblePostID)
	{
		var topicState = new TopicState { TopicID = topicID, PageIndex = pageIndex, PageCount = pageCount, LastVisiblePostID = lastVisiblePostID };
		var user = _userRetrievalShim.GetUser();
		if (user != null)
		{
			topicState.IsImageEnabled = _settingsManager.Current.AllowImages;
			topicState.IsFavorite = await _favoriteTopicService.IsTopicFavorite(user.UserID, topicID);
			topicState.IsSubscribed = await _subscribedTopicsService.IsTopicSubscribed(user.UserID, topicID);
		}
		return topicState;
	}
}