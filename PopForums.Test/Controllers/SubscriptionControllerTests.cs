using System;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using PopForums.Controllers;
using PopForums.Models;
using PopForums.Services;

namespace PopForums.Test.Controllers
{
	[TestFixture]
	public class SubscriptionControllerTests
	{
		private Mock<ISubscribedTopicsService> _subService;
		private Mock<ITopicService> _topicService;
		private Mock<IUserService> _userService;
		private Mock<ILastReadService> _lastReadService;
		private Mock<IForumService> _forumService;

		public TestableSubController GetController()
		{
			_subService = new Mock<ISubscribedTopicsService>();
			_topicService = new Mock<ITopicService>();
			_userService = new Mock<IUserService>();
			_lastReadService = new Mock<ILastReadService>();
			_forumService = new Mock<IForumService>();
			return new TestableSubController(_subService.Object, _topicService.Object, _userService.Object, _lastReadService.Object, _forumService.Object);
		}

		public class TestableSubController : SubscriptionController
		{
			public TestableSubController(ISubscribedTopicsService subService, ITopicService topicService, IUserService userService, ILastReadService lastReadService, IForumService forumService) : base(subService, topicService, userService, lastReadService, forumService) {}

			public void SetUser(User user)
			{
				HttpContext.User = user;
			}
		}

		[Test]
		public void UserCanSeeSubTopics()
		{
			var controller = GetController();
			var context = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(context.MockContext.Object, new RouteData(), controller);
			var user = new User(2, DateTime.MinValue);
			controller.SetUser(user);
			var result = controller.Topics();
			Assert.IsInstanceOf<PagedTopicContainer>(result.ViewData.Model);
		}

		[Test]
		public void NoUserCantSeeSubTopics()
		{
			var controller = GetController();
			var result = controller.Topics();
			Assert.Null(result.ViewData.Model);
		}

		[Test]
		public void UnsubBadGuidReturnsNulls()
		{
			var controller = GetController();
			var result = controller.Unsubscribe(123, "notaguid");
			Assert.Null(((TopicUnsubscribeContainer)result.ViewData.Model).Topic);
			Assert.Null(((TopicUnsubscribeContainer)result.ViewData.Model).User);
		}

		[Test]
		public void GoodIdAndGuidCallsTryUnsubAndReturnsModel()
		{
			var controller = GetController();
			var topic = new Topic(123);
			var user = new User(456, DateTime.MinValue) { AuthorizationKey = Guid.NewGuid() };
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_userService.Setup(u => u.GetUserByAuhtorizationKey(user.AuthorizationKey)).Returns(user);
			var model = (TopicUnsubscribeContainer) controller.Unsubscribe(topic.TopicID, user.AuthorizationKey.ToString()).ViewData.Model;
			Assert.AreSame(topic, model.Topic);
			Assert.AreSame(user, model.User);
			_subService.Verify(s => s.TryRemoveSubscribedTopic(user, topic));
		}
	}
}
