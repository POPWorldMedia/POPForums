using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PopForums.Configuration;
using PopForums.Messaging;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;
using PopForums.Services;

namespace PopForums.Test.Services
{
	[TestFixture]
	public class TopicServiceTests
	{
		private Mock<ISettingsManager> _settingsManager;
		private Mock<IForumRepository> _forumRepo;
		private Mock<ITopicRepository> _topicRepo;
		private Mock<IPostRepository> _postRepo;
		private Mock<IProfileRepository> _profileRepo;
		private Mock<ITextParsingService> _textParser;
		private Mock<ISubscribedTopicsService> _subService;
		private Mock<IModerationLogService> _modService;
		private Mock<IForumService> _forumService;
		private Mock<IEventPublisher> _eventPublisher;
		private Mock<IBroker> _broker;

		private TopicService GetTopicService()
		{
			_settingsManager = new Mock<ISettingsManager>();
			_forumRepo = new Mock<IForumRepository>();
			_topicRepo = new Mock<ITopicRepository>();
			_postRepo = new Mock<IPostRepository>();
			_profileRepo = new Mock<IProfileRepository>();
			_textParser = new Mock<ITextParsingService>();
			_subService = new Mock<ISubscribedTopicsService>();
			_modService = new Mock<IModerationLogService>();
			_forumService = new Mock<IForumService>();
			_eventPublisher = new Mock<IEventPublisher>();
			_broker = new Mock<IBroker>();
			return new TopicService(_forumRepo.Object, _topicRepo.Object, _postRepo.Object, _profileRepo.Object, _textParser.Object, _settingsManager.Object, _subService.Object, _modService.Object, _forumService.Object, _eventPublisher.Object, _broker.Object);
		}

		private static User GetUser()
		{
			return new User(123, DateTime.MinValue) {Name = "Name", Email = "Email", IsApproved = true, LastActivityDate = DateTime.MaxValue, LastLoginDate = DateTime.MaxValue, AuthorizationKey = Guid.NewGuid(), Roles = new List<string>()};
		}

		[Test]
		public void GetTopicsFromRepo()
		{
			var forum = new Forum(1) { TopicCount = 3 };
			var topicService = GetTopicService();
			var repoTopics = new List<Topic>();
			var settings = new Settings {TopicsPerPage = 20};
			_topicRepo.Setup(t => t.Get(1, true, 1, settings.TopicsPerPage)).Returns(repoTopics);
			_settingsManager.Setup(s => s.Current).Returns(settings);
			PagerContext pagerContext;
			var topics = topicService.GetTopics(forum, true, 1, out pagerContext);
			Assert.AreSame(repoTopics, topics);
		}

		[Test]
		public void GetTopicsStartRowCalcd()
		{
			var forum = new Forum(1) { TopicCount = 300 };
			var topicService = GetTopicService();
			var settings = new Settings { TopicsPerPage = 20 };
			_settingsManager.Setup(s => s.Current).Returns(settings);
			PagerContext pagerContext;
			topicService.GetTopics(forum, false, 3, out pagerContext);
			_topicRepo.Verify(t => t.Get(It.IsAny<int>(), It.IsAny<bool>(), 41, It.IsAny<int>()), Times.Once());
		}

		[Test]
		public void GetTopicsIncludeDeletedCallsRepoCount()
		{
			var forum = new Forum(1);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<Topic>());
			_topicRepo.Setup(t => t.GetTopicCount(1, true)).Returns(350);
			_settingsManager.Setup(s => s.Current).Returns(new Settings());
			PagerContext pagerContext;
			topicService.GetTopics(forum, true, 3, out pagerContext);
			_topicRepo.Verify(t => t.GetTopicCount(forum.ForumID, true), Times.Once());
		}

		[Test]
		public void GetTopicsNotIncludeDeletedNotCallRepoCount()
		{
			var forum = new Forum(1);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<Topic>());
			_settingsManager.Setup(s => s.Current).Returns(new Settings());
			PagerContext pagerContext;
			topicService.GetTopics(forum, false, 3, out pagerContext);
			_topicRepo.Verify(t => t.GetTopicCount(forum.ForumID, false), Times.Never());
		}

		[Test]
		public void GetTopicsPagerContextIncludesPageIndexAndCalcdTotalPages()
		{
			var forum = new Forum(1) {TopicCount = 301};
			var forum2 = new Forum(2) {TopicCount = 300};
			var forum3 = new Forum(3) {TopicCount = 299};
			var topicService = GetTopicService();
			var settings = new Settings { TopicsPerPage = 20 };
			_topicRepo.Setup(t => t.Get(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), settings.TopicsPerPage)).Returns(new List<Topic>());
			_settingsManager.Setup(s => s.Current).Returns(settings);
			PagerContext pagerContext;
			topicService.GetTopics(forum, false, 3, out pagerContext);
			Assert.AreEqual(3, pagerContext.PageIndex);
			Assert.AreEqual(16, pagerContext.PageCount);
			topicService.GetTopics(forum2, false, 4, out pagerContext);
			Assert.AreEqual(4, pagerContext.PageIndex);
			Assert.AreEqual(15, pagerContext.PageCount);
			topicService.GetTopics(forum3, false, 5, out pagerContext);
			Assert.AreEqual(5, pagerContext.PageIndex);
			Assert.AreEqual(15, pagerContext.PageCount);
			Assert.AreEqual(settings.TopicsPerPage, pagerContext.PageSize);
		}

		[Test]
		public void PostReplyHitsRepo()
		{
			var topic = new Topic(1) { Title = "" };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var topicService = GetTopicService();
			_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
			var newPost = new NewPost {FullText = "mah text", Title = "mah title", IncludeSignature = true};
			_textParser.Setup(t => t.ClientHtmlToHtml(newPost.FullText)).Returns("parsed text");
			_textParser.Setup(t => t.EscapeHtmlAndCensor(newPost.Title)).Returns("parsed title");
			topicService.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", It.IsAny<Func<User, string>>(), "", x => "");
			_postRepo.Verify(p => p.Create(topic.TopicID, 0, "127.0.0.1", false, true, user.UserID, user.Name, "parsed title", "parsed text", postTime, false, user.Name, null, false, 0));
		}

		[Test]
		public void PostReplyHitsSubscribedService()
		{
			var topic = new Topic(1);
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var topicService = GetTopicService();
			_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
			topicService.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
			_subService.Verify(s => s.NotifySubscribers(topic, user, It.IsAny<string>(), It.IsAny<Func<User, string>>()), Times.Once());
		}

		[Test]
		public void PostReplyHitsTextParserRichText()
		{
			var topic = new Topic(1);
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var topicService = GetTopicService();
			_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, IsPlainText = false };
			topicService.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
			_textParser.Verify(t => t.EscapeHtmlAndCensor("mah title"), Times.Once());
			_textParser.Verify(t => t.ClientHtmlToHtml("mah text"), Times.Once());
			_textParser.Verify(t => t.ForumCodeToHtml("mah text"), Times.Exactly(0));
		}

		[Test]
		public void PostReplyHitsTextParserPlainText()
		{
			var topic = new Topic(1);
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var topicService = GetTopicService();
			_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true, IsPlainText = true };
			topicService.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
			_textParser.Verify(t => t.EscapeHtmlAndCensor("mah title"), Times.Once());
			_textParser.Verify(t => t.ClientHtmlToHtml("mah text"), Times.Exactly(0));
			_textParser.Verify(t => t.ForumCodeToHtml("mah text"), Times.Once());
		}

		[Test]
		public void PostReplyIncrementsTopicReplyCount()
		{
			var topic = new Topic(1);
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var topicService = GetTopicService();
			_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
			topicService.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
			_topicRepo.Verify(t => t.IncrementReplyCount(1));
		}

		[Test]
		public void PostReplyIncrementsForumPostCount()
		{
			var topic = new Topic(1) { ForumID = 2};
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var topicService = GetTopicService();
			_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
			topicService.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
			_forumRepo.Verify(f => f.IncrementPostCount(2));
		}

		[Test]
		public void PostReplyUpdatesTopicLastInfo()
		{
			var topic = new Topic(1) { ForumID = 2 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var topicService = GetTopicService();
			_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
			topicService.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
			_topicRepo.Verify(t => t.UpdateLastTimeAndUser(topic.TopicID, user.UserID, user.Name, postTime));
		}

		[Test]
		public void PostReplyUpdatesForumLastInfo()
		{
			var topic = new Topic(1) { ForumID = 2 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var topicService = GetTopicService();
			_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
			topicService.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
			_forumRepo.Verify(f => f.UpdateLastTimeAndUser(topic.ForumID, postTime, user.Name));
		}

		[Test]
		public void PostReplyNotifiesBroker()
		{
			var topic = new Topic(1) { ForumID = 2 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var topicService = GetTopicService();
			_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
			var forum = new Forum(topic.ForumID);
			_forumRepo.Setup(x => x.Get(topic.ForumID)).Returns(forum);
			_topicRepo.Setup(x => x.Get(topic.TopicID)).Returns(topic);
			topicService.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
			_broker.Verify(x => x.NotifyForumUpdate(forum), Times.Once());
			_broker.Verify(x => x.NotifyTopicUpdate(topic, forum, It.IsAny<string>()), Times.Once());
			_broker.Verify(x => x.NotifyNewPost(topic, It.IsAny<int>()), Times.Once());
		}

		[Test]
		public void PostReplySetsProfileLastPostID()
		{
			var topic = new Topic(1) { ForumID = 2 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var topicService = GetTopicService();
			_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
			var post = topicService.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
			_profileRepo.Verify(p => p.SetLastPostID(user.UserID, post.PostID));
		}

		[Test]
		public void PostReplyPublishesEvent()
		{
			var topic = new Topic(1) { ForumID = 2 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var topicService = GetTopicService();
			_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
			topicService.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
			_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), user, EventDefinitionService.StaticEventIDs.NewPost, false), Times.Once());
		}

		[Test]
		public void PostReplyDoesNotPublisheEventOnViewRestrictedForum()
		{
			var topic = new Topic(1) { ForumID = 2 };
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var topicService = GetTopicService();
			_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string> { "Admin" });
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
			topicService.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
			_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), user, EventDefinitionService.StaticEventIDs.NewPost, true), Times.Once());
		}

		[Test]
		public void PostReplyReturnsHydratedObject()
		{
			var topic = new Topic(1);
			var user = GetUser();
			var postTime = DateTime.UtcNow;
			var topicService = GetTopicService();
			_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
			_postRepo.Setup(p => p.Create(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), false, true, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), false, It.IsAny<string>(), null, false, 0)).Returns(123);
			_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
			_textParser.Setup(t => t.EscapeHtmlAndCensor("mah title")).Returns("parsed title");
			var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
			var post = topicService.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
			Assert.AreEqual(topic.TopicID, post.TopicID);
			Assert.AreEqual("parsed text", post.FullText);
			Assert.AreEqual("127.0.0.1", post.IP);
			Assert.IsFalse(post.IsDeleted);
			Assert.IsFalse(post.IsEdited);
			Assert.IsFalse(post.IsFirstInTopic);
			Assert.AreEqual(user.Name, post.LastEditName);
			Assert.IsNull(post.LastEditTime);
			Assert.AreEqual(user.Name, post.Name);
			Assert.AreEqual(0, post.ParentPostID);
			Assert.AreEqual(123, post.PostID);
			Assert.AreEqual(postTime, post.PostTime);
			Assert.IsTrue(post.ShowSig);
			Assert.AreEqual("parsed title", post.Title);
			Assert.AreEqual(user.UserID, post.UserID);
		}

		[Test]
		public void CloseTopicThrowsWithNonMod()
		{
			var topic = new Topic(1);
			var user = GetUser();
			var topicService = GetTopicService();
			Assert.Throws<InvalidOperationException>(() => topicService.CloseTopic(topic, user));
		}

		[Test]
		public void CloseTopicClosesWithMod()
		{
			var topic = new Topic(1);
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			topicService.CloseTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicClose, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.CloseTopic(topic.TopicID), Times.Exactly(1));
		}

		[Test]
		public void OpenTopicThrowsWithNonMod()
		{
			var topic = new Topic(1);
			var user = GetUser();
			var topicService = GetTopicService();
			Assert.Throws<InvalidOperationException>(() => topicService.OpenTopic(topic, user));
		}

		[Test]
		public void OpenTopicOpensWithMod()
		{
			var topic = new Topic(1);
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			topicService.OpenTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicOpen, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.OpenTopic(topic.TopicID), Times.Exactly(1));
		}

		[Test]
		public void PinTopicThrowsWithNonMod()
		{
			var topic = new Topic(1);
			var user = GetUser();
			var topicService = GetTopicService();
			Assert.Throws<InvalidOperationException>(() => topicService.PinTopic(topic, user));
		}

		[Test]
		public void PinTopicPinsWithMod()
		{
			var topic = new Topic(1);
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			topicService.PinTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicPin, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.PinTopic(topic.TopicID), Times.Exactly(1));
		}

		[Test]
		public void UnpinTopicThrowsWithNonMod()
		{
			var topic = new Topic(1);
			var user = GetUser();
			var topicService = GetTopicService();
			Assert.Throws<InvalidOperationException>(() => topicService.UnpinTopic(topic, user));
		}

		[Test]
		public void UnpinTopicUnpinsWithMod()
		{
			var topic = new Topic(1);
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			topicService.UnpinTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicUnpin, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.UnpinTopic(topic.TopicID), Times.Exactly(1));
		}

		[Test]
		public void DeleteTopicThrowsWithNonMod()
		{
			var topic = new Topic(1);
			var user = GetUser();
			var topicService = GetTopicService();
			Assert.Throws<InvalidOperationException>(() => topicService.DeleteTopic(topic, user));
		}

		[Test]
		public void DeleteTopicDeletesWithMod()
		{
			var topic = new Topic(1);
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			topicService.DeleteTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicDelete, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.DeleteTopic(topic.TopicID), Times.Exactly(1));
		}

		[Test]
		public void DeleteTopicUpdatesCounts()
		{
			var topic = new Topic(1) { ForumID = 123 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			var forum = new Forum(topic.ForumID);
			_forumService.Setup(f => f.Get(topic.ForumID)).Returns(forum);
			topicService.DeleteTopic(topic, user);
			_forumService.Verify(f => f.UpdateCounts(forum), Times.Exactly(1));
		}

		[Test]
		public void DeleteTopicUpdatesLast()
		{
			var topic = new Topic(1) { ForumID = 123 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			var forum = new Forum(topic.ForumID);
			_forumService.Setup(f => f.Get(topic.ForumID)).Returns(forum);
			topicService.DeleteTopic(topic, user);
			_forumService.Verify(f => f.UpdateLast(forum), Times.Exactly(1));
		}

		[Test]
		public void DeleteTopicUpdatesReplyCount()
		{
			var topic = new Topic(1);
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_postRepo.Setup(t => t.GetReplyCount(topic.TopicID, false)).Returns(42);
			topicService.DeleteTopic(topic, user);
			_topicRepo.Verify(t => t.UpdateReplyCount(topic.TopicID, 42), Times.Exactly(1));
		}

		[Test]
		public void DeleteTopicDeletesWithStarter()
		{
			var user = GetUser();
			var topic = new Topic(1) { StartedByUserID = user.UserID };
			var topicService = GetTopicService();
			topicService.DeleteTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicDelete, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.DeleteTopic(topic.TopicID), Times.Exactly(1));
		}

		[Test]
		public void UndeleteTopicThrowsWithNonMod()
		{
			var topic = new Topic(1);
			var user = GetUser();
			var topicService = GetTopicService();
			Assert.Throws<InvalidOperationException>(() => topicService.UndeleteTopic(topic, user));
		}

		[Test]
		public void UndeleteTopicUndeletesWithMod()
		{
			var topic = new Topic(1);
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			topicService.UndeleteTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicUndelete, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.UndeleteTopic(topic.TopicID), Times.Exactly(1));
		}

		[Test]
		public void UndeleteTopicUpdatesCounts()
		{
			var topic = new Topic(1) { ForumID = 123 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			var forum = new Forum(topic.ForumID);
			_forumService.Setup(f => f.Get(topic.ForumID)).Returns(forum);
			topicService.UndeleteTopic(topic, user);
			_forumService.Verify(f => f.UpdateCounts(forum), Times.Exactly(1));
		}

		[Test]
		public void UndeleteTopicUpdatesLast()
		{
			var topic = new Topic(1) {ForumID = 123};
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			var forum = new Forum(topic.ForumID);
			_forumService.Setup(f => f.Get(topic.ForumID)).Returns(forum);
			topicService.UndeleteTopic(topic, user);
			_forumService.Verify(f => f.UpdateLast(forum), Times.Exactly(1));
		}

		[Test]
		public void UndeleteTopicUpdatesReplyCount()
		{
			var topic = new Topic(1);
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_postRepo.Setup(t => t.GetReplyCount(topic.TopicID, false)).Returns(42);
			topicService.UndeleteTopic(topic, user);
			_topicRepo.Verify(t => t.UpdateReplyCount(topic.TopicID, 42), Times.Exactly(1));
		}

		[Test]
		public void UpdateTopicThrowsWithNonMod()
		{
			var topic = new Topic(1);
			var user = GetUser();
			var topicService = GetTopicService();
			Assert.Throws<InvalidOperationException>(() => topicService.UpdateTitleAndForum(topic, new Forum(2), "blah", user));
		}

		[Test]
		public void UpdateTopicUpdatesTitleWithMod()
		{
			var forum = new Forum(2);
			var topic = new Topic(1) { ForumID = forum.ForumID };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(topic.TopicID)).Returns(new Topic(1) { ForumID = 2 });
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).Returns(new List<string>());
			topicService.UpdateTitleAndForum(topic, forum, "new title", user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicRenamed, topic, forum, It.IsAny<string>()), Times.Exactly(1));
			_topicRepo.Verify(t => t.UpdateTitleAndForum(topic.TopicID, forum.ForumID, "new title", "new-title"), Times.Exactly(1));
		}

		[Test]
		public void UpdateTopicMovesTopicWithMod()
		{
			var forum = new Forum(2);
			var topic = new Topic(1) { ForumID = 7, Title = String.Empty };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(topic.TopicID)).Returns(new Topic(1) { ForumID = 3 });
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).Returns(new List<string>());
			topicService.UpdateTitleAndForum(topic, forum, String.Empty, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicMoved, topic, forum, It.IsAny<string>()), Times.Exactly(1));
			_topicRepo.Verify(t => t.UpdateTitleAndForum(topic.TopicID, forum.ForumID, String.Empty, String.Empty), Times.Exactly(1));
		}

		[Test]
		public void UpdateTopicWithNewTitleChangesUrlNameOnTopicParameter()
		{
			var forum = new Forum(2);
			var topic = new Topic(1) { ForumID = forum.ForumID, UrlName = "old" };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).Returns(new List<string>());
			_topicRepo.Setup(t => t.Get(topic.TopicID)).Returns(new Topic(1) { ForumID = 2 });
			topicService.UpdateTitleAndForum(topic, forum, "new title", user);
			Assert.AreEqual("new-title", topic.UrlName);
		}

		[Test]
		public void UpdateTopicMovesUpdatesCountAndLastOnOldForum()
		{
			var forum = new Forum(2);
			var oldForum = new Forum(3);
			var topic = new Topic(1) { ForumID = 7, Title = String.Empty };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(topic.TopicID)).Returns(new Topic(1) { ForumID = oldForum.ForumID });
			_forumService.Setup(f => f.Get(oldForum.ForumID)).Returns(oldForum);
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).Returns(new List<string>());
			topicService.UpdateTitleAndForum(topic, forum, String.Empty, user);
			_forumService.Verify(f => f.UpdateCounts(forum), Times.Exactly(1));
			_forumService.Verify(f => f.UpdateLast(forum), Times.Exactly(1));
		}

		[Test]
		public void UpdateTopicMovesUpdatesCountAndLastOnNewForum()
		{
			var forum = new Forum(2);
			var oldForum = new Forum(3);
			var topic = new Topic(1) { ForumID = 7, Title = String.Empty };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(topic.TopicID)).Returns(new Topic(1) { ForumID = oldForum.ForumID });
			_forumService.Setup(f => f.Get(oldForum.ForumID)).Returns(oldForum);
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).Returns(new List<string>());
			topicService.UpdateTitleAndForum(topic, forum, String.Empty, user);
			_forumService.Verify(f => f.UpdateCounts(oldForum), Times.Exactly(1));
			_forumService.Verify(f => f.UpdateLast(oldForum), Times.Exactly(1));
		}

		[Test]
		public void UpdateLastSetsFieldsFromLastPost()
		{
			var topic = new Topic(456);
			var post = new Post(123) {TopicID = topic.TopicID, UserID = 789, Name = "Dude", PostTime = new DateTime(2000, 1, 3)};
			var service = GetTopicService();
			_postRepo.Setup(x => x.GetLastInTopic(post.TopicID)).Returns(post);
			service.UpdateLast(topic);
			_topicRepo.Verify(x => x.UpdateLastTimeAndUser(topic.TopicID, post.UserID, post.Name, post.PostTime), Times.Once());
		}
	}
}
