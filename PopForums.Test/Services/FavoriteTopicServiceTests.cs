using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
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

		[Test]
		public void GetTopicsFromRepo()
		{
			var user = new User(123, DateTime.MaxValue);
			var service = GetService();
			var settings = new Settings { TopicsPerPage = 20 };
			_mockSettingsManager.Setup(s => s.Current).Returns(settings);
			var list = new List<Topic>();
			_mockFaveRepo.Setup(s => s.GetFavoriteTopics(user.UserID, 1, 20)).Returns(list);
			PagerContext pagerContext;
			var result = service.GetTopics(user, 1, out pagerContext);
			Assert.AreSame(list, result);
		}

		[Test]
		public void AddFaveTopic()
		{
			var service = GetService();
			var user = new User(123, DateTime.MaxValue);
			var topic = new Topic(456);
			service.AddFavoriteTopic(user, topic);
			_mockFaveRepo.Verify(s => s.AddFavoriteTopic(user.UserID, topic.TopicID), Times.Once());
		}

		[Test]
		public void RemoveFaveTopic()
		{
			var service = GetService();
			var user = new User(123, DateTime.MaxValue);
			var topic = new Topic(456);
			service.RemoveFavoriteTopic(user, topic);
			_mockFaveRepo.Verify(s => s.RemoveFavoriteTopic(user.UserID, topic.TopicID), Times.Once());
		}

		[Test]
		public void GetTopicsStartRowCalcd()
		{
			var user = new User(123, DateTime.MaxValue);
			var service = GetService();
			var settings = new Settings { TopicsPerPage = 20 };
			_mockSettingsManager.Setup(s => s.Current).Returns(settings);
			PagerContext pagerContext;
			service.GetTopics(user, 3, out pagerContext);
			_mockFaveRepo.Verify(s => s.GetFavoriteTopics(user.UserID, 41, 20), Times.Once());
			pagerContext.PageSize = settings.TopicsPerPage;
		}
	}
}
