using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using PopForums.Configuration;
using PopForums.Controllers;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Services;
using PopForums.Web;

namespace PopForums.Test.Controllers
{
	[TestFixture]
	public class ForumControllerTests
	{
		private TestableForumController GetForumController()
		{
			_userService = new Mock<IUserService>();
			_settingsManager = new Mock<ISettingsManager>();
			_settingsManager.Setup(s => s.Current).Returns(new Settings());
			_forumService = new Mock<IForumService>();
			_topicService = new Mock<ITopicService>();
			_postService = new Mock<IPostService>();
			_topicViewCountService = new Mock<ITopicViewCountService>();
			_subService = new Mock<ISubscribedTopicsService>();
			_lastReadService = new Mock<ILastReadService>();
			_faveTopicService = new Mock<IFavoriteTopicService>();
			_profileService = new Mock<IProfileService>();
			_mobileDetection = new Mock<IMobileDetectionWrapper>();
			var controller = new TestableForumController(_settingsManager.Object, _forumService.Object, _topicService.Object, _postService.Object, _topicViewCountService.Object, _subService.Object, _lastReadService.Object, _faveTopicService.Object, _profileService.Object, _mobileDetection.Object);
			return controller;
		}

		private class TestableForumController : ForumController
		{
			public TestableForumController(ISettingsManager settingsManager, IForumService forumService, ITopicService topicService, IPostService postService, ITopicViewCountService topicViewService, ISubscribedTopicsService subService, ILastReadService lastReadService, IFavoriteTopicService faveTopicService, IProfileService profileService, IMobileDetectionWrapper mobileDetection) : base(settingsManager, forumService, topicService, postService, topicViewService, subService, lastReadService, faveTopicService, profileService, mobileDetection) { }

			public void SetUser(User user)
			{
				HttpContext.User = user;
			}
		}

		private Mock<IUserService> _userService;
		private Mock<ISettingsManager> _settingsManager;
		private Mock<IForumService> _forumService;
		private Mock<ITopicService> _topicService;
		private Mock<IPostService> _postService;
		private Mock<ITopicViewCountService> _topicViewCountService;
		private Mock<ISubscribedTopicsService> _subService;
		private Mock<ILastReadService> _lastReadService;
		private Mock<IFavoriteTopicService> _faveTopicService;
		private Mock<IProfileService> _profileService;
		private Mock<IMobileDetectionWrapper> _mobileDetection;

		[Test]
		public void BadForumUrlName404()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			_forumService.Setup(f => f.Get(It.IsAny<string>())).Returns((Forum)null);
			var result = (ViewResult)controller.Index("blah");
			Assert.IsInstanceOf<ViewResult>(result);
			Assert.AreEqual("NotFound", result.ViewName);
			contextHelper.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.NotFound);
		}

		[Test]
		public void ForumForbiddenFromView()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var forum = new Forum(1);
			var user = It.IsAny<User>();
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns(user);
			_forumService.Setup(f => f.Get(It.IsAny<string>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user)).Returns(new ForumPermissionContext {UserCanView = false});
			var result = (ViewResult)controller.Index("blah");
			Assert.IsInstanceOf<ViewResult>(result);
			Assert.AreEqual("Forbidden", result.ViewName);
			contextHelper.MockResponse.VerifySet(r => r.StatusCode = (int) HttpStatusCode.Forbidden);
		}

		[Test]
		public void IndexReturnsTopics()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var forum = new Forum(1);
			var topics = new List<Topic>();
			var permissionContext = new ForumPermissionContext {UserCanView = true};
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns((User)null);
			_forumService.Setup(f => f.Get(It.IsAny<string>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, null)).Returns(permissionContext);
			var pagerContext = new PagerContext();
			_topicService.Setup(t => t.GetTopics(It.IsAny<Forum>(), It.IsAny<bool>(), It.IsAny<int>(), out pagerContext)).Returns(topics);
			var result = (ViewResult)controller.Index("blah");
			Assert.IsInstanceOf<ViewResult>(result);
			Assert.IsInstanceOf<ForumTopicContainer>(result.ViewData.Model);
			var model = (ForumTopicContainer) result.ViewData.Model;
			Assert.AreSame(forum, model.Forum);
			Assert.AreSame(topics, model.Topics);
			Assert.AreSame(permissionContext, model.PermissionContext);
			Assert.AreSame(pagerContext, model.PagerContext);
		}

		[Test]
		public void NewTopicPartialNoUser()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.PostTopic(1);
			Assert.IsInstanceOf<ContentResult>(result);
		}

		[Test]
		public void NewTopicPartialUserCantView()
		{
			var controller = GetForumController();
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext {UserCanView = false});
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.PostTopic(1);
			Assert.IsInstanceOf<ContentResult>(result);
		}

		[Test]
		public void NewTopicPartialUserCantPost()
		{
			var controller = GetForumController();
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext { UserCanPost = false });
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.PostTopic(1);
			Assert.IsInstanceOf<ContentResult>(result);
		}

		[Test]
		public void NewTopicPartialUserCanPostCanView()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(new Forum(1));
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
			_profileService.Setup(p => p.GetProfile(user)).Returns(new Profile {Signature = "blah", IsPlainText = true});
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var result = controller.PostTopic(1);
			Assert.IsInstanceOf<ViewResult>(result);
			var viewResult = (ViewResult) result;
			Assert.True(((NewPost)viewResult.Model).IncludeSignature);
			Assert.True(((NewPost)viewResult.Model).IsPlainText);
		}

		[Test]
		public void NewTopicIncludeSigFalseWithNoSig()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(new Forum(1));
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
			_profileService.Setup(p => p.GetProfile(user)).Returns(new Profile { Signature = "" });
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var result = controller.PostTopic(1);
			var viewResult = (ViewResult)result;
			Assert.False(((NewPost)viewResult.Model).IncludeSignature);
		}

		[Test]
		public void NewTopicPostNoUser()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.PostTopic(new NewPost());
			Assert.IsInstanceOf<JsonResult>(result);
			var data = (BasicJsonMessage) result.Data;
			Assert.IsFalse(data.Result);
		}

		[Test]
		public void NewTopicPostUserCantView()
		{
			var controller = GetForumController();
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext { UserCanView = false });
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.PostTopic(new NewPost());
			Assert.IsInstanceOf<JsonResult>(result);
			var data = (BasicJsonMessage)result.Data;
			Assert.IsFalse(data.Result);
		}

		[Test]
		public void NewTopicPostUserCantPost()
		{
			var controller = GetForumController();
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext { UserCanPost = false });
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.PostTopic(new NewPost());
			Assert.IsInstanceOf<JsonResult>(result);
			var data = (BasicJsonMessage)result.Data;
			Assert.IsFalse(data.Result);
		}

		[Test]
		public void NewTopicPostIsDupeOrTimeLimit()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			var forum = new Forum(1);
			var topic = new Topic(2);
			var ip = "127.0.0.1";
			var permissionContext = new ForumPermissionContext { UserCanPost = true, UserCanView = true };
			var newPost = new NewPost { Title = "blah", FullText = "boo", IncludeSignature = true, ItemID = 2 };
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user)).Returns(permissionContext);
			_forumService.Setup(t => t.PostNewTopic(forum, user, permissionContext, newPost, ip, It.IsAny<string>(), It.IsAny<Func<Topic, string>>())).Returns(topic);
			_postService.Setup(p => p.IsNewPostDupeOrInTimeLimit(newPost, user)).Returns(true);
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(r => r.UserHostAddress).Returns(ip);
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var result = controller.PostTopic(newPost);
			Assert.IsInstanceOf<JsonResult>(result);
			var data = (BasicJsonMessage)result.Data;
			Assert.IsFalse(data.Result);
			_postService.Verify(p => p.IsNewPostDupeOrInTimeLimit(newPost, user), Times.Once());
		}

		[Test]
		public void NewTopicPostSetViewTopicCalled()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			var forum = new Forum(1);
			var topic = new Topic(2);
			var ip = "127.0.0.1";
			var permissionContext = new ForumPermissionContext { UserCanPost = true, UserCanView = true };
			var newPost = new NewPost { Title = "blah", FullText = "boo", IncludeSignature = true, ItemID = 2 };
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user)).Returns(permissionContext);
			_forumService.Setup(t => t.PostNewTopic(forum, user, permissionContext, newPost, ip, It.IsAny<string>(), It.IsAny<Func<Topic, string>>())).Returns(topic);
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(r => r.UserHostAddress).Returns(ip);
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			controller.PostTopic(newPost);
			_topicViewCountService.Verify(t => t.SetViewedTopic(topic, contextHelper.MockContext.Object), Times.Once());
		}

        [Test]
        public void NewTopicPostNullFullText()
        {
            var controller = GetForumController();
            var user = Models.UserTest.GetTestUser();
            var forum = new Forum(1);
            var topic = new Topic(2);
            var ip = "127.0.0.1";
            var permissionContext = new ForumPermissionContext { UserCanPost = true, UserCanView = true };
            var newPost = new NewPost { Title = "blah", FullText = null, IncludeSignature = true, ItemID = 2 };
            _forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
            _forumService.Setup(f => f.GetPermissionContext(forum, user)).Returns(permissionContext);
            var contextHelper = new HttpContextHelper();
            contextHelper.MockRequest.Setup(r => r.UserHostAddress).Returns(ip);
            controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
            controller.SetUser(user);
            var result = controller.PostTopic(newPost);
            Assert.IsInstanceOf<JsonResult>(result);
            var data = (BasicJsonMessage)result.Data;
            Assert.IsFalse(data.Result);
            _topicViewCountService.Verify(t => t.SetViewedTopic(topic, contextHelper.MockContext.Object), Times.Never());
        }

		[Test]
		public void NewTopicPostUserCanPostCanView()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			var forum = new Forum(1);
			var topic = new Topic(2);
			var ip = "127.0.0.1";
			var permissionContext = new ForumPermissionContext {UserCanPost = true, UserCanView = true};
			var newPost = new NewPost {Title = "blah", FullText = "boo", IncludeSignature = true, ItemID = 2};
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user)).Returns(permissionContext);
			_forumService.Setup(t => t.PostNewTopic(forum, user, permissionContext, newPost, ip, It.IsAny<string>(), It.IsAny<Func<Topic, string>>())).Returns(topic);
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(r => r.UserHostAddress).Returns(ip);
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var result = controller.PostTopic(newPost);
			_forumService.Verify(t => t.PostNewTopic(forum, user, permissionContext, newPost, ip, It.IsAny<string>(), It.IsAny<Func<Topic, string>>()), Times.Once());
			Assert.IsInstanceOf<JsonResult>(result);
			var data = (BasicJsonMessage)result.Data;
			Assert.IsTrue(data.Result);
			// TODO: test for the redirect URL
		}

		[Test]
		public void BadTopicUrlName404()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			_forumService.Setup(f => f.Get(It.IsAny<string>())).Returns((Forum)null);
			_topicService.Setup(t => t.Get(It.IsAny<string>())).Returns((Topic)null);
			var result = controller.Topic("blah");
			Assert.IsInstanceOf<ViewResult>(result);
			Assert.AreEqual("NotFound", result.ViewName);
			contextHelper.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.NotFound);
		}

		[Test]
		public void TopicHadBadForumAssociationAndThrows()
		{
			var controller = GetForumController();
			_topicService.Setup(t => t.Get(It.IsAny<string>())).Returns(new Topic(1));
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns((Forum)null);
			Assert.Throws<Exception>(() => controller.Topic("blah"));
		}

		[Test]
		public void TopicForbiddenFromView()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var forum = new Forum(1);
			var user = It.IsAny<User>();
			var topic = new Topic(1);
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns(user);
			_topicService.Setup(t => t.Get(It.IsAny<string>())).Returns(topic);
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user, topic)).Returns(new ForumPermissionContext { UserCanView = false });
			var result = controller.Topic("blah");
			Assert.IsInstanceOf<ViewResult>(result);
			Assert.AreEqual("Forbidden", result.ViewName);
			contextHelper.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.Forbidden);
		}

		[Test]
		public void TopicReturnsTopicContainer()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var forum = new Forum(1);
			var topic = new Topic(2);
			var posts = new List<Post> { new Post(456) };
			var permissionContext = new ForumPermissionContext { UserCanView = true, UserCanModerate = false };
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns((User)null);
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, null, topic)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(It.IsAny<string>())).Returns(topic);
			var pagerContext = new PagerContext();
			_postService.Setup(p => p.GetPosts(topic, permissionContext.UserCanModerate, 1, out pagerContext)).Returns(posts);
			var result = controller.Topic("blah");
			Assert.IsInstanceOf<ViewResult>(result);
			Assert.IsInstanceOf<TopicContainer>(result.ViewData.Model);
			var model = (TopicContainer)result.ViewData.Model;
			Assert.AreSame(forum, model.Forum);
			Assert.AreSame(topic, model.Topic);
			Assert.AreSame(posts, model.Posts);
			Assert.AreSame(permissionContext, model.PermissionContext);
			Assert.AreSame(pagerContext, model.PagerContext);
			Assert.Null(result.ViewBag.CategorizedForums);
		}

		[Test]
		public void TopicCallsMarkTopicReadNoAdapter()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var forum = new Forum(1);
			var topic = new Topic(2);
			var posts = new List<Post>();
			var permissionContext = new ForumPermissionContext { UserCanView = true, UserCanModerate = false };
			var user = Models.UserTest.GetTestUser();
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns(user);
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user, topic)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(It.IsAny<string>())).Returns(topic);
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var pagerContext = new PagerContext();
			_postService.Setup(p => p.GetPosts(topic, permissionContext.UserCanModerate, 1, out pagerContext)).Returns(posts);
			controller.Topic("blah");
			_lastReadService.Verify(x => x.MarkTopicRead(user, topic), Times.Once());
		}

		[Test]
		public void TopicCallsMarkTopicReadWithAdapter()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var forum = new Forum(1) { ForumAdapterName = "PopForums.Test.Controllers.TestAdapter, PopForums.Test" };
			var topic = new Topic(2);
			var posts = new List<Post>();
			var permissionContext = new ForumPermissionContext { UserCanView = true, UserCanModerate = false };
			var user = Models.UserTest.GetTestUser();
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns(user);
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user, topic)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(It.IsAny<string>())).Returns(topic);
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var pagerContext = new PagerContext();
			_postService.Setup(p => p.GetPosts(topic, permissionContext.UserCanModerate, 1, out pagerContext)).Returns(posts);
			controller.Topic("blah");
			_lastReadService.Verify(x => x.MarkTopicRead(user, topic), Times.Once());
		}

		[Test]
		public void TopicNotCallsMarkTopicReadWithAdapterFalse()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var forum = new Forum(1) { ForumAdapterName = "PopForums.Test.Controllers.TestAdapterNoRead, PopForums.Test" };
			var topic = new Topic(2);
			var posts = new List<Post>();
			var permissionContext = new ForumPermissionContext { UserCanView = true, UserCanModerate = false };
			var user = Models.UserTest.GetTestUser();
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns(user);
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user, topic)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(It.IsAny<string>())).Returns(topic);
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var pagerContext = new PagerContext();
			_postService.Setup(p => p.GetPosts(topic, permissionContext.UserCanModerate, 1, out pagerContext)).Returns(posts);
			controller.Topic("blah");
			_lastReadService.Verify(x => x.MarkTopicRead(user, topic), Times.Never());
		}

		[Test]
		public void TopicCallsForSigs()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var forum = new Forum(1);
			var topic = new Topic(2);
			var posts = new List<Post> { new Post(456) };
			var sigs = new Dictionary<int, string>();
			var permissionContext = new ForumPermissionContext { UserCanView = true, UserCanModerate = false };
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns((User)null);
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, null, topic)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(It.IsAny<string>())).Returns(topic);
			PagerContext pagerContext;
			_postService.Setup(p => p.GetPosts(topic, permissionContext.UserCanModerate, 1, out pagerContext)).Returns(posts);
			_profileService.Setup(p => p.GetSignatures(posts)).Returns(sigs);
			var result = controller.Topic("blah");
			var model = (TopicContainer)result.ViewData.Model;
			_profileService.Verify(p => p.GetSignatures(posts), Times.Exactly(1));
			Assert.AreSame(sigs, model.Signatures);
		}

		[Test]
		public void TopicCallsForAvatars()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var forum = new Forum(1);
			var topic = new Topic(2);
			var posts = new List<Post> { new Post(456) };
			var avatars = new Dictionary<int, int>();
			var permissionContext = new ForumPermissionContext { UserCanView = true, UserCanModerate = false };
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns((User)null);
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, null, topic)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(It.IsAny<string>())).Returns(topic);
			PagerContext pagerContext;
			_postService.Setup(p => p.GetPosts(topic, permissionContext.UserCanModerate, 1, out pagerContext)).Returns(posts);
			_profileService.Setup(p => p.GetAvatars(posts)).Returns(avatars);
			var result = controller.Topic("blah");
			var model = (TopicContainer)result.ViewData.Model;
			_profileService.Verify(p => p.GetAvatars(posts), Times.Exactly(1));
			Assert.AreSame(avatars, model.Avatars);
		}

		[Test]
		public void TopicCallsForVotedIDs()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var forum = new Forum(1);
			var topic = new Topic(2);
			var posts = new List<Post> { new Post(456) };
			var postIDs = new List<int>();
			var user = new User(1, DateTime.MinValue) { Roles = new List<string>() };
			var permissionContext = new ForumPermissionContext { UserCanView = true, UserCanModerate = false };
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns(user);
			controller.SetUser(user);
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user, topic)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(It.IsAny<string>())).Returns(topic);
			PagerContext pagerContext;
			_postService.Setup(p => p.GetPosts(topic, permissionContext.UserCanModerate, 1, out pagerContext)).Returns(posts);
			_postService.Setup(x => x.GetVotedPostIDs(user, posts)).Returns(postIDs);
			var result = (TopicContainer)controller.Topic("blah").Model;
			_postService.Verify(x => x.GetVotedPostIDs(user, posts), Times.Once());
			Assert.AreSame(postIDs, result.VotedPostIDs);
		}

		[Test]
		public void TopicPartialCallsForVotedIDs()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var forum = new Forum(1);
			var topic = new Topic(2);
			var posts = new List<Post>();
			var postIDs = new List<int>();
			var user = new User(1, DateTime.MinValue) { Roles = new List<string>() };
			var permissionContext = new ForumPermissionContext { UserCanView = true, UserCanModerate = false };
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns(user);
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user, topic)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			var pagerContext = new PagerContext();
			controller.SetUser(user);
			_postService.Setup(p => p.GetPosts(topic, 1, permissionContext.UserCanModerate, out pagerContext)).Returns(posts);
			_postService.Setup(x => x.GetVotedPostIDs(user, posts)).Returns(postIDs);
			var result = (TopicContainer)((ViewResult)controller.TopicPartial(topic.TopicID,1,1)).Model;
			_postService.Verify(x => x.GetVotedPostIDs(user, posts), Times.Once());
			Assert.AreSame(postIDs, result.VotedPostIDs);
		}

		[Test]
		public void TopicPagedCallsForVotedIDs()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var forum = new Forum(1);
			var topic = new Topic(2);
			var posts = new List<Post> { new Post(456) };
			var postIDs = new List<int>();
			var user = new User(1, DateTime.MinValue) { Roles = new List<string>() };
			var permissionContext = new ForumPermissionContext { UserCanView = true, UserCanModerate = false };
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns(user);
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user, topic)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			var pagerContext = new PagerContext();
			controller.SetUser(user);
			_postService.Setup(p => p.GetPosts(topic, permissionContext.UserCanModerate, 1, out pagerContext)).Returns(posts);
			_postService.Setup(x => x.GetVotedPostIDs(user, posts)).Returns(postIDs);
			var result = (TopicContainer)((ViewResult)controller.TopicPage(topic.TopicID, 1, 1, 1)).Model;
			_postService.Verify(x => x.GetVotedPostIDs(user, posts), Times.Once());
			Assert.AreSame(postIDs, result.VotedPostIDs);
		}

		[Test]
		public void TopicReturnsCategorizedForumsForModerator()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var forum = new Forum(1);
			var topic = new Topic(2);
			var posts = new List<Post> { new Post(456) };
			var user = new User(1, DateTime.MinValue) {Roles = new List<string> {PermanentRoles.Moderator}};
			var permissionContext = new ForumPermissionContext { UserCanView = true, UserCanModerate = true };
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns(user);
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user, topic)).Returns(permissionContext);
			_forumService.Setup(f => f.GetCategorizedForumContainer()).Returns(new CategorizedForumContainer(new List<Category>(), new List<Forum>()));
			_topicService.Setup(t => t.Get(It.IsAny<string>())).Returns(topic);
			PagerContext pagerContext;
			_postService.Setup(p => p.GetPosts(topic, permissionContext.UserCanModerate, 1, out pagerContext)).Returns(posts);
			controller.SetUser(user);
			var result = controller.Topic("blah");
			Assert.NotNull(result.ViewBag.CategorizedForums);
			Assert.IsInstanceOf<CategorizedForumContainer>(result.ViewBag.CategorizedForums);
		}

		[Test]
		public void TopicReturnsNotFoundOnEmptyPage()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var forum = new Forum(1);
			var topic = new Topic(2);
			var posts = new List<Post>();
			var user = new User(1, DateTime.MinValue) { Roles = new List<string>() };
			var permissionContext = new ForumPermissionContext { UserCanView = true, UserCanModerate = false };
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns(user);
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user, topic)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(It.IsAny<string>())).Returns(topic);
			PagerContext pagerContext;
			_postService.Setup(p => p.GetPosts(topic, false, 2, out pagerContext)).Returns(posts);
			controller.SetUser(user);
			var result = controller.Topic("blah", 2);
			Assert.AreEqual("NotFound", result.ViewName);
		}

		[Test]
		public void TopicPageReturnsNotFoundOnEmptyPage()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var forum = new Forum(1);
			var topic = new Topic(2);
			var posts = new List<Post>();
			var user = new User(1, DateTime.MinValue) { Roles = new List<string>() };
			var permissionContext = new ForumPermissionContext { UserCanView = true, UserCanModerate = false };
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns(user);
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user, topic)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(It.IsAny<string>())).Returns(topic);
			PagerContext pagerContext;
			_postService.Setup(p => p.GetPosts(topic, false, 2, out pagerContext)).Returns(posts);
			controller.SetUser(user);
			var result = (ViewResult)controller.TopicPage(topic.TopicID, 2, It.IsAny<int>(), It.IsAny<int>());
			Assert.AreEqual("NotFound", result.ViewName);
		}

		[Test]
		public void IsSubscribedWiredUp()
		{
			
		}

		[Test]
		public void ViewCountCalled()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var forum = new Forum(1);
			var topic = new Topic(2);
			var posts = new List<Post> { new Post(456) };
			var permissionContext = new ForumPermissionContext { UserCanView = true, UserCanModerate = false };
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns((User)null);
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, null, topic)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(It.IsAny<string>())).Returns(topic);
			PagerContext pagerContext;
			_postService.Setup(p => p.GetPosts(It.IsAny<Topic>(), It.IsAny<bool>(), It.IsAny<int>(), out pagerContext)).Returns(posts);
			controller.Topic("blah");
			_topicViewCountService.Verify(v => v.ProcessView(topic, contextHelper.MockContext.Object), Times.Once());
		}

		[Test]
		public void ReplyPartialNoUser()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.PostReply(1);
			Assert.IsInstanceOf<ContentResult>(result);
		}

		[Test]
		public void ReplyPartialUserCantView()
		{
			var controller = GetForumController();
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext { UserCanView = false });
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.PostReply(1);
			Assert.IsInstanceOf<ContentResult>(result);
		}

		[Test]
		public void ReplyPartialUserCantPost()
		{
			var controller = GetForumController();
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext { UserCanPost = false });
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.PostReply(1);
			Assert.IsInstanceOf<ContentResult>(result);
		}

		[Test]
		public void ReplyPartialUserCanPostCanView()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(new Forum(1));
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>(), It.IsAny<Topic>())).Returns(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
			_topicService.Setup(t => t.Get(1)).Returns(new Topic(2) {Title = "blah"});
			_profileService.Setup(p => p.GetProfile(user)).Returns(new Profile {Signature = "wo;heif", IsPlainText = true});
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var result = controller.PostReply(1);
			Assert.IsInstanceOf<ViewResult>(result);
			var viewResult = (ViewResult)result;
			Assert.AreEqual("Re: blah", ((NewPost)viewResult.ViewData.Model).Title);
			Assert.AreEqual(2, ((NewPost)viewResult.ViewData.Model).ItemID);
			Assert.True(((NewPost)viewResult.Model).IncludeSignature);
			Assert.True(((NewPost)viewResult.Model).IsPlainText);
		}

		[Test]
		public void ReplyPartialInclueSigFalseWithNoSig()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(new Forum(1));
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>(), It.IsAny<Topic>())).Returns(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
			_topicService.Setup(t => t.Get(1)).Returns(new Topic(2) { Title = "blah" });
			_profileService.Setup(p => p.GetProfile(user)).Returns(new Profile { Signature = "" });
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var result = controller.PostReply(1);
			Assert.IsInstanceOf<ViewResult>(result);
			var viewResult = (ViewResult)result;
			Assert.False(((NewPost)viewResult.Model).IncludeSignature);
		}

		[Test]
		public void ReplyPartialClosedTopic()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(new Forum(1));
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>(), It.IsAny<Topic>())).Returns(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
			_topicService.Setup(t => t.Get(1)).Returns(new Topic(2) { Title = "blah", IsClosed = true });
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var result = controller.PostReply(1);
			Assert.IsInstanceOf<ContentResult>(result);
			var contentResult = (ContentResult)result;
			Assert.AreEqual(Resources.Closed, contentResult.Content);
		}

		[Test]
		public void ReplyPartialPostClosedTopic()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(new Forum(1));
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>(), It.IsAny<Topic>())).Returns(new ForumPermissionContext { UserCanPost = true, UserCanView = true });
			_topicService.Setup(t => t.Get(1)).Returns(new Topic(2) { Title = "blah", IsClosed = true });
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var newPost = new NewPost { ItemID = 1 };
			var result = controller.PostReply(newPost);
			var messageResult = GetValueFromJsonResult<string>(result, "Message");
			Assert.AreEqual(Resources.Closed, messageResult);
			Assert.IsFalse(GetValueFromJsonResult<bool>(result, "Result"));
		}

		private T GetValueFromJsonResult<T>(JsonResult jsonResult, string propertyName)
		{
			var property = jsonResult.Data.GetType().GetProperties().FirstOrDefault(p => String.Compare(p.Name, propertyName) == 0);
			if (property == null)
				throw new NullReferenceException();
			return (T)property.GetValue(jsonResult.Data, null);
		}

		[Test]
		public void ReplyPostNoUser()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.PostReply(new NewPost());
			Assert.IsInstanceOf<JsonResult>(result);
			var data = (BasicJsonMessage)result.Data;
			Assert.IsFalse(data.Result);
		}

		[Test]
		public void ReplyPostUserCantView()
		{
			var controller = GetForumController();
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext { UserCanView = false });
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.PostReply(new NewPost());
			Assert.IsInstanceOf<JsonResult>(result);
			var data = (BasicJsonMessage)result.Data;
			Assert.IsFalse(data.Result);
		}

		[Test]
		public void ReplyPostUserCantPost()
		{
			var controller = GetForumController();
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext { UserCanPost = false });
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.PostReply(new NewPost());
			Assert.IsInstanceOf<JsonResult>(result);
			var data = (BasicJsonMessage)result.Data;
			Assert.IsFalse(data.Result);
		}

		[Test]
		public void ReplyPostIsDupeOrTimeLimit()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			var forum = new Forum(1);
			var topic = new Topic(2);
			var ip = "127.0.0.1";
			var postID = 123;
			var permissionContext = new ForumPermissionContext { UserCanPost = true, UserCanView = true };
			var newPost = new NewPost { Title = "blah", FullText = "boo", IncludeSignature = true, ItemID = 2 };
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_topicService.Setup(p => p.PostReply(topic, user, 0, ip, false, newPost, It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<Func<User, string>>(), "", null)).Returns(new Post(postID));
			_postService.Setup(p => p.IsNewPostDupeOrInTimeLimit(newPost, user)).Returns(true);
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(r => r.UserHostAddress).Returns(ip);
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var result = controller.PostReply(newPost);
			Assert.IsInstanceOf<JsonResult>(result);
			var data = (BasicJsonMessage)result.Data;
			Assert.IsFalse(data.Result);
			_postService.Verify(p => p.IsNewPostDupeOrInTimeLimit(newPost, user), Times.Once());
		}

		[Test]
		public void ReplyPostUserCanPostCanView()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			var forum = new Forum(1);
			var topic = new Topic(2);
			var ip = "127.0.0.1";
			var postID = 123;
			var permissionContext = new ForumPermissionContext { UserCanPost = true, UserCanView = true };
			var newPost = new NewPost { Title = "blah", FullText = "boo", IncludeSignature = true, ItemID = 2 };
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_topicService.Setup(p => p.PostReply(topic, user, 0, ip, false, newPost, It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<Func<User, string>>(), It.IsAny<string>(), It.IsAny<Func<Post, string>>())).Returns(new Post(postID));
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(r => r.UserHostAddress).Returns(ip);
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var result = controller.PostReply(newPost);
			_topicService.Verify(p => p.PostReply(topic, user, 0, ip, false, newPost, It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<Func<User, string>>(), It.IsAny<string>(), It.IsAny<Func<Post, string>>()), Times.Once());
			Assert.IsInstanceOf<JsonResult>(result);
			var data = (BasicJsonMessage)result.Data;
			Assert.IsTrue(data.Result);
			// TODO: test for the right json data
		}

		[Test]
		public void ReplyPostFailForNonExistParent()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			var forum = new Forum(1);
			var topic = new Topic(2);
			var ip = "127.0.0.1";
			var badParent = 9876;
			var permissionContext = new ForumPermissionContext { UserCanPost = true, UserCanView = true };
			var newPost = new NewPost { Title = "blah", FullText = "boo", IncludeSignature = true, ItemID = 2, ParentPostID = badParent };
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_postService.Setup(p => p.Get(badParent)).Returns((Post)null);
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(r => r.UserHostAddress).Returns(ip);
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var result = controller.PostReply(newPost);
			Assert.IsInstanceOf<JsonResult>(result);
			var data = (BasicJsonMessage)result.Data;
			Assert.IsFalse(data.Result);
		}

		[Test]
		public void ReplyPostFailForBadParent()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			var forum = new Forum(1);
			var topic = new Topic(2);
			var ip = "127.0.0.1";
			var badParent = 9876;
			var permissionContext = new ForumPermissionContext { UserCanPost = true, UserCanView = true };
			var newPost = new NewPost { Title = "blah", FullText = "boo", IncludeSignature = true, ItemID = 2, ParentPostID = badParent };
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_postService.Setup(p => p.Get(badParent)).Returns(new Post(badParent) { TopicID = 53453326 });
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(r => r.UserHostAddress).Returns(ip);
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var result = controller.PostReply(newPost);
			Assert.IsInstanceOf<JsonResult>(result);
			var data = (BasicJsonMessage)result.Data;
			Assert.IsFalse(data.Result);
		}

		[Test]
		public void ReplyPostCloseWithReplyFromMod()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			user.Roles = new List<string> {PermanentRoles.Moderator};
			var forum = new Forum(1);
			var topic = new Topic(2);
			var ip = "127.0.0.1";
			var postID = 123;
			var permissionContext = new ForumPermissionContext { UserCanPost = true, UserCanView = true };
			var newPost = new NewPost { Title = "blah", FullText = "boo", IncludeSignature = true, ItemID = 2, CloseOnReply = true };
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_topicService.Setup(p => p.PostReply(topic, user, 0, ip, false, newPost, It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<Func<User, string>>(), It.IsAny<string>(), It.IsAny<Func<Post, string>>())).Returns(new Post(postID));
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(r => r.UserHostAddress).Returns(ip);
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			controller.PostReply(newPost);
			_topicService.Verify(t => t.CloseTopic(topic, user), Times.Exactly(1));
		}

		[Test]
		public void ReplyPostNoCloseWithReplyFromNonMod()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			var forum = new Forum(1);
			var topic = new Topic(2);
			var ip = "127.0.0.1";
			var postID = 123;
			var permissionContext = new ForumPermissionContext { UserCanPost = true, UserCanView = true };
			var newPost = new NewPost { Title = "blah", FullText = "boo", IncludeSignature = true, ItemID = 2, CloseOnReply = true };
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user)).Returns(permissionContext);
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_topicService.Setup(p => p.PostReply(topic, user, 0, ip, false, newPost, It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<Func<User, string>>(), It.IsAny<string>(), It.IsAny<Func<Post, string>>())).Returns(new Post(postID));
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(r => r.UserHostAddress).Returns(ip);
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			controller.PostReply(newPost);
			_topicService.Verify(t => t.CloseTopic(topic, user), Times.Exactly(0));
		}

		[Test]
		public void MarkForumReadThrowsWithNoUser()
		{
			var controller = GetForumController();
			Assert.Throws<Exception>(() => controller.MarkForumRead(1));
			_lastReadService.Verify(l => l.MarkForumRead(It.IsAny<User>(), It.IsAny<Forum>()), Times.Exactly(0));
		}

		[Test]
		public void MarkForumReadThrowsWithBadForumID()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			_forumService.Setup(f => f.Get(1)).Returns((Forum)null);
			Assert.Throws<Exception>(() => controller.MarkForumRead(1));
			_lastReadService.Verify(l => l.MarkForumRead(It.IsAny<User>(), It.IsAny<Forum>()), Times.Exactly(0));
		}

		[Test]
		public void MarkForumGoodUserAndForum()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			var forum = new Forum(1);
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			_forumService.Setup(f => f.Get(1)).Returns(forum);
			var result = controller.MarkForumRead(1);
			_lastReadService.Verify(l => l.MarkForumRead(user, forum), Times.Exactly(1));
			Assert.IsInstanceOf<RedirectToRouteResult>(result);
		}

		[Test]
		public void MarkAllForumsReadThrowsWithNoUser()
		{
			var controller = GetForumController();
			Assert.Throws<Exception>(() => controller.MarkAllForumsRead());
			_lastReadService.Verify(l => l.MarkAllForumsRead(It.IsAny<User>()), Times.Exactly(0));
		}

		[Test]
		public void MarkAllForumsReadGoodUser()
		{
			var controller = GetForumController();
			var user = Models.UserTest.GetTestUser();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var result = controller.MarkAllForumsRead();
			_lastReadService.Verify(l => l.MarkAllForumsRead(user), Times.Exactly(1));
			Assert.IsInstanceOf<RedirectToRouteResult>(result);
		}

		[Test]
		public void TopicListCallsLastRead()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = new User(123, DateTime.MinValue);
			controller.SetUser(user);
			var forum = new Forum(1);
			var topics = new List<Topic>();
			var permissionContext = new ForumPermissionContext { UserCanView = true };
			_userService.Setup(u => u.GetUserByName(It.IsAny<string>())).Returns(user);
			_forumService.Setup(f => f.Get(It.IsAny<string>())).Returns(forum);
			_forumService.Setup(f => f.GetPermissionContext(forum, user)).Returns(permissionContext);
			PagerContext pagerContext;
			_topicService.Setup(t => t.GetTopics(It.IsAny<Forum>(), It.IsAny<bool>(), It.IsAny<int>(), out pagerContext)).Returns(topics);
			controller.Index("blah");
			_lastReadService.Verify(l => l.GetTopicReadStatus(user, It.IsAny<PagedTopicContainer>()), Times.Exactly(1));
		}

		[Test]
		public void SinglePostNotFoundReturns404AndNotFoundView()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			_postService.Setup(p => p.Get(It.IsAny<int>())).Returns((Post) null);
			var result = controller.Post(123);
			Assert.AreEqual("NotFound", result.ViewName);
			contextHelper.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.NotFound);
		}

		[Test]
		public void SinglePostReturnsPostItemPartial()
		{
			var controller = GetForumController();
			var post = new Post(123);
			_postService.Setup(p => p.Get(123)).Returns(post);
			_topicService.Setup(t => t.Get(It.IsAny<int>())).Returns(new Topic(1));
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(new Forum(1));
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext { UserCanView = true });
			var result = controller.Post(123);
			Assert.AreEqual("PostItem", result.ViewName);
		}

		[Test]
		public void SinglePostCallsLastReadService()
		{
			var controller = GetForumController();
			var topic = new Topic(2);
			var post = new Post(123) { TopicID = topic.TopicID };
			var user = new User(456, DateTime.MinValue);
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			_postService.Setup(p => p.Get(123)).Returns(post);
			_topicService.Setup(t => t.Get(It.IsAny<int>())).Returns(topic);
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(new Forum(1));
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext { UserCanView = true });
			var result = controller.Post(123);
			_lastReadService.Verify(x => x.MarkTopicRead(user, topic), Times.Once());
		}

		[Test]
		public void SinglePostForbiddenOnNoView()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var post = new Post(123);
			_postService.Setup(p => p.Get(123)).Returns(post);
			_topicService.Setup(t => t.Get(It.IsAny<int>())).Returns(new Topic(1));
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(new Forum(1));
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext { UserCanView = false });
			var result = controller.Post(123);
			Assert.AreEqual("Forbidden", result.ViewName);
			contextHelper.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.Forbidden);
		}

		[Test]
		public void SinglePostPutsSignaturesOnViewBag()
		{
			var controller = GetForumController();
			var post = new Post(123);
			_postService.Setup(p => p.Get(123)).Returns(post);
			var sigs = new Dictionary<int, string>();
			_profileService.Setup(p => p.GetSignatures(It.IsAny<List<Post>>())).Returns(sigs);
			_topicService.Setup(t => t.Get(It.IsAny<int>())).Returns(new Topic(1));
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(new Forum(1));
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext { UserCanView = true });
			var result = controller.Post(123);
			Assert.AreSame(sigs, result.ViewBag.Signatures);
		}

		[Test]
		public void SinglePostPutsAvatarsOnViewBag()
		{
			var controller = GetForumController();
			var post = new Post(123);
			_postService.Setup(p => p.Get(123)).Returns(post);
			var avatars = new Dictionary<int, int>();
			_profileService.Setup(p => p.GetAvatars(It.IsAny<List<Post>>())).Returns(avatars);
			_topicService.Setup(t => t.Get(It.IsAny<int>())).Returns(new Topic(1));
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(new Forum(1));
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext { UserCanView = true });
			var result = controller.Post(123);
			Assert.AreSame(avatars, result.ViewBag.Avatars);
		}

		[Test]
		public void SinglePostPutsVotedPostIDsOnViewBag()
		{
			var controller = GetForumController();
			var post = new Post(123);
			_postService.Setup(p => p.Get(123)).Returns(post);
			var user = new User(123, DateTime.MinValue);
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var votePostIDs = new List<int>();
			_postService.Setup(x => x.GetVotedPostIDs(user, It.IsAny<List<Post>>())).Returns(votePostIDs);
			_topicService.Setup(t => t.Get(It.IsAny<int>())).Returns(new Topic(1));
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(new Forum(1));
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext { UserCanView = true });
			var result = controller.Post(123);
			_postService.Verify(x => x.GetVotedPostIDs(user, It.IsAny<List<Post>>()), Times.Once());
			Assert.AreSame(votePostIDs, result.ViewBag.VotedPostIDs);
		}

		[Test]
		public void SinglePostPutsUserOnViewBag()
		{
			var controller = GetForumController();
			var post = new Post(123);
			_postService.Setup(p => p.Get(123)).Returns(post);
			var user = new User(123, DateTime.MinValue);
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			var votePostIDs = new List<int>();
			_postService.Setup(x => x.GetVotedPostIDs(user, It.IsAny<List<Post>>())).Returns(votePostIDs);
			_topicService.Setup(t => t.Get(It.IsAny<int>())).Returns(new Topic(1));
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(new Forum(1));
			_forumService.Setup(f => f.GetPermissionContext(It.IsAny<Forum>(), It.IsAny<User>())).Returns(new ForumPermissionContext { UserCanView = true });
			var result = controller.Post(123);
			Assert.AreSame(user, ((User)result.ViewData[ViewDataDictionaries.ViewDataUserKey]));
		}

		[Test]
		public void Edit404WhenNoPost()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			_postService.Setup(p => p.Get(It.IsAny<int>())).Returns((Post)null);
			var result = (ViewResult)controller.Edit(123);
			Assert.AreEqual("NotFound", result.ViewName);
			contextHelper.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.NotFound);
		}

		[Test]
		public void Edit403WhenNoUser()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			_postService.Setup(p => p.Get(It.IsAny<int>())).Returns(new Post(456));
			var result = (ViewResult)controller.Edit(123);
			Assert.AreEqual("Forbidden", result.ViewName);
			contextHelper.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.Forbidden);
		}

		[Test]
		public void Edit403WhenUserDoesntOwnPost()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(new User(123, DateTime.MinValue) { Roles = new List<string>() });
			_postService.Setup(p => p.Get(It.IsAny<int>())).Returns(new Post(456) { UserID = 789 });
			var result = (ViewResult)controller.Edit(123);
			Assert.AreEqual("Forbidden", result.ViewName);
			contextHelper.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.Forbidden);
		}

		[Test]
		public void EditUserOwnsPost()
		{
			var user = new User(123, DateTime.MinValue) {Roles = new List<string>()};
			var post = new Post(456) {UserID = user.UserID};
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			_postService.Setup(p => p.Get(It.IsAny<int>())).Returns(post);
			var postEdit = new PostEdit();
			_postService.Setup(p => p.GetPostForEdit(post, user, false)).Returns(postEdit);
			var result = (ViewResult)controller.Edit(456);
			Assert.AreSame(postEdit, result.Model);
		}

		[Test]
		public void EditUserIsMod()
		{
			var user = new User(123, DateTime.MinValue) { Roles = new List<string> { PermanentRoles.Moderator} };
			var post = new Post(456) { UserID = 789 };
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			_postService.Setup(p => p.Get(It.IsAny<int>())).Returns(post);
			var postEdit = new PostEdit();
			_postService.Setup(p => p.GetPostForEdit(post, user, false)).Returns(postEdit);
			var result = (ViewResult)controller.Edit(456);
			Assert.AreSame(postEdit, result.Model);
		}

		[Test]
		public void EditPost403WhenNoUser()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			_postService.Setup(p => p.Get(It.IsAny<int>())).Returns(new Post(456));
			var result = (ViewResult)controller.Edit(123, new PostEdit());
			Assert.AreEqual("Forbidden", result.ViewName);
			contextHelper.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.Forbidden);
		}

		[Test]
		public void EditPost403WhenUserDoesntOwnPost()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(new User(123, DateTime.MinValue) { Roles = new List<string>() });
			_postService.Setup(p => p.Get(It.IsAny<int>())).Returns(new Post(456) { UserID = 789 });
			var result = (ViewResult)controller.Edit(123, new PostEdit());
			Assert.AreEqual("Forbidden", result.ViewName);
			contextHelper.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.Forbidden);
		}

		[Test]
		public void EditPostUserOwnsPost()
		{
			var user = new User(123, DateTime.MinValue) { Roles = new List<string>() };
			var post = new Post(456) { UserID = user.UserID };
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			_postService.Setup(p => p.Get(It.IsAny<int>())).Returns(post);
			var postEdit = new PostEdit();
			var result = (RedirectToRouteResult)controller.Edit(456, postEdit);
			_postService.Verify(p => p.EditPost(post, postEdit, user), Times.Exactly(1));
			Assert.AreSame("PostLink", result.RouteValues["Action"]);
		}

		[Test]
		public void EditPostUserIsMod()
		{
			var user = new User(56456, DateTime.MinValue) { Roles = new List<string> { PermanentRoles.Moderator } };
			var post = new Post(456) { UserID = user.UserID };
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(user);
			_postService.Setup(p => p.Get(It.IsAny<int>())).Returns(post);
			var postEdit = new PostEdit();
			var result = (RedirectToRouteResult)controller.Edit(456, postEdit);
			_postService.Verify(p => p.EditPost(post, postEdit, user), Times.Exactly(1));
			Assert.AreSame("PostLink", result.RouteValues["Action"]);
		}

		[Test]
		public void DeletePost403WhenNoUser()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			_postService.Setup(p => p.Get(It.IsAny<int>())).Returns(new Post(456));
			var result = (ViewResult)controller.DeletePost(123);
			Assert.AreEqual("Forbidden", result.ViewName);
			_postService.Verify(p => p.Delete(It.IsAny<Post>(), It.IsAny<User>()), Times.Exactly(0));
			contextHelper.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.Forbidden);
		}

		[Test]
		public void DeletePost403WhenNonModNonAuthor()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			controller.SetUser(new User(123, DateTime.MinValue) { Roles = new List<string>() });
			_postService.Setup(p => p.Get(It.IsAny<int>())).Returns(new Post(456));
			var result = (ViewResult)controller.DeletePost(123);
			Assert.AreEqual("Forbidden", result.ViewName);
			_postService.Verify(p => p.Delete(It.IsAny<Post>(), It.IsAny<User>()), Times.Exactly(0));
			contextHelper.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.Forbidden);
		}

		[Test]
		public void DeletePostMod()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = new User(123, DateTime.MinValue) {Roles = new List<string> {PermanentRoles.Moderator}};
			controller.SetUser(user);
			var post = new Post(456) { UserID = 1};
			_postService.Setup(p => p.Get(post.PostID)).Returns(post);
			controller.DeletePost(post.PostID);
			_postService.Verify(p => p.Delete(post, user), Times.Exactly(1));
		}

		[Test]
		public void DeletePostAuthor()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = new User(123, DateTime.MinValue) { Roles = new List<string>() };
			controller.SetUser(user);
			var post = new Post(456) { UserID = user.UserID };
			_postService.Setup(p => p.Get(post.PostID)).Returns(post);
			_topicService.Setup(t => t.Get(It.IsAny<int>())).Returns(new Topic(7));
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(new Forum(8));
			controller.DeletePost(post.PostID);
			_postService.Verify(p => p.Delete(post, user), Times.Exactly(1));
		}

		[Test]
		public void DeletePostRedirectsToPostForMod()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = new User(123, DateTime.MinValue) { Roles = new List<string> { PermanentRoles.Moderator } };
			controller.SetUser(user);
			var post = new Post(456) { UserID = user.UserID };
			_postService.Setup(p => p.Get(post.PostID)).Returns(post);
			var result = (RedirectToRouteResult)controller.DeletePost(post.PostID);
			Assert.AreSame("PostLink", result.RouteValues["Action"]);
		}

		[Test]
		public void DeletePostRedirectsForumIndexForModOnFirstInTopic()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = new User(123, DateTime.MinValue) { Roles = new List<string> { PermanentRoles.Moderator } };
			controller.SetUser(user);
			var post = new Post(456) { UserID = user.UserID, IsFirstInTopic = true };
			_postService.Setup(p => p.Get(post.PostID)).Returns(post);
			_topicService.Setup(t => t.Get(It.IsAny<int>())).Returns(new Topic(7));
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(new Forum(8));
			var result = (RedirectToRouteResult)controller.DeletePost(post.PostID);
			Assert.AreSame("Index", result.RouteValues["Action"]);
		}

		[Test]
		public void DeletePostRedirectsForumIndexForAuthor()
		{
			var controller = GetForumController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var user = new User(123, DateTime.MinValue) { Roles = new List<string>() };
			controller.SetUser(user);
			var post = new Post(456) { UserID = user.UserID };
			_postService.Setup(p => p.Get(post.PostID)).Returns(post);
			_topicService.Setup(t => t.Get(It.IsAny<int>())).Returns(new Topic(7));
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns(new Forum(8));
			var result = (RedirectToRouteResult)controller.DeletePost(post.PostID);
			Assert.AreSame("Index", result.RouteValues["Action"]);
		}

		[Test]
		public void IsLastPostTrue()
		{
			var controller = GetForumController();
			_postService.Setup(x => x.GetLastPostID(123)).Returns(456);
			Assert.IsTrue(Convert.ToBoolean(controller.IsLastPostInTopic(123, 456).Content));
		}

		[Test]
		public void IsLastPostFalse()
		{
			var controller = GetForumController();
			_postService.Setup(x => x.GetLastPostID(123)).Returns(456);
			Assert.IsFalse(Convert.ToBoolean(controller.IsLastPostInTopic(123, 789).Content));
		}
	}

	public class TestAdapter : IForumAdapter
	{
		public void AdaptForum(Controller controller, ForumTopicContainer forumTopicContainer)
		{
			_controller = controller;
			_model = forumTopicContainer;
		}

		private Controller _controller;
		private object _model;

		public void AdaptTopic(Controller controller, TopicContainer topicContainer)
		{
			_controller = controller;
			_model = topicContainer;
		}

		public RedirectResult AdaptPostLink(Controller controller, Post post, Topic topic, Forum forum)
		{
			throw new NotImplementedException();
		}

		public string ViewName
		{
			get { return "test"; }
		}

		public object Model
		{
			get { return _model; }
		}

		virtual public bool MarkViewedTopicRead
		{
			get { return true; }
		}
	}

	public class TestAdapterNoRead : TestAdapter
	{
		public override bool MarkViewedTopicRead
		{
			get { return false; }
		}
	}
}
