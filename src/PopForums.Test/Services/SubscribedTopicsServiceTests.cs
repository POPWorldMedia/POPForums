using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using Xunit;
using PopForums.Configuration;
using PopForums.Email;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Test.Services
{
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

		[Fact]
		public void AddSubTopic()
		{
			var service = GetService();
			var user = new User { UserID = 123 };
			var topic = new Topic { TopicID = 456 };
			service.AddSubscribedTopic(user, topic);
			_mockSubRepo.Verify(s => s.AddSubscribedTopic(user.UserID, topic.TopicID), Times.Once());
		}

		[Fact]
		public void RemoveSubTopic()
		{
			var service = GetService();
			var user = new User { UserID = 123 };
			var topic = new Topic { TopicID = 456 };
			service.RemoveSubscribedTopic(user, topic);
			_mockSubRepo.Verify(s => s.RemoveSubscribedTopic(user.UserID, topic.TopicID), Times.Once());
		}

		[Fact]
		public void TryRemoveSubTopic()
		{
			var service = GetService();
			var user = new User { UserID = 123 };
			var topic = new Topic { TopicID = 456 };
			service.TryRemoveSubscribedTopic(user, topic);
			_mockSubRepo.Verify(s => s.RemoveSubscribedTopic(user.UserID, topic.TopicID), Times.Once());
		}

		[Fact]
		public void TryRemoveSubTopicNullTopic()
		{
			var service = GetService();
			var user = new User { UserID = 123 };
			service.TryRemoveSubscribedTopic(user, null);
			_mockSubRepo.Verify(s => s.RemoveSubscribedTopic(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
		}

		[Fact]
		public void TryRemoveSubTopicNullUser()
		{
			var service = GetService();
			var topic = new Topic { TopicID = 456 };
			service.TryRemoveSubscribedTopic(null, topic);
			_mockSubRepo.Verify(s => s.RemoveSubscribedTopic(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
		}

		[Fact]
		public void MarkSubNullUser()
		{
			var service = GetService();
			var topic = new Topic { TopicID = 456 };
			service.MarkSubscribedTopicViewed(null, topic);
			_mockSubRepo.Verify(s => s.MarkSubscribedTopicViewed(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
		}

		[Fact]
		public void MarkSubNullTopic()
		{
			var service = GetService();
			var user = new User { UserID = 123 };
			service.MarkSubscribedTopicViewed(user, null);
			_mockSubRepo.Verify(s => s.MarkSubscribedTopicViewed(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
		}

		[Fact]
		public void MarkSubNotSub()
		{
			var service = GetService();
			_mockSubRepo.Setup(s => s.IsTopicSubscribed(It.IsAny<int>(), It.IsAny<int>())).Returns(false);
			service.MarkSubscribedTopicViewed(It.IsAny<User>(), It.IsAny<Topic>());
			_mockSubRepo.Verify(s => s.MarkSubscribedTopicViewed(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
		}

		[Fact]
		public void MarkSubIsSub()
		{
			var service = GetService();
			var user = new User { UserID = 123 };
			var topic = new Topic { TopicID = 456 };
			_mockSubRepo.Setup(s => s.IsTopicSubscribed(user.UserID, topic.TopicID)).Returns(true);
			service.MarkSubscribedTopicViewed(user, topic);
			_mockSubRepo.Verify(s => s.MarkSubscribedTopicViewed(user.UserID, topic.TopicID), Times.Once());
		}
		
		[Fact(Skip = "Barrier doesn't block the async sends in core.")] // TODO: make this test work
		public void NotifyCallQueueOnEveryUser()
		{
			var service = GetService();
			var topic = new Topic { TopicID = 123 };
			var list = new List<User> {new User { UserID = 1 }, new User { UserID = 2 } };
			_mockSubRepo.Setup(s => s.GetSubscribedUsersThatHaveViewed(topic.TopicID)).Returns(list);
			var topicLink = "foo";
			Func<User, Topic, string> gen = (u,t) => "x" + u.UserID;
			var barrier = new Barrier(1);
			Action action = () => {
				service.NotifySubscribers(topic, new User { UserID = 45643 }, topicLink, gen);
			    barrier.SignalAndWait();
			};
			action();
			barrier.Dispose();
			_mockSubTopicEmail.Verify(s => s.ComposeAndQueue(topic, list[0], topicLink, "x" + list[0].UserID), Times.Once());
			_mockSubTopicEmail.Verify(s => s.ComposeAndQueue(topic, list[1], topicLink, "x" + list[1].UserID), Times.Once());
		}

        [Fact(Skip = "Barrier doesn't block the async sends in core.")] // TODO: make this test work
		public void NotifyCallQueueOnEveryUserButPostingUser()
		{
			var service = GetService();
			var topic = new Topic { TopicID = 123 };
			var user = new User { UserID = 768 };
			var list = new List<User> { user, new User { UserID = 2 } };
			_mockSubRepo.Setup(s => s.GetSubscribedUsersThatHaveViewed(topic.TopicID)).Returns(list);
			var topicLink = "foo";
			Func<User, Topic, string> gen = (u,t) => "x" + u.UserID;
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

		[Fact]
		public void GetTopicsFromRepo()
		{
			var user = new User { UserID = 123 };
			var service = GetService();
			var settings = new Settings { TopicsPerPage = 20 };
			_mockSettingsManager.Setup(s => s.Current).Returns(settings);
			var list = new List<Topic>();
			_mockSubRepo.Setup(s => s.GetSubscribedTopics(user.UserID, 1, 20)).Returns(list);
			PagerContext pagerContext;
			var result = service.GetTopics(user, 1, out pagerContext);
			Assert.Same(list, result);
		}

		[Fact]
		public void GetTopicsStartRowCalcd()
		{
			var user = new User { UserID = 123 };
			var service = GetService();
			var settings = new Settings { TopicsPerPage = 20 };
			_mockSettingsManager.Setup(s => s.Current).Returns(settings);
			PagerContext pagerContext;
			service.GetTopics(user, 3, out pagerContext);
			_mockSubRepo.Verify(s => s.GetSubscribedTopics(user.UserID, 41, 20), Times.Once());
			Assert.Equal(20, pagerContext.PageSize);
		}
	}
}
