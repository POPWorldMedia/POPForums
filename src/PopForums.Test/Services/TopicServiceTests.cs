using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Moq;
using Xunit;
using PopForums.Configuration;
using PopForums.Messaging;
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
			return new User { UserID = 123, Name = "Name", Email = "Email", IsApproved = true, LastActivityDate = DateTime.MaxValue, LastLoginDate = DateTime.MaxValue, AuthorizationKey = Guid.NewGuid(), Roles = new List<string>()};
		}

		[Fact]
		public void GetTopicsFromRepo()
		{
			var forum = new Forum { ForumID = 1, TopicCount = 3 };
			var topicService = GetTopicService();
			var repoTopics = new List<Topic>();
			var settings = new Settings {TopicsPerPage = 20};
			_topicRepo.Setup(t => t.Get(1, true, 1, settings.TopicsPerPage)).Returns(repoTopics);
			_settingsManager.Setup(s => s.Current).Returns(settings);
			PagerContext pagerContext;
			var topics = topicService.GetTopics(forum, true, 1, out pagerContext);
			Assert.Same(repoTopics, topics);
		}

		[Fact]
		public void GetTopicsStartRowCalcd()
		{
			var forum = new Forum { ForumID = 1, TopicCount = 300 };
			var topicService = GetTopicService();
			var settings = new Settings { TopicsPerPage = 20 };
			_settingsManager.Setup(s => s.Current).Returns(settings);
			PagerContext pagerContext;
			topicService.GetTopics(forum, false, 3, out pagerContext);
			_topicRepo.Verify(t => t.Get(It.IsAny<int>(), It.IsAny<bool>(), 41, It.IsAny<int>()), Times.Once());
		}

		[Fact]
		public void GetTopicsIncludeDeletedCallsRepoCount()
		{
			var forum = new Forum { ForumID = 1 };
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<Topic>());
			_topicRepo.Setup(t => t.GetTopicCount(1, true)).Returns(350);
			_settingsManager.Setup(s => s.Current).Returns(new Settings());
			PagerContext pagerContext;
			topicService.GetTopics(forum, true, 3, out pagerContext);
			_topicRepo.Verify(t => t.GetTopicCount(forum.ForumID, true), Times.Once());
		}

		[Fact]
		public void GetTopicsNotIncludeDeletedNotCallRepoCount()
		{
			var forum = new Forum { ForumID = 1 };
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<Topic>());
			_settingsManager.Setup(s => s.Current).Returns(new Settings());
			PagerContext pagerContext;
			topicService.GetTopics(forum, false, 3, out pagerContext);
			_topicRepo.Verify(t => t.GetTopicCount(forum.ForumID, false), Times.Never());
		}

		[Fact]
		public void GetTopicsPagerContextIncludesPageIndexAndCalcdTotalPages()
		{
			var forum = new Forum {ForumID = 1, TopicCount = 301};
			var forum2 = new Forum {ForumID = 2, TopicCount = 300};
			var forum3 = new Forum {ForumID = 3, TopicCount = 299};
			var topicService = GetTopicService();
			var settings = new Settings { TopicsPerPage = 20 };
			_topicRepo.Setup(t => t.Get(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), settings.TopicsPerPage)).Returns(new List<Topic>());
			_settingsManager.Setup(s => s.Current).Returns(settings);
			PagerContext pagerContext;
			topicService.GetTopics(forum, false, 3, out pagerContext);
			Assert.Equal(3, pagerContext.PageIndex);
			Assert.Equal(16, pagerContext.PageCount);
			topicService.GetTopics(forum2, false, 4, out pagerContext);
			Assert.Equal(4, pagerContext.PageIndex);
			Assert.Equal(15, pagerContext.PageCount);
			topicService.GetTopics(forum3, false, 5, out pagerContext);
			Assert.Equal(5, pagerContext.PageIndex);
			Assert.Equal(15, pagerContext.PageCount);
			Assert.Equal(settings.TopicsPerPage, pagerContext.PageSize);
		}

		[Fact]
		public void CloseTopicThrowsWithNonMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var topicService = GetTopicService();
			Assert.Throws<InvalidOperationException>(() => topicService.CloseTopic(topic, user));
		}

		[Fact]
		public void CloseTopicClosesWithMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			topicService.CloseTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicClose, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.CloseTopic(topic.TopicID), Times.Exactly(1));
		}

		[Fact]
		public void OpenTopicThrowsWithNonMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var topicService = GetTopicService();
			Assert.Throws<InvalidOperationException>(() => topicService.OpenTopic(topic, user));
		}

		[Fact]
		public void OpenTopicOpensWithMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			topicService.OpenTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicOpen, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.OpenTopic(topic.TopicID), Times.Exactly(1));
		}

		[Fact]
		public void PinTopicThrowsWithNonMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var topicService = GetTopicService();
			Assert.Throws<InvalidOperationException>(() => topicService.PinTopic(topic, user));
		}

		[Fact]
		public void PinTopicPinsWithMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			topicService.PinTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicPin, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.PinTopic(topic.TopicID), Times.Exactly(1));
		}

		[Fact]
		public void UnpinTopicThrowsWithNonMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var topicService = GetTopicService();
			Assert.Throws<InvalidOperationException>(() => topicService.UnpinTopic(topic, user));
		}

		[Fact]
		public void UnpinTopicUnpinsWithMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			topicService.UnpinTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicUnpin, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.UnpinTopic(topic.TopicID), Times.Exactly(1));
		}

		[Fact]
		public void DeleteTopicThrowsWithNonMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var topicService = GetTopicService();
			Assert.Throws<InvalidOperationException>(() => topicService.DeleteTopic(topic, user));
		}

		[Fact]
		public void DeleteTopicDeletesWithMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			topicService.DeleteTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicDelete, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.DeleteTopic(topic.TopicID), Times.Exactly(1));
		}

		[Fact]
		public void DeleteTopicUpdatesCounts()
		{
			var topic = new Topic { TopicID = 1, ForumID = 123 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			var forum = new Forum { ForumID = topic.ForumID };
			_forumService.Setup(f => f.Get(topic.ForumID)).Returns(forum);
			topicService.DeleteTopic(topic, user);
			_forumService.Verify(f => f.UpdateCounts(forum), Times.Exactly(1));
		}

		[Fact]
		public void DeleteTopicUpdatesLast()
		{
			var topic = new Topic { TopicID = 1, ForumID = 123 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			var forum = new Forum { ForumID = topic.ForumID };
			_forumService.Setup(f => f.Get(topic.ForumID)).Returns(forum);
			topicService.DeleteTopic(topic, user);
			_forumService.Verify(f => f.UpdateLast(forum), Times.Exactly(1));
		}

		[Fact]
		public void DeleteTopicUpdatesReplyCount()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_postRepo.Setup(t => t.GetReplyCount(topic.TopicID, false)).Returns(42);
			topicService.DeleteTopic(topic, user);
			_topicRepo.Verify(t => t.UpdateReplyCount(topic.TopicID, 42), Times.Exactly(1));
		}

		[Fact]
		public void DeleteTopicDeletesWithStarter()
		{
			var user = GetUser();
			var topic = new Topic { TopicID = 1, StartedByUserID = user.UserID };
			var topicService = GetTopicService();
			topicService.DeleteTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicDelete, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.DeleteTopic(topic.TopicID), Times.Exactly(1));
		}

		[Fact]
		public void UndeleteTopicThrowsWithNonMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var topicService = GetTopicService();
			Assert.Throws<InvalidOperationException>(() => topicService.UndeleteTopic(topic, user));
		}

		[Fact]
		public void UndeleteTopicUndeletesWithMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			topicService.UndeleteTopic(topic, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicUndelete, topic, null), Times.Exactly(1));
			_topicRepo.Verify(t => t.UndeleteTopic(topic.TopicID), Times.Exactly(1));
		}

		[Fact]
		public void UndeleteTopicUpdatesCounts()
		{
			var topic = new Topic { TopicID = 1, ForumID = 123 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			var forum = new Forum { ForumID = topic.ForumID };
			_forumService.Setup(f => f.Get(topic.ForumID)).Returns(forum);
			topicService.UndeleteTopic(topic, user);
			_forumService.Verify(f => f.UpdateCounts(forum), Times.Exactly(1));
		}

		[Fact]
		public void UndeleteTopicUpdatesLast()
		{
			var topic = new Topic { TopicID = 1, ForumID = 123 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			var forum = new Forum { ForumID = topic.ForumID };
			_forumService.Setup(f => f.Get(topic.ForumID)).Returns(forum);
			topicService.UndeleteTopic(topic, user);
			_forumService.Verify(f => f.UpdateLast(forum), Times.Exactly(1));
		}

		[Fact]
		public void UndeleteTopicUpdatesReplyCount()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_postRepo.Setup(t => t.GetReplyCount(topic.TopicID, false)).Returns(42);
			topicService.UndeleteTopic(topic, user);
			_topicRepo.Verify(t => t.UpdateReplyCount(topic.TopicID, 42), Times.Exactly(1));
		}

		[Fact]
		public void UpdateTopicThrowsWithNonMod()
		{
			var topic = new Topic { TopicID = 1 };
			var user = GetUser();
			var topicService = GetTopicService();
			Assert.Throws<InvalidOperationException>(() => topicService.UpdateTitleAndForum(topic, new Forum { ForumID = 2 }, "blah", user));
		}

		[Fact]
		public void UpdateTopicUpdatesTitleWithMod()
		{
			var forum = new Forum { ForumID = 2 };
			var topic = new Topic { TopicID = 1, ForumID = forum.ForumID };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(topic.TopicID)).Returns(new Topic { TopicID = 1, ForumID = 2 });
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).Returns(new List<string>());
			topicService.UpdateTitleAndForum(topic, forum, "new title", user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicRenamed, topic, forum, It.IsAny<string>()), Times.Exactly(1));
			_topicRepo.Verify(t => t.UpdateTitleAndForum(topic.TopicID, forum.ForumID, "new title", "new-title"), Times.Exactly(1));
		}

		[Fact]
		public void UpdateTopicQueuesTopicForIndexingWithMod()
		{
			var forum = new Forum { ForumID = 2 };
			var topic = new Topic { TopicID = 1, ForumID = forum.ForumID };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(topic.TopicID)).Returns(new Topic { TopicID = 1, ForumID = 2 });
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).Returns(new List<string>());
			_tenantService.Setup(x => x.GetTenant()).Returns("");
			topicService.UpdateTitleAndForum(topic, forum, "new title", user);
			_searchIndexQueueRepo.Verify(x => x.Enqueue(It.IsAny<SearchIndexPayload>()), Times.Once);
		}

		[Fact]
		public void UpdateTopicMovesTopicWithMod()
		{
			var forum = new Forum { ForumID = 2 };
			var topic = new Topic { TopicID = 1, ForumID = 7, Title = String.Empty };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(topic.TopicID)).Returns(new Topic { TopicID = 1, ForumID = 3 });
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).Returns(new List<string>());
			topicService.UpdateTitleAndForum(topic, forum, String.Empty, user);
			_modService.Verify(m => m.LogTopic(user, ModerationType.TopicMoved, topic, forum, It.IsAny<string>()), Times.Exactly(1));
			_topicRepo.Verify(t => t.UpdateTitleAndForum(topic.TopicID, forum.ForumID, String.Empty, String.Empty), Times.Exactly(1));
		}

		[Fact]
		public void UpdateTopicWithNewTitleChangesUrlNameOnTopicParameter()
		{
			var forum = new Forum { ForumID = 2 };
			var topic = new Topic { TopicID = 1, ForumID = forum.ForumID, UrlName = "old" };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).Returns(new List<string>());
			_topicRepo.Setup(t => t.Get(topic.TopicID)).Returns(new Topic { TopicID = 1, ForumID = 2 });
			topicService.UpdateTitleAndForum(topic, forum, "new title", user);
			Assert.Equal("new-title", topic.UrlName);
		}

		[Fact]
		public void UpdateTopicMovesUpdatesCountAndLastOnOldForum()
		{
			var forum = new Forum { ForumID = 2 };
			var oldForum = new Forum { ForumID = 3 };
			var topic = new Topic { TopicID = 1, ForumID = 7, Title = String.Empty };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(topic.TopicID)).Returns(new Topic { TopicID = 1, ForumID = oldForum.ForumID });
			_forumService.Setup(f => f.Get(oldForum.ForumID)).Returns(oldForum);
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).Returns(new List<string>());
			topicService.UpdateTitleAndForum(topic, forum, String.Empty, user);
			_forumService.Verify(f => f.UpdateCounts(forum), Times.Exactly(1));
			_forumService.Verify(f => f.UpdateLast(forum), Times.Exactly(1));
		}

		[Fact]
		public void UpdateTopicMovesUpdatesCountAndLastOnNewForum()
		{
			var forum = new Forum { ForumID = 2 };
			var oldForum = new Forum { ForumID = 3 };
			var topic = new Topic { TopicID = 1, ForumID = 7, Title = String.Empty };
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var topicService = GetTopicService();
			_topicRepo.Setup(t => t.Get(topic.TopicID)).Returns(new Topic { TopicID = 1, ForumID = oldForum.ForumID });
			_forumService.Setup(f => f.Get(oldForum.ForumID)).Returns(oldForum);
			_topicRepo.Setup(t => t.GetUrlNamesThatStartWith(It.IsAny<string>())).Returns(new List<string>());
			topicService.UpdateTitleAndForum(topic, forum, String.Empty, user);
			_forumService.Verify(f => f.UpdateCounts(oldForum), Times.Exactly(1));
			_forumService.Verify(f => f.UpdateLast(oldForum), Times.Exactly(1));
		}

		[Fact]
		public void UpdateLastSetsFieldsFromLastPost()
		{
			var topic = new Topic { TopicID = 456 };
			var post = new Post { PostID = 123, TopicID = topic.TopicID, UserID = 789, Name = "Dude", PostTime = new DateTime(2000, 1, 3)};
			var service = GetTopicService();
			_postRepo.Setup(x => x.GetLastInTopic(post.TopicID)).Returns(post);
			service.UpdateLast(topic);
			_topicRepo.Verify(x => x.UpdateLastTimeAndUser(topic.TopicID, post.UserID, post.Name, post.PostTime), Times.Once());
		}

		[Fact]
		public void HardDeleteThrowsIfUserNotAdmin()
		{
			var user = new User { UserID = 123, Roles = new List<string>() };
			var topic = new Topic { TopicID = 45 };
			var service = GetTopicService();
			Assert.Throws<InvalidOperationException>(() => service.HardDeleteTopic(topic, user));
		}

		[Fact]
		public void HardDeleteCallsModerationService()
		{
			var user = new User { UserID = 123, Roles = new List<string> { "Admin" } };
			var topic = new Topic { TopicID = 45 };
			var service = GetTopicService();
			service.HardDeleteTopic(topic, user);
			_modService.Verify(x => x.LogTopic(user, ModerationType.TopicDeletePermanently, topic, null), Times.Once());
		}

		[Fact]
		public void HardDeleteCallsSearchRepoToDeleteSearchWords()
		{
			var user = new User { UserID = 123, Roles = new List<string> { "Admin" } };
			var topic = new Topic { TopicID = 45 };
			var service = GetTopicService();
			service.HardDeleteTopic(topic, user);
			_searchRepo.Verify(x => x.DeleteAllIndexedWordsForTopic(topic.TopicID), Times.Once());
		}

		[Fact]
		public void HardDeleteCallsTopiRepoToDeleteTopic()
		{
			var user = new User { UserID = 123, Roles = new List<string> { "Admin" } };
			var topic = new Topic { TopicID = 45 };
			var service = GetTopicService();
			service.HardDeleteTopic(topic, user);
			_topicRepo.Verify(x => x.HardDeleteTopic(topic.TopicID), Times.Once());
		}

		[Fact]
		public void HardDeleteCallsForumServiceToUpdateLastAndCounts()
		{
			var user = new User { UserID = 123, Roles = new List<string> { "Admin" } };
			var topic = new Topic { TopicID = 45, ForumID = 67};
			var forum = new Forum { ForumID = topic.ForumID };
			var service = GetTopicService();
			_forumService.Setup(x => x.Get(topic.ForumID)).Returns(forum);
			service.HardDeleteTopic(topic, user);
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
			_postRepo.Setup(x => x.Get(It.IsAny<int>())).Returns((Post) null);
			await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.SetAnswer(user, topic, new Post { PostID = 789 }, "", ""));
		}

		[Fact]
		public async Task SetAnswerThrowsIfPostIsNotPartOfTopic()
		{
			var service = GetTopicService();
			var user = new User { UserID = 123 };
			var topic = new Topic { TopicID = 456, StartedByUserID = 123 };
			var post = new Post { PostID = 789, TopicID = 111 };
			_postRepo.Setup(x => x.Get(post.PostID)).Returns(post);
			await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.SetAnswer(user, topic, post, "", ""));
		}

		[Fact]
		public async Task SetAnswerCallsTopicRepoWithUpdatedValue()
		{
			var service = GetTopicService();
			var user = new User { UserID = 123 };
			var topic = new Topic { TopicID = 456, StartedByUserID = 123 };
			var post = new Post { PostID = 789, TopicID = topic.TopicID };
			_postRepo.Setup(x => x.Get(post.PostID)).Returns(post);
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
			_postRepo.Setup(x => x.Get(post.PostID)).Returns(post);
			_userRepo.Setup(x => x.GetUser(answerUser.UserID)).Returns(answerUser);
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
			_postRepo.Setup(x => x.Get(post.PostID)).Returns(post);
			_userRepo.Setup(x => x.GetUser(It.IsAny<int>())).Returns((User)null);
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
			_postRepo.Setup(x => x.Get(post.PostID)).Returns(post);
			_userRepo.Setup(x => x.GetUser(answerUser.UserID)).Returns(answerUser);
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
			_postRepo.Setup(x => x.Get(post.PostID)).Returns(post);
			_userRepo.Setup(x => x.GetUser(user.UserID)).Returns(user);
			await service.SetAnswer(user, topic, post, "", "");
			_eventPublisher.Verify(x => x.ProcessEvent(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
		}
	}
}
