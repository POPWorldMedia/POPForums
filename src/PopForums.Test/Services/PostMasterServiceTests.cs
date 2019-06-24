using System;
using System.Collections.Generic;
using Moq;
using PopForums.Messaging;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;
using PopForums.Services;
using Xunit;

namespace PopForums.Test.Services
{
	public class PostMasterServiceTests
	{
		private PostMasterService GetService()
		{
			_textParser = new Mock<ITextParsingService>();
			_topicRepo = new Mock<ITopicRepository>();
			_postRepo = new Mock<IPostRepository>();
			_forumRepo = new Mock<IForumRepository>();
			_profileRepo = new Mock<IProfileRepository>();
			_eventPublisher = new Mock<IEventPublisher>();
			_broker = new Mock<IBroker>();
			_searchIndexQueueRepo = new Mock<ISearchIndexQueueRepository>();
			_tenantService = new Mock<ITenantService>();
			_subscribedTopicsService = new Mock<ISubscribedTopicsService>();
			_moderationLogService = new Mock<IModerationLogService>();
			_forumPermissionService = new Mock<IForumPermissionService>();
			return new PostMasterService(_textParser.Object, _topicRepo.Object, _postRepo.Object, _forumRepo.Object, _profileRepo.Object, _eventPublisher.Object, _broker.Object, _searchIndexQueueRepo.Object, _tenantService.Object, _subscribedTopicsService.Object, _moderationLogService.Object, _forumPermissionService.Object);
		}

		private Mock<ITextParsingService> _textParser;
		private Mock<ITopicRepository> _topicRepo;
		private Mock<IPostRepository> _postRepo;
		private Mock<IForumRepository> _forumRepo;
		private Mock<IProfileRepository> _profileRepo;
		private Mock<IEventPublisher> _eventPublisher;
		private Mock<IBroker> _broker;
		private Mock<ISearchIndexQueueRepository> _searchIndexQueueRepo;
		private Mock<ITenantService> _tenantService;
		private Mock<ISubscribedTopicsService> _subscribedTopicsService;
		private Mock<IModerationLogService> _moderationLogService;
		private Mock<IForumPermissionService> _forumPermissionService;

		private User DoUpNewTopic()
		{
			var forum = new Forum { ForumID = 1 };
			var user = GetUser();
			const string ip = "127.0.0.1";
			const string title = "mah title";
			const string text = "mah text";
			var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
			var forumService = GetService();
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
			_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
			_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
			_postRepo.Setup(p => p.Create(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<string>(), null, It.IsAny<bool>(), It.IsAny<int>())).Returns(69);
			_forumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string>());
			_topicRepo.Setup(x => x.Create(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).Returns(111);
			forumService.PostNewTopic(forum, user, new ForumPermissionContext { UserCanPost = true, UserCanView = true }, newPost, ip, It.IsAny<string>(), x => "");
			return user;
		}

		private User GetUser()
		{
			var user = Models.UserTest.GetTestUser();
			user.Roles = new List<string>();
			return user;
		}

		public class PostNewTopicTests : PostMasterServiceTests
		{
			[Fact]
			public void UserWithoutPermissionThrowsOnPost()
			{
				var topicService = GetService();
				Assert.Throws<Exception>(() => topicService.PostNewTopic(new Forum { ForumID = 1 }, GetUser(), new ForumPermissionContext { UserCanPost = false }, new NewPost(), String.Empty, It.IsAny<string>(), x => ""));
				Assert.Throws<Exception>(() => topicService.PostNewTopic(new Forum { ForumID = 1 }, GetUser(), new ForumPermissionContext { UserCanView = false }, new NewPost(), String.Empty, It.IsAny<string>(), x => ""));
			}

			[Fact]
			public void CallsTopicRepoCreate()
			{
				var forum = new Forum { ForumID = 1 };
				var user = GetUser();
				const string ip = "127.0.0.1";
				const string title = "mah title";
				const string text = "mah text";
				var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
				var topicService = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string>());
				_topicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
				_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
				_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
				topicService.PostNewTopic(forum, user, new ForumPermissionContext { UserCanPost = true, UserCanView = true }, newPost, ip, It.IsAny<string>(), x => "");
				_topicRepo.Verify(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, "parsed-title"), Times.Once());
			}

			[Fact]
			public void CallsForumTopicPostIncrement()
			{
				DoUpNewTopic();
				_forumRepo.Verify(f => f.IncrementPostAndTopicCount(1), Times.Once());
			}

			[Fact]
			public void CallsForumUpdateLastUser()
			{
				var user = DoUpNewTopic();
				_forumRepo.Verify(f => f.UpdateLastTimeAndUser(1, It.IsAny<DateTime>(), user.Name), Times.Once());
			}

			[Fact]
			public void CallsProfileSetLastPost()
			{
				var user = DoUpNewTopic();
				_profileRepo.Verify(p => p.SetLastPostID(user.UserID, 69), Times.Once());
			}

			[Fact]
			public void PublishesNewTopicEvent()
			{
				var user = DoUpNewTopic();
				_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), user, EventDefinitionService.StaticEventIDs.NewTopic, false), Times.Once());
			}

			[Fact]
			public void PublishesNewPostEvent()
			{
				var user = DoUpNewTopic();
				_eventPublisher.Verify(x => x.ProcessEvent(String.Empty, user, EventDefinitionService.StaticEventIDs.NewPost, true), Times.Once());
			}

			[Fact]
			public void CallsBroker()
			{
				DoUpNewTopic();
				_broker.Verify(x => x.NotifyForumUpdate(It.IsAny<Forum>()), Times.Once());
				_broker.Verify(x => x.NotifyTopicUpdate(It.IsAny<Topic>(), It.IsAny<Forum>(), It.IsAny<string>()), Times.Once());
			}

			[Fact]
			public void QueuesTopicForIndexing()
			{
				DoUpNewTopic();
				_tenantService.Setup(x => x.GetTenant()).Returns("");
				_searchIndexQueueRepo.Verify(x => x.Enqueue(It.IsAny<SearchIndexPayload>()), Times.Once);
			}

			[Fact]
			public void DoesNotPublishToFeedIfForumHasViewRestrictions()
			{
				var forum = new Forum { ForumID = 1 };
				var user = GetUser();
				const string ip = "127.0.0.1";
				const string title = "mah title";
				const string text = "mah text";
				var newPost = new NewPost { Title = title, FullText = text, ItemID = 1 };
				var topicService = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string> { "Admin" });
				_topicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
				_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
				_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
				_topicRepo.Setup(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, "parsed-title")).Returns(2);
				var topic = topicService.PostNewTopic(forum, user, new ForumPermissionContext { UserCanPost = true, UserCanView = true }, newPost, ip, It.IsAny<string>(), x => "");
				_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), It.IsAny<User>(), EventDefinitionService.StaticEventIDs.NewTopic, true), Times.Once());
			}

			[Fact]
			public void ReturnsTopic()
			{
				var forum = new Forum {ForumID = 1};
				var user = GetUser();
				const string ip = "127.0.0.1";
				const string title = "mah title";
				const string text = "mah text";
				var newPost = new NewPost {Title = title, FullText = text, ItemID = 1};
				var topicService = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(forum.ForumID)).Returns(new List<string>());
				_topicRepo.Setup(t => t.GetUrlNamesThatStartWith("parsed-title")).Returns(new List<string>());
				_textParser.Setup(t => t.ClientHtmlToHtml("mah text")).Returns("parsed text");
				_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
				_topicRepo.Setup(t => t.Create(forum.ForumID, "parsed title", 0, 0, user.UserID, user.Name, user.UserID, user.Name, It.IsAny<DateTime>(), false, false, false, "parsed-title")).Returns(2);
				var topic = topicService.PostNewTopic(forum, user, new ForumPermissionContext {UserCanPost = true, UserCanView = true}, newPost, ip, It.IsAny<string>(), x => "");
				Assert.Equal(2, topic.TopicID);
				Assert.Equal(forum.ForumID, topic.ForumID);
				Assert.Equal("parsed title", topic.Title);
				Assert.Equal(0, topic.ReplyCount);
				Assert.Equal(0, topic.ViewCount);
				Assert.Equal(user.UserID, topic.StartedByUserID);
				Assert.Equal(user.Name, topic.StartedByName);
				Assert.Equal(user.UserID, topic.LastPostUserID);
				Assert.Equal(user.Name, topic.LastPostName);
				Assert.False(topic.IsClosed);
				Assert.False(topic.IsDeleted);
				Assert.False(topic.IsPinned);
				Assert.Equal("parsed-title", topic.UrlName);
			}
		}

		public class PostReplyTests : PostNewTopicTests
		{
			[Fact]
			public void PostReplyHitsRepo()
			{
				var topic = new Topic { TopicID = 1, Title = "" };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
				_textParser.Setup(t => t.Censor(newPost.Title)).Returns("parsed title");
				service.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", It.IsAny<Func<User, string>>(), "", x => "");
				_postRepo.Verify(p => p.Create(topic.TopicID, 0, "127.0.0.1", false, true, user.UserID, user.Name, "parsed title", "mah text", postTime, false, user.Name, null, false, 0));
			}

			[Fact]
			public void PostReplyHitsSubscribedService()
			{
				var topic = new Topic { TopicID = 1 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
				service.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
				_subscribedTopicsService.Verify(s => s.NotifySubscribers(topic, user, It.IsAny<string>(), It.IsAny<Func<User, string>>()), Times.Once());
			}

			[Fact]
			public void PostReplyIncrementsTopicReplyCount()
			{
				var topic = new Topic { TopicID = 1 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
				service.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
				_topicRepo.Verify(t => t.IncrementReplyCount(1));
			}

			[Fact]
			public void PostReplyIncrementsForumPostCount()
			{
				var topic = new Topic { TopicID = 1, ForumID = 2 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
				service.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
				_forumRepo.Verify(f => f.IncrementPostCount(2));
			}

			[Fact]
			public void PostReplyUpdatesTopicLastInfo()
			{
				var topic = new Topic { TopicID = 1, ForumID = 2 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
				service.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
				_topicRepo.Verify(t => t.UpdateLastTimeAndUser(topic.TopicID, user.UserID, user.Name, postTime));
			}

			[Fact]
			public void PostReplyUpdatesForumLastInfo()
			{
				var topic = new Topic { TopicID = 1, ForumID = 2 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
				service.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
				_forumRepo.Verify(f => f.UpdateLastTimeAndUser(topic.ForumID, postTime, user.Name));
			}

			[Fact]
			public void PostQueuesMarksTopicForIndexing()
			{
				var topic = new Topic { TopicID = 1, ForumID = 2 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
				_tenantService.Setup(x => x.GetTenant()).Returns("");
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
				service.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
				_searchIndexQueueRepo.Verify(x => x.Enqueue(It.IsAny<SearchIndexPayload>()), Times.Once);
			}

			[Fact]
			public void PostReplyNotifiesBroker()
			{
				var topic = new Topic { TopicID = 1, ForumID = 2 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
				var forum = new Forum { ForumID = topic.ForumID };
				_forumRepo.Setup(x => x.Get(topic.ForumID)).Returns(forum);
				_topicRepo.Setup(x => x.Get(topic.TopicID)).Returns(topic);
				service.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
				_broker.Verify(x => x.NotifyForumUpdate(forum), Times.Once());
				_broker.Verify(x => x.NotifyTopicUpdate(topic, forum, It.IsAny<string>()), Times.Once());
				_broker.Verify(x => x.NotifyNewPost(topic, It.IsAny<int>()), Times.Once());
			}

			[Fact]
			public void PostReplySetsProfileLastPostID()
			{
				var topic = new Topic { TopicID = 1, ForumID = 2 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
				var post = service.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
				_profileRepo.Verify(p => p.SetLastPostID(user.UserID, post.PostID));
			}

			[Fact]
			public void PostReplyPublishesEvent()
			{
				var topic = new Topic { TopicID = 1, ForumID = 2 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
				service.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
				_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), user, EventDefinitionService.StaticEventIDs.NewPost, false), Times.Once());
			}

			[Fact]
			public void PostReplyDoesNotPublisheEventOnViewRestrictedForum()
			{
				var topic = new Topic { TopicID = 1, ForumID = 2 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string> { "Admin" });
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
				service.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
				_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), user, EventDefinitionService.StaticEventIDs.NewPost, true), Times.Once());
			}

			[Fact]
			public void PostReplyReturnsHydratedObject()
			{
				var topic = new Topic { TopicID = 1 };
				var user = GetUser();
				var postTime = DateTime.UtcNow;
				var service = GetService();
				_forumRepo.Setup(x => x.GetForumViewRoles(It.IsAny<int>())).Returns(new List<string>());
				_postRepo.Setup(p => p.Create(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), false, true, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), false, It.IsAny<string>(), null, false, 0)).Returns(123);
				_textParser.Setup(t => t.Censor("mah title")).Returns("parsed title");
				var newPost = new NewPost { FullText = "mah text", Title = "mah title", IncludeSignature = true };
				var post = service.PostReply(topic, user, 0, "127.0.0.1", false, newPost, postTime, "", u => "", "", x => "");
				Assert.Equal(topic.TopicID, post.TopicID);
				Assert.Equal("mah text", post.FullText);
				Assert.Equal("127.0.0.1", post.IP);
				Assert.False(post.IsDeleted);
				Assert.False(post.IsEdited);
				Assert.False(post.IsFirstInTopic);
				Assert.Equal(user.Name, post.LastEditName);
				Assert.Null(post.LastEditTime);
				Assert.Equal(user.Name, post.Name);
				Assert.Equal(0, post.ParentPostID);
				Assert.Equal(123, post.PostID);
				Assert.Equal(postTime, post.PostTime);
				Assert.True(post.ShowSig);
				Assert.Equal("parsed title", post.Title);
				Assert.Equal(user.UserID, post.UserID);
			}
		}

		public class EditPostTests : PostMasterServiceTests
		{
			[Fact]
			public void EditPostCensorsTitle()
			{
				var service = GetService();
				service.EditPost(new Post { PostID = 456 }, new PostEdit { Title = "blah" }, new User());
				_textParser.Verify(t => t.Censor("blah"), Times.Exactly(1));
			}

			[Fact]
			public void EditPostPlainTextParsed()
			{
				var service = GetService();
				service.EditPost(new Post { PostID = 456 }, new PostEdit { FullText = "blah", IsPlainText = true }, new User());
				_textParser.Verify(t => t.ForumCodeToHtml("blah"), Times.Exactly(1));
			}

			[Fact]
			public void EditPostRichTextParsed()
			{
				var service = GetService();
				service.EditPost(new Post { PostID = 456 }, new PostEdit { FullText = "blah", IsPlainText = false }, new User());
				_textParser.Verify(t => t.ClientHtmlToHtml("blah"), Times.Exactly(1));
			}

			[Fact]
			public void EditPostSavesMappedValues()
			{
				var service = GetService();
				var post = new Post { PostID = 67 };
				_postRepo.Setup(p => p.Update(It.IsAny<Post>())).Callback<Post>(p => post = p);
				_textParser.Setup(t => t.ClientHtmlToHtml("blah")).Returns("new");
				_textParser.Setup(t => t.Censor("unparsed title")).Returns("new title");
				service.EditPost(new Post { PostID = 456, ShowSig = false }, new PostEdit { FullText = "blah", Title = "unparsed title", IsPlainText = false, ShowSig = true }, new User { UserID = 123, Name = "dude" });
				Assert.NotEqual(post.LastEditTime, new DateTime(2009, 1, 1));
				Assert.Equal(456, post.PostID);
				Assert.Equal("new", post.FullText);
				Assert.Equal("new title", post.Title);
				Assert.True(post.ShowSig);
				Assert.True(post.IsEdited);
				Assert.Equal("dude", post.LastEditName);
			}

			[Fact]
			public void EditPostModeratorLogged()
			{
				var service = GetService();
				var user = new User { UserID = 123, Name = "dude" };
				_textParser.Setup(t => t.ClientHtmlToHtml("blah")).Returns("new");
				_textParser.Setup(t => t.Censor("unparsed title")).Returns("new title");
				service.EditPost(new Post { PostID = 456, ShowSig = false, FullText = "old text" }, new PostEdit { FullText = "blah", Title = "unparsed title", IsPlainText = false, ShowSig = true, Comment = "mah comment" }, user);
				_moderationLogService.Verify(m => m.LogPost(user, ModerationType.PostEdit, It.IsAny<Post>(), "mah comment", "old text"), Times.Exactly(1));
			}

			[Fact]
			public void EditPostQueuesTopicForIndexing()
			{
				var service = GetService();
				var user = new User { UserID = 123, Name = "dude" };
				var post = new Post { PostID = 456, ShowSig = false, FullText = "old text", TopicID = 999 };
				_tenantService.Setup(x => x.GetTenant()).Returns("");

				service.EditPost(post, new PostEdit { FullText = "blah", Title = "unparsed title", IsPlainText = false, ShowSig = true, Comment = "mah comment" }, user);

				_searchIndexQueueRepo.Verify(x => x.Enqueue(It.IsAny<SearchIndexPayload>()), Times.Once);
			}
		}
	}
}