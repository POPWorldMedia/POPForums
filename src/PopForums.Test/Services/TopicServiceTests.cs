using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Moq;
using Xunit;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;
using PopForums.Services;

namespace PopForums.Test.Services
{
	public class TopicServiceTests
	{
		private Mock<ISettingsManager> _settingsManager;
		private Mock<ITopicRepository> _topicRepo;
		private Mock<IPostRepository> _postRepo;
		private Mock<IModerationLogService> _modService;
		private Mock<IForumService> _forumService;
		private Mock<IEventPublisher> _eventPublisher;
		private Mock<ISearchRepository> _searchRepo;
		private Mock<IUserRepository> _userRepo;
		private Mock<ISearchIndexQueueRepository> _searchIndexQueueRepo;
		private Mock<ITenantService> _tenantService;

		private TopicService GetTopicService()
		{
			_settingsManager = new Mock<ISettingsManager>();
			_topicRepo = new Mock<ITopicRepository>();
			_postRepo = new Mock<IPostRepository>();
			_modService = new Mock<IModerationLogService>();
			_forumService = new Mock<IForumService>();
			_eventPublisher = new Mock<IEventPublisher>();
			_searchRepo = new Mock<ISearchRepository>();
			_userRepo = new Mock<IUserRepository>();
			_searchIndexQueueRepo = new Mock<ISearchIndexQueueRepository>();
			_tenantService = new Mock<ITenantService>();
			return new TopicService(_topicRepo.Object, _postRepo.Object, _settingsManager.Object, _modService.Object, _forumService.Object, _eventPublisher.Object, _searchRepo.Object, _userRepo.Object, _searchIndexQueueRepo.Object, _tenantService.Object);
		}

		private static User GetUser()
		{
			return new User { UserID = 123, Name = "Name", Email = "Email", IsApproved = true, AuthorizationKey = Guid.NewGuid(), Roles = new List<string>()};
		}

		[Fact]
		public async Task GetTopicsFromRepo()
		{
			var forum = new Forum { ForumID = 1, TopicCount = 3 };
			var topicService = GetTopicService();
			var repoTopics = new List<Topic>();
			var settings = new Settings {TopicsPerPage = 20};
			_topicRepo.Setup(t => t.Get(1, true, 1, settings.TopicsPerPage)).ReturnsAsync(repoTopics);
			_settingsManager.Setup(s => s.Current).Returns(settings);
			var (topics, _) = await topicService.GetTopics(forum, true, 1);
			Assert.Same(repoTopics, topics);
		}

		[Fact]
		public async Task GetTopicsStartRowCalcd()
		{
			var forum = new Forum { ForumID = 1, TopicCount = 300 };
			var topicService = GetTopicService();
			var settings = new Settings { TopicsPerPage = 20 };
			_settingsManager.Setup(s => s.Current).Returns(settings);
			await topicService.GetTopics(forum, false, 3);
			_topicRepo.Verify(t => t.Get(It.IsAny<int>(), It.IsAny<bool>(), 41, It.IsAny<int>()), Times.Once());
		}

		[Fact]
		public async Task GetTopicsIncludeDeletedCallsRepoCount()
		{
			var forum = new Forum { ForumID = 1 };
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<Topic>());
			_topicRepo.Setup(t => t.GetTopicCount(1, true)).ReturnsAsync(350);
			_settingsManager.Setup(s => s.Current).Returns(new Settings());
			await topicService.GetTopics(forum, true, 3);
			_topicRepo.Verify(t => t.GetTopicCount(forum.ForumID, true), Times.Once());
		}

		[Fact]
		public async Task GetTopicsNotIncludeDeletedNotCallRepoCount()
		{
			var forum = new Forum { ForumID = 1 };
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<Topic>());
			_settingsManager.Setup(s => s.Current).Returns(new Settings());
			await topicService.GetTopics(forum, false, 3);
			_topicRepo.Verify(t => t.GetTopicCount(forum.ForumID, false), Times.Never());
		}

		[Fact]
		public async Task GetTopicsPagerContextIncludesPageIndexAndCalcdTotalPages()
		{
			var forum = new Forum {ForumID = 1, TopicCount = 301};
			var forum2 = new Forum {ForumID = 2, TopicCount = 300};
			var forum3 = new Forum {ForumID = 3, TopicCount = 299};
			var topicService = GetTopicService();
			var settings = new Settings { TopicsPerPage = 20 };
			_topicRepo.Setup(t => t.Get(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), settings.TopicsPerPage)).ReturnsAsync(new List<Topic>());
			_settingsManager.Setup(s => s.Current).Returns(settings);
			var (_, pagerContext) = await topicService.GetTopics(forum, false, 3);
			Assert.Equal(3, pagerContext.PageIndex);
			Assert.Equal(16, pagerContext.PageCount);
			(_, pagerContext) = await topicService.GetTopics(forum2, false, 4);
			Assert.Equal(4, pagerContext.PageIndex);
			Assert.Equal(15, pagerContext.PageCount);
			(_, pagerContext) = await topicService.GetTopics(forum3, false, 5);
			Assert.Equal(5, pagerContext.PageIndex);
			Assert.Equal(15, pagerContext.PageCount);
			Assert.Equal(settings.TopicsPerPage, pagerContext.PageSize);
		}

		[Fact]
		public async Task CloseTopicThrowsWithNonMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var topicService = GetTopicService();
			await Assert.ThrowsAsync<InvalidOperationException>(async () => await topicService.CloseTopic(topic, user));
		}

		[Fact]
		public async Task CloseTopicClosesWithMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			await topicService.CloseTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicClose, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.CloseTopic(topic.TopicID), Times.Exactly(1));
		}

		[Fact]
		public async Task OpenTopicThrowsWithNonMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var topicService = GetTopicService();
			await Assert.ThrowsAsync<InvalidOperationException>(async () => await topicService.OpenTopic(topic, user));
		}

		[Fact]
		public async Task OpenTopicOpensWithMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			await topicService.OpenTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicOpen, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.OpenTopic(topic.TopicID), Times.Exactly(1));
		}

		[Fact]
		public async Task PinTopicThrowsWithNonMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var topicService = GetTopicService();
			await Assert.ThrowsAsync<InvalidOperationException>(async () => await topicService.PinTopic(topic, user));
		}

		[Fact]
		public async Task PinTopicPinsWithMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			await topicService.PinTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicPin, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.PinTopic(topic.TopicID), Times.Exactly(1));
		}

		[Fact]
		public async Task UnpinTopicThrowsWithNonMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var topicService = GetTopicService();
			await Assert.ThrowsAsync<InvalidOperationException>(async () => await topicService.UnpinTopic(topic, user));
		}

		[Fact]
		public async Task UnpinTopicUnpinsWithMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			await topicService.UnpinTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicUnpin, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.UnpinTopic(topic.TopicID), Times.Exactly(1));
		}

		[Fact]
		public async Task DeleteTopicThrowsWithNonMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var topicService = GetTopicService();
			await Assert.ThrowsAsync<InvalidOperationException>(async () => await topicService.DeleteTopic(topic, user));
		}

		[Fact]
		public async Task DeleteTopicDeletesWithMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			await topicService.DeleteTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicDelete, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.DeleteTopic(topic.TopicID), Times.Exactly(1));
		}

		[Fact]
		public async Task DeleteTopicUpdatesCounts()
		{
			var topic = new Topic { TopicID = 1, ForumID = 123 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			var forum = new Forum { ForumID = topic.ForumID };
			_forumService.Setup(f => f.Get(topic.ForumID)).ReturnsAsync(forum);
			await topicService.DeleteTopic(topic, user);
			_forumService.Verify(f => f.UpdateCounts(forum), Times.Exactly(1));
		}

		[Fact]
		public async Task DeleteTopicQueuesIndexRemoval()
		{
			var topic = new Topic { TopicID = 1, ForumID = 123 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			var forum = new Forum { ForumID = topic.ForumID };
			_forumService.Setup(f => f.Get(topic.ForumID)).ReturnsAsync(forum);
			SearchIndexPayload payload = null;
			_searchIndexQueueRepo.Setup(x => x.Enqueue(It.IsAny<SearchIndexPayload>())).Callback<SearchIndexPayload>(x => payload = x);
			_tenantService.Setup(x => x.GetTenant()).Returns("t");

			await topicService.DeleteTopic(topic, user);

			Assert.Equal(topic.TopicID, payload.TopicID);
			Assert.Equal("t", payload.TenantID);
			Assert.True(payload.IsForRemoval);
		}

		[Fact]
		public async Task DeleteTopicUpdatesLast()
		{
			var topic = new Topic { TopicID = 1, ForumID = 123 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			var forum = new Forum { ForumID = topic.ForumID };
			_forumService.Setup(f => f.Get(topic.ForumID)).ReturnsAsync(forum);
			await topicService.DeleteTopic(topic, user);
			_forumService.Verify(f => f.UpdateLast(forum), Times.Exactly(1));
		}

		[Fact]
		public async Task DeleteTopicUpdatesReplyCount()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_postRepo.Setup(t => t.GetReplyCount(topic.TopicID, false)).ReturnsAsync(42);
			await topicService.DeleteTopic(topic, user);
			_topicRepo.Verify(t => t.UpdateReplyCount(topic.TopicID, 42), Times.Exactly(1));
		}

		[Fact]
		public async Task DeleteTopicDeletesWithStarter()
		{
			var user = GetUser();
			var topic = new Topic { TopicID = 1, StartedByUserID = user.UserID };
			var topicService = GetTopicService();
			await topicService.DeleteTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicDelete, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.DeleteTopic(topic.TopicID), Times.Exactly(1));
		}

		[Fact]
		public async Task UndeleteTopicThrowsWithNonMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var topicService = GetTopicService();
			await Assert.ThrowsAsync<InvalidOperationException>(async () => await topicService.UndeleteTopic(topic, user));
		}

		[Fact]
		public async Task UndeleteTopicUndeletesWithMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			await topicService.UndeleteTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicUndelete, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.UndeleteTopic(topic.TopicID), Times.Exactly(1));
		}

		[Fact]
		public async Task UndeleteTopicUpdatesCounts()
		{
			var topic = new Topic { TopicID = 1, ForumID = 123 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			var forum = new Forum { ForumID = topic.ForumID };
			_forumService.Setup(f => f.Get(topic.ForumID)).ReturnsAsync(forum);
			await topicService.UndeleteTopic(topic, user);
			_forumService.Verify(f => f.UpdateCounts(forum), Times.Exactly(1));
		}

		[Fact]
		public async Task UndeleteTopicUpdatesLast()
		{
			var topic = new Topic { TopicID = 1, ForumID = 123 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			var forum = new Forum { ForumID = topic.ForumID };
			_forumService.Setup(f => f.Get(topic.ForumID)).ReturnsAsync(forum);
			await topicService.UndeleteTopic(topic, user);
			_forumService.Verify(f => f.UpdateLast(forum), Times.Exactly(1));
		}

		[Fact]
		public async Task UndeleteQueuesReindex()
		{
			var topic = new Topic { TopicID = 1, ForumID = 123 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			var forum = new Forum { ForumID = topic.ForumID };
			_forumService.Setup(f => f.Get(topic.ForumID)).ReturnsAsync(forum);
			SearchIndexPayload payload = null;
			_searchIndexQueueRepo.Setup(x => x.Enqueue(It.IsAny<SearchIndexPayload>())).Callback<SearchIndexPayload>(x => payload = x);
			_tenantService.Setup(x => x.GetTenant()).Returns("t");

			await topicService.UndeleteTopic(topic, user);

			Assert.Equal(topic.TopicID, payload.TopicID);
			Assert.Equal("t", payload.TenantID);
			Assert.False(payload.IsForRemoval);
		}

		[Fact]
		public async Task UndeleteTopicUpdatesReplyCount()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_postRepo.Setup(t => t.GetReplyCount(topic.TopicID, false)).ReturnsAsync(42);
			await topicService.UndeleteTopic(topic, user);
			_topicRepo.Verify(t => t.UpdateReplyCount(topic.TopicID, 42), Times.Exactly(1));
		}

		[Fact]
		public async Task UpdateTopicThrowsWithNonMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var topicService = GetTopicService();
			await Assert.ThrowsAsync<InvalidOperationException>(async () => await topicService.UpdateTitleAndForum(topic, new Forum { ForumID = 2 }, "blah", user));
		}

		[Fact]
		public async Task UpdateTopicUpdatesTitleWithMod()
		{
			var forum = new Forum { ForumID = 2 };
			var topic = new Topic { TopicID = 1, ForumID = forum.ForumID };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(topic.TopicID)).ReturnsAsync(new Topic { TopicID = 1, ForumID = 2 });
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).ReturnsAsync(new List<string>());
			await topicService.UpdateTitleAndForum(topic, forum, "new title", user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicRenamed, topic, forum, It.IsAny<string>()), Times.Exactly(1));
			_topicRepo.Verify(t => t.UpdateTitleAndForum(topic.TopicID, forum.ForumID, "new title", "new-title"), Times.Exactly(1));
		}

		[Fact]
		public async Task UpdateTopicQueuesTopicForIndexingWithMod()
		{
			var forum = new Forum { ForumID = 2 };
			var topic = new Topic { TopicID = 1, ForumID = forum.ForumID };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(topic.TopicID)).ReturnsAsync(new Topic { TopicID = 1, ForumID = 2 });
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).ReturnsAsync(new List<string>());
			_tenantService.Setup(x => x.GetTenant()).Returns("");
			await topicService.UpdateTitleAndForum(topic, forum, "new title", user);
			_searchIndexQueueRepo.Verify(x => x.Enqueue(It.IsAny<SearchIndexPayload>()), Times.Once);
		}

		[Fact]
		public async Task UpdateTopicMovesTopicWithMod()
		{
			var forum = new Forum { ForumID = 2 };
			var topic = new Topic { TopicID = 1, ForumID = 7, Title = String.Empty };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(topic.TopicID)).ReturnsAsync(new Topic { TopicID = 1, ForumID = 3 });
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).ReturnsAsync(new List<string>());
			await topicService.UpdateTitleAndForum(topic, forum, string.Empty, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicMoved, topic, forum, It.IsAny<string>()), Times.Exactly(1));
			_topicRepo.Verify(t => t.UpdateTitleAndForum(topic.TopicID, forum.ForumID, String.Empty, String.Empty), Times.Exactly(1));
		}

		[Fact]
		public async Task UpdateTopicWithNewTitleChangesUrlNameOnTopicParameter()
		{
			var forum = new Forum { ForumID = 2 };
			var topic = new Topic { TopicID = 1, ForumID = forum.ForumID, UrlName = "old" };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).ReturnsAsync(new List<string>());
			_topicRepo.Setup(t => t.Get(topic.TopicID)).ReturnsAsync(new Topic { TopicID = 1, ForumID = 2 });
			await topicService.UpdateTitleAndForum(topic, forum, "new title", user);
			Assert.Equal("new-title", topic.UrlName);
		}

		[Fact]
		public async Task UpdateTopicMovesUpdatesCountAndLastOnOldForum()
		{
			var forum = new Forum { ForumID = 2 };
			var oldForum = new Forum { ForumID = 3 };
			var topic = new Topic { TopicID = 1, ForumID = 7, Title = String.Empty };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(topic.TopicID)).ReturnsAsync(new Topic { TopicID = 1, ForumID = oldForum.ForumID });
			_forumService.Setup(f => f.Get(oldForum.ForumID)).ReturnsAsync(oldForum);
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).ReturnsAsync(new List<string>());
			await topicService.UpdateTitleAndForum(topic, forum, String.Empty, user);
			_forumService.Verify(f => f.UpdateCounts(forum), Times.Exactly(1));
			_forumService.Verify(f => f.UpdateLast(forum), Times.Exactly(1));
		}

		[Fact]
		public async Task UpdateTopicMovesUpdatesCountAndLastOnNewForum()
		{
			var forum = new Forum { ForumID = 2 };
			var oldForum = new Forum { ForumID = 3 };
			var topic = new Topic { TopicID = 1, ForumID = 7, Title = String.Empty };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(topic.TopicID)).ReturnsAsync(new Topic { TopicID = 1, ForumID = oldForum.ForumID });
			_forumService.Setup(f => f.Get(oldForum.ForumID)).ReturnsAsync(oldForum);
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).ReturnsAsync(new List<string>());
			await topicService.UpdateTitleAndForum(topic, forum, String.Empty, user);
			_forumService.Verify(f => f.UpdateCounts(oldForum), Times.Exactly(1));
			_forumService.Verify(f => f.UpdateLast(oldForum), Times.Exactly(1));
		}

		[Fact]
		public async Task UpdateLastSetsFieldsFromLastPost()
		{
			var topic = new Topic { TopicID = 456 };
			var post = new Post { PostID = 123, TopicID = topic.TopicID, UserID = 789, Name = "Dude", PostTime = new DateTime(2000, 1, 3)};
			var service = GetTopicService();
			_postRepo.Setup(x => x.GetLastInTopic(post.TopicID)).ReturnsAsync(post);
			await service.UpdateLast(topic);
			_topicRepo.Verify(x => x.UpdateLastTimeAndUser(topic.TopicID, post.UserID, post.Name, post.PostTime), Times.Once());
		}

		[Fact]
		public async Task HardDeleteThrowsIfUserNotAdmin()
		{
			var user = new User { UserID = 123, Roles = new List<string>() };
			var topic = new Topic { TopicID = 45 };
			var service = GetTopicService();
			await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.HardDeleteTopic(topic, user));
		}

		[Fact]
		public async Task HardDeleteCallsModerationService()
		{
			var user = new User { UserID = 123, Roles = new List<string> { "Admin" } };
			var topic = new Topic { TopicID = 45 };
			var service = GetTopicService();
			await service.HardDeleteTopic(topic, user);
			_modService.Verify(x => x.LogTopic(user, ModerationType.TopicDeletePermanently, topic, null), Times.Once());
		}

		[Fact]
		public async Task HardDeleteCallsSearchIndexRepo()
		{
			var user = new User { UserID = 123, Roles = new List<string> { "Admin" } };
			var topic = new Topic { TopicID = 45 };
			var service = GetTopicService();
			SearchIndexPayload payload = null;
			_searchIndexQueueRepo.Setup(x => x.Enqueue(It.IsAny<SearchIndexPayload>())).Callback<SearchIndexPayload>(x => payload = x);
			_tenantService.Setup(x => x.GetTenant()).Returns("t");

			await service.HardDeleteTopic(topic, user);

			_searchIndexQueueRepo.Verify(x => x.Enqueue(It.IsAny<SearchIndexPayload>()), Times.Once);
			Assert.Equal(topic.TopicID, payload.TopicID);
			Assert.Equal("t", payload.TenantID);
			Assert.True(payload.IsForRemoval);
		}

		[Fact]
		public async Task HardDeleteCallsTopiRepoToDeleteTopic()
		{
			var user = new User { UserID = 123, Roles = new List<string> { "Admin" } };
			var topic = new Topic { TopicID = 45 };
			var service = GetTopicService();
			await service.HardDeleteTopic(topic, user);
			_topicRepo.Verify(x => x.HardDeleteTopic(topic.TopicID), Times.Once());
		}

		[Fact]
		public async Task HardDeleteCallsForumServiceToUpdateLastAndCounts()
		{
			var user = new User { UserID = 123, Roles = new List<string> { "Admin" } };
			var topic = new Topic { TopicID = 45, ForumID = 67};
			var forum = new Forum { ForumID = topic.ForumID };
			var service = GetTopicService();
			_forumService.Setup(x => x.Get(topic.ForumID)).ReturnsAsync(forum);
			await service.HardDeleteTopic(topic, user);
			_forumService.Verify(x => x.UpdateCounts(forum), Times.Once());
			_forumService.Verify(x => x.UpdateLast(forum), Times.Once());
		}

		[Fact]
		public async Task SetAnswerThrowsWhenUserNotTopicStarter()
		{
			var service = GetTopicService();
			var user = new User { UserID = 123 };
			var topic = new Topic { TopicID = 456, StartedByUserID = 789 };
			await Assert.ThrowsAsync<SecurityException>(async () => await service.SetAnswer(user, topic, new Post { PostID = 789 }, "", ""));
		}

		[Fact]
		public async Task SetAnswerThrowsIfPostIDOfAnswerDoesntExist()
		{
			var service = GetTopicService();
			var user = new User { UserID = 123 };
			var topic = new Topic { TopicID = 456, StartedByUserID = 123 };
			_postRepo.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync((Post) null);
			await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.SetAnswer(user, topic, new Post { PostID = 789 }, "", ""));
		}

		[Fact]
		public async Task SetAnswerThrowsIfPostIsNotPartOfTopic()
		{
			var service = GetTopicService();
			var user = new User { UserID = 123 };
			var topic = new Topic { TopicID = 456, StartedByUserID = 123 };
			var post = new Post { PostID = 789, TopicID = 111 };
			_postRepo.Setup(x => x.Get(post.PostID)).ReturnsAsync(post);
			await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.SetAnswer(user, topic, post, "", ""));
		}

		[Fact]
		public async Task SetAnswerCallsTopicRepoWithUpdatedValue()
		{
			var service = GetTopicService();
			var user = new User { UserID = 123 };
			var topic = new Topic { TopicID = 456, StartedByUserID = 123 };
			var post = new Post { PostID = 789, TopicID = topic.TopicID };
			_postRepo.Setup(x => x.Get(post.PostID)).ReturnsAsync(post);
			await service.SetAnswer(user, topic, post, "", "");
			_topicRepo.Verify(x => x.UpdateAnswerPostID(topic.TopicID, post.PostID), Times.Once());
		}

		[Fact]
		public async Task SetAnswerCallsEventPubWhenThereIsNoPreviousAnswerOnTheTopic()
		{
			var service = GetTopicService();
			var user = new User { UserID = 123 };
			var answerUser = new User { UserID = 777 };
			var topic = new Topic { TopicID = 456, StartedByUserID = user.UserID, AnswerPostID = null};
			var post = new Post { PostID = 789, TopicID = topic.TopicID, UserID = answerUser.UserID};
			_postRepo.Setup(x => x.Get(post.PostID)).ReturnsAsync(post);
			_userRepo.Setup(x => x.GetUser(answerUser.UserID)).ReturnsAsync(answerUser);
			await service.SetAnswer(user, topic, post, "", "");
			_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), answerUser, EventDefinitionService.StaticEventIDs.QuestionAnswered, false), Times.Once());
		}

		[Fact]
		public async Task SetAnswerDoesNotCallEventPubWhenTheAnswerUserDoesNotExist()
		{
			var service = GetTopicService();
			var user = new User { UserID = 123 };
			var topic = new Topic { TopicID = 456, StartedByUserID = user.UserID, AnswerPostID = null };
			var post = new Post { PostID = 789, TopicID = topic.TopicID, UserID = 777 };
			_postRepo.Setup(x => x.Get(post.PostID)).ReturnsAsync(post);
			_userRepo.Setup(x => x.GetUser(It.IsAny<int>())).ReturnsAsync((User)null);
			await service.SetAnswer(user, topic, post, "", "");
			_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
		}

		[Fact]
		public async Task SetAnswerDoesNotCallEventPubWhenTheTopicAlreadyHasAnAnswer()
		{
			var service = GetTopicService();
			var user = new User { UserID = 123 };
			var answerUser = new User { UserID = 777 };
			var topic = new Topic { TopicID = 456, StartedByUserID = user.UserID, AnswerPostID = 666 };
			var post = new Post { PostID = 789, TopicID = topic.TopicID, UserID = answerUser.UserID };
			_postRepo.Setup(x => x.Get(post.PostID)).ReturnsAsync(post);
			_userRepo.Setup(x => x.GetUser(answerUser.UserID)).ReturnsAsync(answerUser);
			await service.SetAnswer(user, topic, post, "", "");
			_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
		}

		[Fact]
		public async Task SetAnswerDoesNotCallEventPubWhenTopicUserIDIsSameAsAnswerUserID()
		{
			var service = GetTopicService();
			var user = new User { UserID = 123 };
			var topic = new Topic { TopicID = 456, StartedByUserID = user.UserID, AnswerPostID = null };
			var post = new Post { PostID = 789, TopicID = topic.TopicID, UserID = user.UserID };
			_postRepo.Setup(x => x.Get(post.PostID)).ReturnsAsync(post);
			_userRepo.Setup(x => x.GetUser(user.UserID)).ReturnsAsync(user);
			await service.SetAnswer(user, topic, post, "", "");
			_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
		}

		[Fact]
		public async Task CloseAgedTopicsDoesNothingWhenSettingIsOff()
		{
			var service = GetTopicService();
			_settingsManager.Setup(x => x.Current.IsClosingAgedTopics).Returns(false);

			await service.CloseAgedTopics();

			_topicRepo.Verify(x => x.CloseTopicsOlderThan(It.IsAny<DateTime>()), Times.Never);
		}

		[Fact]
		public async Task CloseAgedTopicsCallsRepoWhenSettingIsOn()
		{
			var service = GetTopicService();
			_settingsManager.Setup(x => x.Current.IsClosingAgedTopics).Returns(true);
			_topicRepo.Setup(x => x.CloseTopicsOlderThan(It.IsAny<DateTime>())).ReturnsAsync(new List<int>());

			await service.CloseAgedTopics();

			_topicRepo.Verify(x => x.CloseTopicsOlderThan(It.IsAny<DateTime>()), Times.Once);
		}

		[Fact]
		public async Task CloseAgedTopicsLogsTopicModeration()
		{
			var service = GetTopicService();
			_settingsManager.Setup(x => x.Current.IsClosingAgedTopics).Returns(true);
			_topicRepo.Setup(x => x.CloseTopicsOlderThan(It.IsAny<DateTime>())).ReturnsAsync(new List<int>{1,2,3});

			await service.CloseAgedTopics();

			_topicRepo.Verify(x => x.CloseTopicsOlderThan(It.IsAny<DateTime>()), Times.Once);
			_modService.Verify(x => x.LogTopic(ModerationType.TopicCloseAuto, 1), Times.Once);
			_modService.Verify(x => x.LogTopic(ModerationType.TopicCloseAuto, 2), Times.Once);
			_modService.Verify(x => x.LogTopic(ModerationType.TopicCloseAuto, 3), Times.Once);
		}
	}
}
