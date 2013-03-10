using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using NUnit.Framework;
using PopForums.Configuration;
using PopForums.Email;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Test.Services
{
	[TestFixture]
	public class SubscribedTopicsServiceTests
	{
		private Mock<ISubscribedTopicsRepository> _mockSubRepo;
		private Mock<ISubscribedTopicEmailComposer> _mockSubTopicEmail;
		private Mock<ISettingsManager> _mockSettingsManager;

		private SubscribedTopicsService GetService()
		{
			_mockSubRepo = new Mock<ISubscribedTopicsRepository>();
			_mockSubTopicEmail = new Mock<ISubscribedTopicEmailComposer>();
			_mockSettingsManager = new Mock<ISettingsManager>();
			return new SubscribedTopicsService(_mockSubRepo.Object, _mockSubTopicEmail.Object, _mockSettingsManager.Object);
		}

		[Test]
		public void AddSubTopic()
		{
			var service = GetService();
			var user = new User(123, DateTime.MaxValue);
			var topic = new Topic(456);
			service.AddSubscribedTopic(user, topic);
			_mockSubRepo.Verify(s => s.AddSubscribedTopic(user.UserID, topic.TopicID), Times.Once());
		}

		[Test]
		public void RemoveSubTopic()
		{
			var service = GetService();
			var user = new User(123, DateTime.MaxValue);
			var topic = new Topic(456);
			service.RemoveSubscribedTopic(user, topic);
			_mockSubRepo.Verify(s => s.RemoveSubscribedTopic(user.UserID, topic.TopicID), Times.Once());
		}

		[Test]
		public void TryRemoveSubTopic()
		{
			var service = GetService();
			var user = new User(123, DateTime.MaxValue);
			var topic = new Topic(456);
			service.TryRemoveSubscribedTopic(user, topic);
			_mockSubRepo.Verify(s => s.RemoveSubscribedTopic(user.UserID, topic.TopicID), Times.Once());
		}

		[Test]
		public void TryRemoveSubTopicNullTopic()
		{
			var service = GetService();
			var user = new User(123, DateTime.MaxValue);
			service.TryRemoveSubscribedTopic(user, null);
			_mockSubRepo.Verify(s => s.RemoveSubscribedTopic(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
		}

		[Test]
		public void TryRemoveSubTopicNullUser()
		{
			var service = GetService();
			var topic = new Topic(456);
			service.TryRemoveSubscribedTopic(null, topic);
			_mockSubRepo.Verify(s => s.RemoveSubscribedTopic(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
		}

		[Test]
		public void MarkSubNullUser()
		{
			var service = GetService();
			var topic = new Topic(456);
			service.MarkSubscribedTopicViewed(null, topic);
			_mockSubRepo.Verify(s => s.MarkSubscribedTopicViewed(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
		}

		[Test]
		public void MarkSubNullTopic()
		{
			var service = GetService();
			var user = new User(123, DateTime.MaxValue);
			service.MarkSubscribedTopicViewed(user, null);
			_mockSubRepo.Verify(s => s.MarkSubscribedTopicViewed(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
		}

		[Test]
		public void MarkSubNotSub()
		{
			var service = GetService();
			_mockSubRepo.Setup(s => s.IsTopicSubscribed(It.IsAny<int>(), It.IsAny<int>())).Returns(false);
			service.MarkSubscribedTopicViewed(It.IsAny<User>(), It.IsAny<Topic>());
			_mockSubRepo.Verify(s => s.MarkSubscribedTopicViewed(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
		}

		[Test]
		public void MarkSubIsSub()
		{
			var service = GetService();
			var user = new User(123, DateTime.MaxValue);
			var topic = new Topic(456);
			_mockSubRepo.Setup(s => s.IsTopicSubscribed(user.UserID, topic.TopicID)).Returns(true);
			service.MarkSubscribedTopicViewed(user, topic);
			_mockSubRepo.Verify(s => s.MarkSubscribedTopicViewed(user.UserID, topic.TopicID), Times.Once());
		}

		[Test]
		public void NotifyCallQueueOnEveryUser()
		{
			var service = GetService();
			var topic = new Topic(123);
			var list = new List<User> {new User(1, DateTime.MinValue), new User(2, DateTime.MinValue)};
			_mockSubRepo.Setup(s => s.GetSubscribedUsersThatHaveViewed(topic.TopicID)).Returns(list);
			var topicLink = "foo";
			Func<User, string> gen = u => "x" + u.UserID;
			var barrier = new Barrier(1);
			Action action = () => {
				service.NotifySubscribers(topic, new User(45643, DateTime.MinValue), topicLink, gen);
			    barrier.SignalAndWait();
			};
			action.Invoke();
			barrier.Dispose();
			_mockSubTopicEmail.Verify(s => s.ComposeAndQueue(topic, It.IsAny<User>(), topicLink, It.IsAny<string>()), Times.Exactly(2));
			_mockSubTopicEmail.Verify(s => s.ComposeAndQueue(topic, list[0], topicLink, "x" + list[0].UserID), Times.Once());
			_mockSubTopicEmail.Verify(s => s.ComposeAndQueue(topic, list[1], topicLink, "x" + list[1].UserID), Times.Once());
		}

		[Test]
		public void NotifyCallQueueOnEveryUserButPostingUser()
		{
			var service = GetService();
			var topic = new Topic(123);
			var user = new User(768, DateTime.MinValue);
			var list = new List<User> { user, new User(2, DateTime.MinValue) };
			_mockSubRepo.Setup(s => s.GetSubscribedUsersThatHaveViewed(topic.TopicID)).Returns(list);
			var topicLink = "foo";
			Func<User, string> gen = u => "x" + u.UserID;
			var barrier = new Barrier(1);
			Action action = () =>
			{
				service.NotifySubscribers(topic, user, topicLink, gen);
				barrier.SignalAndWait();
			};
			action.Invoke();
			barrier.Dispose();
			_mockSubTopicEmail.Verify(s => s.ComposeAndQueue(topic, It.IsAny<User>(), topicLink, It.IsAny<string>()), Times.Exactly(1));
			_mockSubTopicEmail.Verify(s => s.ComposeAndQueue(topic, list[0], topicLink, "x" + list[0].UserID), Times.Exactly(0));
			_mockSubTopicEmail.Verify(s => s.ComposeAndQueue(topic, list[1], topicLink, "x" + list[1].UserID), Times.Exactly(1));
		}

		[Test]
		public void GetTopicsFromRepo()
		{
			var user = new User(123, DateTime.MaxValue);
			var service = GetService();
			var settings = new Settings { TopicsPerPage = 20 };
			_mockSettingsManager.Setup(s => s.Current).Returns(settings);
			var list = new List<Topic>();
			_mockSubRepo.Setup(s => s.GetSubscribedTopics(user.UserID, 1, 20)).Returns(list);
			PagerContext pagerContext;
			var result = service.GetTopics(user, 1, out pagerContext);
			Assert.AreSame(list, result);
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
			_mockSubRepo.Verify(s => s.GetSubscribedTopics(user.UserID, 41, 20), Times.Once());
			Assert.AreEqual(20, pagerContext.PageSize);
		}
	}
}
