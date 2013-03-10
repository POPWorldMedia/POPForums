using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PopForums.Feeds;
using PopForums.Messaging;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Test.ScoringGame
{
	[TestFixture]
	public class FeedServiceTests
	{
		private FeedService GetService()
		{
			_feedRepo = new Mock<IFeedRepository>();
			_broker = new Mock<IBroker>();
			return new FeedService(_feedRepo.Object, _broker.Object);
		}

		private Mock<IFeedRepository> _feedRepo;
		private Mock<IBroker> _broker;

		[Test]
		public void PublishSavesToRepo()
		{
			var service = GetService();
			var user = new User(123, DateTime.MinValue);
			const string msg = "oiehgfoih";
			const int points = 5352;
			var timeStamp = new DateTime(2000, 1, 1);
			service.PublishToFeed(user, msg, points, timeStamp);
			_feedRepo.Verify(x => x.PublishEvent(user.UserID, msg, points, timeStamp), Times.Once());
		}

		[Test]
		public void PublishDeletesOlderThan50()
		{
			var service = GetService();
			var user = new User(123, DateTime.MinValue);
			var timeStamp = new DateTime(2000, 1, 1);
			var cutOff = new DateTime(1999, 2, 2);
			const int points = 5352;
			_feedRepo.Setup(x => x.GetOldestTime(user.UserID, 50)).Returns(cutOff);
			service.PublishToFeed(user, "whatevs", points, timeStamp);
			_feedRepo.Verify(x => x.DeleteOlderThan(user.UserID, cutOff), Times.Once());
		}

		[Test]
		public void PublishDoesNothingIfUserIsNull()
		{
			var service = GetService();
			service.PublishToFeed(null, String.Empty, 423, new DateTime());
			_feedRepo.Verify(x => x.PublishEvent(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never());
		}

		[Test]
		public void GetFeedGets50ItemsMaxFromRepo()
		{
			var service = GetService();
			var user = new User(123, DateTime.MinValue);
			var list = new List<FeedEvent>();
			_feedRepo.Setup(x => x.GetFeed(user.UserID, 50)).Returns(list);
			var result = service.GetFeed(user);
			Assert.AreSame(result, list);
		}
	}
}