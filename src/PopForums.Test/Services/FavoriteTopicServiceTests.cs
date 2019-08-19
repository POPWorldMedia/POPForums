using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Test.Services
{
	public class FavoriteTopicServiceTests
	{
		private Mock<IFavoriteTopicsRepository> _mockFaveRepo;
		private Mock<ISettingsManager> _mockSettingsManager;

		private FavoriteTopicService GetService()
		{
			_mockFaveRepo = new Mock<IFavoriteTopicsRepository>();
			_mockSettingsManager = new Mock<ISettingsManager>();
			return new FavoriteTopicService(_mockSettingsManager.Object, _mockFaveRepo.Object);
		}

		[Fact]
		public async Task GetTopicsFromRepo()
		{
			var user = new User { UserID = 123 };
			var service = GetService();
			var settings = new Settings { TopicsPerPage = 20 };
			_mockSettingsManager.Setup(s => s.Current).Returns(settings);
			var list = new List<Topic>();
			_mockFaveRepo.Setup(s => s.GetFavoriteTopics(user.UserID, 1, 20)).ReturnsAsync(list);
			var result = await service.GetTopics(user, 1);
			Assert.Same(list, result.Item1);
		}

		[Fact]
		public async Task AddFaveTopic()
		{
			var service = GetService();
			var user = new User { UserID = 123 };
			var topic = new Topic { TopicID = 456 };
			await service.AddFavoriteTopic(user, topic);
			_mockFaveRepo.Verify(s => s.AddFavoriteTopic(user.UserID, topic.TopicID), Times.Once());
		}

		[Fact]
		public async Task RemoveFaveTopic()
		{
			var service = GetService();
			var user = new User { UserID = 123 };
			var topic = new Topic { TopicID = 456 };
			await service.RemoveFavoriteTopic(user, topic);
			_mockFaveRepo.Verify(s => s.RemoveFavoriteTopic(user.UserID, topic.TopicID), Times.Once());
		}

		[Fact]
		public async Task GetTopicsStartRowCalcd()
		{
			var user = new User { UserID = 123 };
			var service = GetService();
			var settings = new Settings { TopicsPerPage = 20 };
			_mockSettingsManager.Setup(s => s.Current).Returns(settings);
			var result = await service.GetTopics(user, 3);
			_mockFaveRepo.Verify(s => s.GetFavoriteTopics(user.UserID, 41, 20), Times.Once());
			result.Item2.PageSize = settings.TopicsPerPage;
		}
	}
}
