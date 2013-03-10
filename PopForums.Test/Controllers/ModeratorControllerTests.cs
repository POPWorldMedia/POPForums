using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PopForums.Controllers;
using PopForums.Models;
using PopForums.Services;

namespace PopForums.Test.Controllers
{
	[TestFixture]
	public class ModeratorControllerTests
	{
		private ModeratorController GetController()
		{
			_mockTopicService = new Mock<ITopicService>();
			_mockForumService = new Mock<IForumService>();
			_mockPostService = new Mock<IPostService>();
			_mockModLogService = new Mock<IModerationLogService>();
			return new ModeratorController(_mockTopicService.Object, _mockForumService.Object, _mockPostService.Object, _mockModLogService.Object);
		}

		private Mock<ITopicService> _mockTopicService;
		private Mock<IForumService> _mockForumService;
		private Mock<IPostService> _mockPostService;
		private Mock<IModerationLogService> _mockModLogService;

		[Test]
		public void PinThrowOnNoTopic()
		{
			var controller = GetController();
			_mockTopicService.Setup(t => t.Get(It.IsAny<int>())).Returns((Topic)null);
			Assert.Throws<Exception>(() => controller.TogglePin(123));
		}

		[Test]
		public void PinOnUnpinnedTopic()
		{
			var topic = new Topic(5) { IsPinned = false };
			var controller = GetController();
			_mockTopicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			controller.TogglePin(topic.TopicID);
			_mockTopicService.Verify(t => t.PinTopic(topic, It.IsAny<User>()), Times.Exactly(1));
			_mockTopicService.Verify(t => t.UnpinTopic(topic, It.IsAny<User>()), Times.Exactly(0));
		}

		[Test]
		public void UnpinOnPinnedTopic()
		{
			var topic = new Topic(5) { IsPinned = true };
			var controller = GetController();
			_mockTopicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			controller.TogglePin(topic.TopicID);
			_mockTopicService.Verify(t => t.PinTopic(topic, It.IsAny<User>()), Times.Exactly(0));
			_mockTopicService.Verify(t => t.UnpinTopic(topic, It.IsAny<User>()), Times.Exactly(1));
		}

		[Test]
		public void CloseThrowOnNoTopic()
		{
			var controller = GetController();
			_mockTopicService.Setup(t => t.Get(It.IsAny<int>())).Returns((Topic)null);
			Assert.Throws<Exception>(() => controller.ToggleClosed(123));
		}

		[Test]
		public void CloseOnOpenTopic()
		{
			var topic = new Topic(5) { IsClosed = false };
			var controller = GetController();
			_mockTopicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			controller.ToggleClosed(topic.TopicID);
			_mockTopicService.Verify(t => t.CloseTopic(topic, It.IsAny<User>()), Times.Exactly(1));
			_mockTopicService.Verify(t => t.OpenTopic(topic, It.IsAny<User>()), Times.Exactly(0));
		}

		[Test]
		public void OpenOnClosedTopic()
		{
			var topic = new Topic(5) { IsClosed = true };
			var controller = GetController();
			_mockTopicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			controller.ToggleClosed(topic.TopicID);
			_mockTopicService.Verify(t => t.CloseTopic(topic, It.IsAny<User>()), Times.Exactly(0));
			_mockTopicService.Verify(t => t.OpenTopic(topic, It.IsAny<User>()), Times.Exactly(1));
		}

		[Test]
		public void DeleteThrowOnNoTopic()
		{
			var controller = GetController();
			_mockTopicService.Setup(t => t.Get(It.IsAny<int>())).Returns((Topic)null);
			Assert.Throws<Exception>(() => controller.ToggleDeleted(123));
		}

		[Test]
		public void DeleteOnUndeletedTopic()
		{
			var topic = new Topic(5) { IsDeleted = false };
			var controller = GetController();
			_mockTopicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			controller.ToggleDeleted(topic.TopicID);
			_mockTopicService.Verify(t => t.DeleteTopic(topic, It.IsAny<User>()), Times.Exactly(1));
			_mockTopicService.Verify(t => t.UndeleteTopic(topic, It.IsAny<User>()), Times.Exactly(0));
		}

		[Test]
		public void UndeleteOnDeletedTopic()
		{
			var topic = new Topic(5) { IsDeleted = true };
			var controller = GetController();
			_mockTopicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			controller.ToggleDeleted(topic.TopicID);
			_mockTopicService.Verify(t => t.DeleteTopic(topic, It.IsAny<User>()), Times.Exactly(0));
			_mockTopicService.Verify(t => t.UndeleteTopic(topic, It.IsAny<User>()), Times.Exactly(1));
		}

		// TODO: update topic tests

		[Test]
		public void UndeletePostThrowOnNoPost()
		{
			var controller = GetController();
			_mockPostService.Setup(p => p.Get(It.IsAny<int>())).Returns((Post)null);
			Assert.Throws<Exception>(() => controller.UndeletePost(123));
		}

		[Test]
		public void ModLogTopicThrowsWithNoTopic()
		{
			var controller = GetController();
			_mockTopicService.Setup(t => t.Get(It.IsAny<int>())).Returns((Topic) null);
			Assert.Throws<Exception>(() => controller.TopicModerationLog(12));
		}

		[Test]
		public void ModLogTopicReturnsFromRepo()
		{
			var controller = GetController();
			var topic = new Topic(123);
			var list = new List<ModerationLogEntry>();
			_mockTopicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_mockModLogService.Setup(m => m.GetLog(topic, true)).Returns(list);
			var result = controller.TopicModerationLog(topic.TopicID);
			Assert.AreSame(list, result.Model);
		}

		[Test]
		public void ModLogPostThrowsWithNoPost()
		{
			var controller = GetController();
			_mockPostService.Setup(t => t.Get(It.IsAny<int>())).Returns((Post)null);
			Assert.Throws<Exception>(() => controller.PostModerationLog(12));
		}

		[Test]
		public void ModLogPostReturnsFromRepo()
		{
			var controller = GetController();
			var post = new Post(123);
			var list = new List<ModerationLogEntry>();
			_mockPostService.Setup(t => t.Get(post.PostID)).Returns(post);
			_mockModLogService.Setup(m => m.GetLog(post)).Returns(list);
			var result = controller.PostModerationLog(post.PostID);
			Assert.AreSame(list, result.Model);
		}
	}
}
