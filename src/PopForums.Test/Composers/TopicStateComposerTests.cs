using PopForums.Composers;

namespace PopForums.Test.Composers;

public class TopicStateComposerTests
{
	protected TopicStateComposer GetComposer()
	{
		_userRetrievalShim = Substitute.For<IUserRetrievalShim>();
		_settingsManager = Substitute.For<ISettingsManager>();
		_subscribedTopicsService = Substitute.For<ISubscribedTopicsService>();
		_favoriteTopicService = Substitute.For<IFavoriteTopicService>();
		return new TopicStateComposer(_userRetrievalShim, _settingsManager, _subscribedTopicsService, _favoriteTopicService);
	}

	private IUserRetrievalShim _userRetrievalShim;
	private ISettingsManager _settingsManager;
	private ISubscribedTopicsService _subscribedTopicsService;
	private IFavoriteTopicService _favoriteTopicService;

	public class GetState : TopicStateComposerTests
	{
		[Fact]
		public async Task MapsCorrectlyWithoutUser()
		{
			var composer = GetComposer();
			_userRetrievalShim.GetUser().Returns((User) null);
			var topic = new Topic {TopicID = 123, AnswerPostID = 789};

			var result = await composer.GetState(topic, 4, 5, 6);

			Assert.Equal(topic.TopicID, result.TopicID);
			Assert.Equal(topic.AnswerPostID, result.AnswerPostID);
			Assert.Equal(4, result.PageIndex);
			Assert.Equal(5, result.PageCount);
			Assert.Equal(6, result.LastVisiblePostID);
			Assert.False(result.IsFavorite);
			Assert.False(result.IsSubscribed);
			Assert.False(result.IsImageEnabled);
		}

		[Fact]
		public async Task MapsCorrectlyWithUser()
		{
			var composer = GetComposer();
			var user = new User {UserID = 111};
			var topic = new Topic { TopicID = 123, AnswerPostID = 789 };
			_userRetrievalShim.GetUser().Returns(user);
			_favoriteTopicService.IsTopicFavorite(user.UserID, topic.TopicID).Returns(Task.FromResult(true));
			_subscribedTopicsService.IsTopicSubscribed(user.UserID, topic.TopicID).Returns(Task.FromResult(true));
			_settingsManager.Current.AllowImages.Returns(true);

			var result = await composer.GetState(topic, 4, 5, 6);

			Assert.Equal(topic.TopicID, result.TopicID);
			Assert.Equal(topic.AnswerPostID, result.AnswerPostID);
			Assert.Equal(4, result.PageIndex);
			Assert.Equal(5, result.PageCount);
			Assert.Equal(6, result.LastVisiblePostID);
			Assert.True(result.IsFavorite);
			Assert.True(result.IsSubscribed);
			Assert.True(result.IsImageEnabled);
			await _favoriteTopicService.Received().IsTopicFavorite(user.UserID, topic.TopicID);
			await _subscribedTopicsService.Received().IsTopicSubscribed(user.UserID, topic.TopicID);
		}
	}
}