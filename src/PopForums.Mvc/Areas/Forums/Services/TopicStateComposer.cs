namespace PopForums.Mvc.Areas.Forums.Services;

public interface ITopicStateComposer
{
	TopicState GetState(int topicID);
}

public class TopicStateComposer : ITopicStateComposer
{
	private readonly IUserRetrievalShim _userRetrievalShim;
	private readonly ISettingsManager _settingsManager;

	public TopicStateComposer(IUserRetrievalShim userRetrievalShim, ISettingsManager settingsManager)
	{
		_userRetrievalShim = userRetrievalShim;
		_settingsManager = settingsManager;
	}

	public TopicState GetState(int topicID)
	{
		var topicState = new TopicState { TopicID = topicID };
		var user = _userRetrievalShim.GetUser();
		if (user != null)
		{
			var profile = _userRetrievalShim.GetProfile();
			topicState.IsPlainText = profile.IsPlainText;
			topicState.IsImageEnabled = _settingsManager.Current.AllowImages;
		}
		return topicState;
	}
}