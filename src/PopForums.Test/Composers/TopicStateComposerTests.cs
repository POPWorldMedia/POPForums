using PopForums.Composers;

namespace PopForums.Test.Composers;

public class TopicStateComposerTests
{
	protected TopicStateComposer GetComposer()
	{
		_userRetrievalShim = new Mock<IUserRetrievalShim>();
		_settingsManager = new Mock<ISettingsManager>();
		_subscribedTopicsService = new Mock<ISubscribedTopicsService>();
		_favoriteTopicService = new Mock<IFavoriteTopicService>();
		return new TopicStateComposer(_userRetrievalShim.Object, _settingsManager.Object, _subscribedTopicsService.Object, _favoriteTopicService.Object);
	}

	private Mock<IUserRetrievalShim> _userRetrievalShim;
	private Mock<ISettingsManager> _settingsManager;
	private Mock<ISubscribedTopicsService> _subscribedTopicsService;
	private Mock<IFavoriteTopicService> _favoriteTopicService;

	public class GetState : TopicStateComposerTests
	{
		[Fact]
		public async Task MapsCorrectlyWithoutUser()
		{
			var composer = GetComposer();
			_userRetrievalShim.Setup(x => x.GetUser()).Returns((User) null);
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
			_userRetrievalShim.Setup(x => x.GetUser()).Returns(user);
			_favoriteTopicService.Setup(x => x.IsTopicFavorite(user.UserID, topic.TopicID)).ReturnsAsync(true);
			_subscribedTopicsService.Setup(x => x.IsTopicSubscribed(user.UserID, topic.TopicID)).ReturnsAsync(true);
			_settingsManager.Setup(x => x.Current.AllowImages).Returns(true);

			var result = await composer.GetState(topic, 4, 5, 6);

			Assert.Equal(topic.TopicID, result.TopicID);
			Assert.Equal(topic.AnswerPostID, result.AnswerPostID);
			Assert.Equal(4, result.PageIndex);
			Assert.Equal(5, result.PageCount);
			Assert.Equal(6, result.LastVisiblePostID);
			Assert.True(result.IsFavorite);
			Assert.True(result.IsSubscribed);
			Assert.True(result.IsImageEnabled);
			_favoriteTopicService.Verify(x => x.IsTopicFavorite(user.UserID, topic.TopicID), Times.Once());
			_subscribedTopicsService.Verify(x => x.IsTopicSubscribed(user.UserID, topic.TopicID), Times.Once);
		}
	}
}