using System;
using System.Collections.Generic;
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
	public class PostServiceTests
	{
		private Mock<IPostRepository> _postRepo;
		private Mock<IProfileRepository> _profileRepo;
		private Mock<ISettingsManager> _settingsManager;
		private Mock<Settings> _settings;
		private Mock<ITopicService> _topicService;
		private Mock<ITextParsingService> _textParsingService;
		private Mock<IModerationLogService> _modLogService;
		private Mock<IForumService> _forumService;
		private Mock<IEventPublisher> _eventPub;
		private Mock<IUserService> _userService;
		private Mock<ISearchIndexQueueRepository> _searchIndexQueue;
		private Mock<ITenantService> _tenantService;

		private PostService GetService()
		{
			_postRepo = new Mock<IPostRepository>();
			_profileRepo = new Mock<IProfileRepository>();
			_settingsManager = new Mock<ISettingsManager>();
			_settings = new Mock<Settings>();
			_topicService = new Mock<ITopicService>();
			_textParsingService = new Mock<ITextParsingService>();
			_modLogService = new Mock<IModerationLogService>();
			_forumService = new Mock<IForumService>();
			_eventPub = new Mock<IEventPublisher>();
			_userService = new Mock<IUserService>();
			_searchIndexQueue = new Mock<ISearchIndexQueueRepository>();
			_tenantService = new Mock<ITenantService>();
			_settingsManager.Setup(s => s.Current).Returns(_settings.Object);
			return new PostService(_postRepo.Object, _profileRepo.Object, _settingsManager.Object, _topicService.Object, _textParsingService.Object, _modLogService.Object, _forumService.Object, _eventPub.Object, _userService.Object, _searchIndexQueue.Object, _tenantService.Object);
		}

		[Fact]
		public async Task GetPostsPageSizeAndStartRowCalcdCorrectly()
		{
			var topic = new Topic { TopicID = 1, ReplyCount = 20};
			var postService = GetService();
			_settings.Setup(s => s.PostsPerPage).Returns(2);
			var (posts, pagerContext) = await postService.GetPosts(topic, false, 4);
			_postRepo.Verify(p => p.Get(1, false, 7, 2), Times.Once());
			_postRepo.Verify(p => p.GetReplyCount(It.IsAny<int>(), It.IsAny<bool>()), Times.Never());
			Assert.Equal(11, pagerContext.PageCount);
			Assert.Equal(2, pagerContext.PageSize);
		}

		[Fact]
		public async Task GetPostsReplyCountCalledOnIncludeDeleted()
		{
			var topic = new Topic { TopicID = 1, ReplyCount = 20 };
			var postService = GetService();
			_settings.Setup(s => s.PostsPerPage).Returns(2);
			_postRepo.Setup(p => p.GetReplyCount(topic.TopicID, true)).ReturnsAsync(21);
			var (posts, pagerContext) = await postService.GetPosts(topic, true, 4);
			_postRepo.Verify(p => p.GetReplyCount(topic.TopicID, true), Times.Once());
			Assert.Equal(11, pagerContext.PageCount);
		}

		[Fact]
		public async Task GetPostsPagerContextConstructed()
		{
			var topic = new Topic { TopicID = 1, ReplyCount = 20 };
			var postService = GetService();
			_settings.Setup(s => s.PostsPerPage).Returns(3);
			var (posts, pagerContext) = await postService.GetPosts(topic, false, 4);
			Assert.Equal(7, pagerContext.PageCount);
			Assert.Equal(4, pagerContext.PageIndex);
		}

		[Fact]
		public async Task GetPostsHitsRepo()
		{
			var topic = new Topic { TopicID = 1, ReplyCount = 20 };
			var posts = new List<Post>();
			var postService = GetService();
			_settings.Setup(s => s.PostsPerPage).Returns(3);
			_postRepo.Setup(p => p.Get(1, false, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(posts);
			var result = await postService.GetPosts(topic, false, 4);
			Assert.Same(posts, result.Item1);
		}

		[Fact]
		public async Task GetCallsRepoAndReturns()
		{
			var postService = GetService();
			var postID = 123;
			var post = new Post {PostID = postID};
			_postRepo.Setup(p => p.Get(postID)).ReturnsAsync(post);
			var postResult = await postService.Get(postID);
			_postRepo.Verify(p => p.Get(postID), Times.Once());
			Assert.Same(postResult, post);
		}

		[Fact]
		public async Task GetPostCountCallsRepo()
		{
			var postService = GetService();
			_postRepo.Setup(p => p.GetPostCount(123)).ReturnsAsync(456);
			var user = new User { UserID = 123 };
			var result = await postService.GetPostCount(user);
			_postRepo.Verify(p => p.GetPostCount(123), Times.Exactly(1));
			Assert.Equal(456, result);
		}

		[Fact]
		public async Task GetPostForEditPlainText()
		{
			var service = GetService();
			var post = new Post { PostID = 123, Title = "mah title", FullText = "not", ShowSig = true, IsFirstInTopic = true };
			var user = new User { UserID = 456 };
			_profileRepo.Setup(p => p.GetProfile(user.UserID)).ReturnsAsync(new Profile {IsPlainText = true});
			_textParsingService.Setup(p => p.HtmlToForumCode("not")).Returns("new text");
			var postEdit = await service.GetPostForEdit(post, user);
			Assert.Equal("mah title", postEdit.Title);
			Assert.Equal("new text", postEdit.FullText);
			Assert.True(postEdit.IsFirstInTopic);
			Assert.True(postEdit.ShowSig);
			Assert.True(postEdit.IsPlainText);
			_textParsingService.Verify(t => t.HtmlToForumCode("not"), Times.Exactly(1));
		}

		[Fact]
		public async Task GetPostForEditNotPlainText()
		{
			var service = GetService();
			var post = new Post { PostID = 123, Title = "mah title", FullText = "not", ShowSig = true, IsFirstInTopic = true };
			var user = new User { UserID = 456 };
			_profileRepo.Setup(p => p.GetProfile(user.UserID)).ReturnsAsync(new Profile { IsPlainText = false });
			_textParsingService.Setup(p => p.HtmlToClientHtml("not")).Returns("new text");
			var postEdit = await service.GetPostForEdit(post, user);
			Assert.Equal("mah title", postEdit.Title);
			Assert.Equal("new text", postEdit.FullText);
			Assert.True(postEdit.IsFirstInTopic);
			Assert.True(postEdit.ShowSig);
			Assert.False(postEdit.IsPlainText);
			_textParsingService.Verify(t => t.HtmlToClientHtml("not"), Times.Exactly(1));
		}

		[Fact]
		public async Task DeleteThrowsForNonAuthorAndNonMod()
		{
			var service = GetService();
			var user = new User { UserID = 123 };
			var post = new Post { PostID = 67 };
			await Assert.ThrowsAsync<Exception>(async () => await service.Delete(post, user));
		}

		[Fact]
		public async Task DeleteCallDeleteTopicIfFirstInTopic()
		{
			var forum = new Forum { ForumID = 5 };
			var topic = new Topic { TopicID = 4, ForumID = forum.ForumID };
			var service = GetService();
			var user = new User { UserID = 123 };
			var post = new Post { PostID = 67, UserID = user.UserID, IsFirstInTopic = true, TopicID = topic.TopicID };
			_topicService.Setup(t => t.Get(topic.TopicID)).ReturnsAsync(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).ReturnsAsync(forum);
			await service.Delete(post, user);
			_topicService.Verify(t => t.DeleteTopic(topic, user), Times.Exactly(1));
		}

		[Fact]
		public async Task DeleteCallLogs()
		{
			var forum = new Forum { ForumID = 5 };
			var topic = new Topic { TopicID = 4, ForumID = forum.ForumID };
			var service = GetService();
			var user = new User { UserID = 123 };
			var post = new Post { PostID = 67, UserID = user.UserID, IsFirstInTopic = false, TopicID = topic.TopicID };
			_topicService.Setup(t => t.Get(topic.TopicID)).ReturnsAsync(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).ReturnsAsync(forum);
			await service.Delete(post, user);
			_modLogService.Verify(m => m.LogPost(user, ModerationType.PostDelete, post, String.Empty, String.Empty), Times.Exactly(1));
		}

		[Fact]
		public async Task DeleteSetsEditFields()
		{
			var forum = new Forum { ForumID = 5 };
			var topic = new Topic { TopicID = 4, ForumID = forum.ForumID };
			var service = GetService();
			var user = new User { UserID = 123 };
			var post = new Post { PostID = 67, UserID = user.UserID, IsFirstInTopic = false, TopicID = topic.TopicID, IsEdited = false };
			_topicService.Setup(t => t.Get(topic.TopicID)).ReturnsAsync(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).ReturnsAsync(forum);
			var editedPost = new Post();
			_postRepo.Setup(p => p.Update(It.IsAny<Post>())).Callback<Post>(x => editedPost = x);
			await service.Delete(post, user);
			Assert.True(editedPost.IsEdited);
			Assert.Equal(user.Name, editedPost.LastEditName);
			Assert.True(editedPost.LastEditTime.HasValue);
		}

		[Fact]
		public async Task DeleteCallSetsIsDeletedAndUpdates()
		{
			var forum = new Forum { ForumID = 5 };
			var topic = new Topic { TopicID = 4, ForumID = forum.ForumID };
			var service = GetService();
			var user = new User { UserID = 123 };
			var post = new Post { PostID = 67, UserID = user.UserID, IsFirstInTopic = false, TopicID = topic.TopicID, IsDeleted = false };
			_topicService.Setup(t => t.Get(topic.TopicID)).ReturnsAsync(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).ReturnsAsync(forum);
			var persistedPost = new Post();
			_postRepo.Setup(p => p.Update(It.IsAny<Post>())).Callback<Post>(p => persistedPost = p);
			await service.Delete(post, user);
			Assert.Equal(post.PostID, persistedPost.PostID);
			Assert.True(persistedPost.IsDeleted);
		}

		[Fact]
		public async Task DeleteCallFiresRecalcs()
		{
			var forum = new Forum { ForumID = 5 };
			var topic = new Topic { TopicID = 4, ForumID = forum.ForumID };
			var service = GetService();
			var user = new User { UserID = 123 };
			var post = new Post { PostID = 67, UserID = user.UserID, IsFirstInTopic = false, TopicID = topic.TopicID };
			_topicService.Setup(t => t.Get(topic.TopicID)).ReturnsAsync(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).ReturnsAsync(forum);
			var payload = new SearchIndexPayload();
			_searchIndexQueue.Setup(x => x.Enqueue(It.IsAny<SearchIndexPayload>())).Callback<SearchIndexPayload>(p => payload = p);

			await service.Delete(post, user);

			_topicService.Verify(t => t.RecalculateReplyCount(topic), Times.Exactly(1));
			_topicService.Verify(t => t.UpdateLast(topic), Times.Once());
			_forumService.Verify(f => f.UpdateCounts(forum), Times.Exactly(1));
			_forumService.Verify(f => f.UpdateLast(forum), Times.Exactly(1));
			_searchIndexQueue.Verify(x => x.Enqueue(payload), Times.Once);
			Assert.Equal(topic.TopicID, payload.TopicID);
		}

		[Fact]
		public async Task UndeleteThrowsForNonMod()
		{
			var service = GetService();
			var user = new User { UserID = 123 };
			var post = new Post { PostID = 67, UserID = user.UserID };
			await Assert.ThrowsAsync<Exception>(async () => await service.Undelete(post, user));
		}

		[Fact]
		public async Task UndeleteCallLogs()
		{
			var forum = new Forum { ForumID = 5 };
			var topic = new Topic { TopicID = 4, ForumID = forum.ForumID };
			var service = GetService();
			var user = new User { UserID = 123, Roles = new List<string> { PermanentRoles.Moderator }};
			var post = new Post { PostID = 67, TopicID = topic.TopicID };
			_topicService.Setup(t => t.Get(topic.TopicID)).ReturnsAsync(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).ReturnsAsync(forum);
			await service.Undelete(post, user);
			_modLogService.Verify(m => m.LogPost(user, ModerationType.PostUndelete, post, String.Empty, String.Empty), Times.Exactly(1));
		}

		[Fact]
		public async Task UndeleteSetsEditFields()
		{
			var forum = new Forum { ForumID = 5 };
			var topic = new Topic { TopicID = 4, ForumID = forum.ForumID };
			var service = GetService();
			var user = new User { UserID = 123, Roles = new List<string> { PermanentRoles.Moderator } };
			var post = new Post { PostID = 67, TopicID = topic.TopicID, IsEdited = false, UserID = user.UserID };
			_topicService.Setup(t => t.Get(topic.TopicID)).ReturnsAsync(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).ReturnsAsync(forum);
			var editedPost = new Post();
			_postRepo.Setup(p => p.Update(It.IsAny<Post>())).Callback<Post>(x => editedPost = x);
			await service.Undelete(post, user);
			Assert.True(editedPost.IsEdited);
			Assert.Equal(user.Name, editedPost.LastEditName);
			Assert.True(editedPost.LastEditTime.HasValue);
		}

		[Fact]
		public async Task UndeleteCallSetsIsDeletedAndUpdates()
		{
			var forum = new Forum { ForumID = 5 };
			var topic = new Topic { TopicID = 4, ForumID = forum.ForumID };
			var service = GetService();
			var user = new User { UserID = 123, Roles = new List<string> { PermanentRoles.Moderator } };
			var post = new Post { PostID = 67, TopicID = topic.TopicID, IsDeleted = true };
			_topicService.Setup(t => t.Get(topic.TopicID)).ReturnsAsync(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).ReturnsAsync(forum);
			var persistedPost = new Post();
			_postRepo.Setup(p => p.Update(It.IsAny<Post>())).Callback<Post>(p => persistedPost = p);
			await service.Undelete(post, user);
			Assert.Equal(post.PostID, persistedPost.PostID);
			Assert.False(persistedPost.IsDeleted);
		}

		[Fact]
		public async Task UndeleteCallFiresRecalcs()
		{
			var forum = new Forum { ForumID = 5 };
			var topic = new Topic { TopicID = 4, ForumID = forum.ForumID };
			var service = GetService();
			var user = new User { UserID = 123, Roles = new List<string> { PermanentRoles.Moderator } };
			var post = new Post { PostID = 67, TopicID = topic.TopicID };
			_topicService.Setup(t => t.Get(topic.TopicID)).ReturnsAsync(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).ReturnsAsync(forum);
			var payload = new SearchIndexPayload();
			_searchIndexQueue.Setup(x => x.Enqueue(It.IsAny<SearchIndexPayload>())).Callback<SearchIndexPayload>(p => payload = p);

			await service.Undelete(post, user);

			_topicService.Verify(t => t.RecalculateReplyCount(topic), Times.Exactly(1));
			_topicService.Verify(t => t.UpdateLast(topic), Times.Once());
			_forumService.Verify(f => f.UpdateCounts(forum), Times.Exactly(1));
			_forumService.Verify(f => f.UpdateLast(forum), Times.Exactly(1));
			_searchIndexQueue.Verify(x => x.Enqueue(payload), Times.Once);
			Assert.Equal(topic.TopicID, payload.TopicID);
		}

		[Fact]
		public async Task GetPostsToEndSkipsLoadedPosts()
		{
			var service = GetService();
			var posts = new List<Post> {new Post { PostID = 1 }, new Post { PostID = 2 }, new Post { PostID = 3 }, new Post { PostID = 4 } };
			_postRepo.Setup(p => p.Get(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(posts);
			_settingsManager.Setup(s => s.Current.PostsPerPage).Returns(5);
			var result = await service.GetPosts(new Topic { TopicID = 123 }, 2, true);
			Assert.Equal(2, result.Item1.Count);
			Assert.Equal(3, result.Item1[0].PostID);
			Assert.Equal(4, result.Item1[1].PostID);
		}

		[Fact]
		public async Task GetPostsToEndCalcsCorrectPagerContext()
		{
			var service = GetService();
			var posts = new List<Post> { new Post { PostID = 1 }, new Post { PostID = 2 }, new Post { PostID = 3 }, new Post { PostID = 4 }, new Post { PostID = 5 }, new Post { PostID = 6 }, new Post { PostID = 7 }, new Post { PostID = 8 } };
			_postRepo.Setup(p => p.Get(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(posts);
			_settingsManager.Setup(s => s.Current.PostsPerPage).Returns(3);
			var (resultPosts, pagerContext) = await service.GetPosts(new Topic { TopicID = 123 }, 2, true);
			Assert.Equal(3, pagerContext.PageCount);
			Assert.Equal(3, pagerContext.PageIndex);
		}

		[Fact]
		public async Task VotePostCallsRepoWithPostIDAndUserID()
		{
			var service = GetService();
			var post = new Post { PostID = 1 };
			var user = new User { UserID = 2 };
			_postRepo.Setup(x => x.GetVotes(post.PostID)).ReturnsAsync(new Dictionary<int, string>());
			await service.VotePost(post, user, "abc", "def", "ghi");
			_postRepo.Verify(x => x.VotePost(post.PostID, user.UserID), Times.Once());
		}

		[Fact]
		public async Task VotePostCalcsAndSetsCount()
		{
			var service = GetService();
			var post = new Post { PostID = 1 };
			var user = new User { UserID = 2 };
			const int votes = 32;
			_postRepo.Setup(x => x.GetVotes(post.PostID)).ReturnsAsync(new Dictionary<int, string>());
			_postRepo.Setup(x => x.CalculateVoteCount(post.PostID)).ReturnsAsync(votes);
			await service.VotePost(post, user, "abc", "def", "ghi");
			_postRepo.Verify(x => x.SetVoteCount(post.PostID, votes), Times.Once());
		}

		[Fact]
		public async Task VotePostDoesntAllowDupeVote()
		{
			var service = GetService();
			var post = new Post { PostID = 1 };
			var user = new User { UserID = 2 };
			var voters = new Dictionary<int, string> { { 1, "Foo" }, { user.UserID, "Dude" }, { 3, null }, { 4, "Chica" } };
			_postRepo.Setup(x => x.GetVotes(post.PostID)).ReturnsAsync(voters);
			await service.VotePost(post, user, "abc", "def", "ghi");
			_postRepo.Verify(x => x.VotePost(post.PostID, user.UserID), Times.Never());
		}

		[Fact]
		public async Task GetVoteCountCallsRepoAndReturns()
		{
			var service = GetService();
			var post = new Post { PostID = 1 };
			const int votes = 32;
			_postRepo.Setup(x => x.GetVoteCount(post.PostID)).ReturnsAsync(votes);
			var result = await service.GetVoteCount(post);
			Assert.Equal(votes, result);
		}

		[Fact]
		public async Task GetVotersReturnsContainerWithPostID()
		{
			var service = GetService();
			var post = new Post { PostID = 1 };
			_postRepo.Setup(x => x.GetVotes(post.PostID)).ReturnsAsync(new Dictionary<int, string>());
			var result = await service.GetVoters(post);
			Assert.Equal(post.PostID, result.PostID);
		}

		[Fact]
		public async Task GetVotersReturnsContainerWithTotalVotes()
		{
			var service = GetService();
			var post = new Post { PostID = 1 };
			var voters = new Dictionary<int, string> {{1, "Foo"}, {2, "Dude"}, {3, null}, {4, "Chica"}};
			_postRepo.Setup(x => x.GetVotes(post.PostID)).ReturnsAsync(voters);
			var result = await service.GetVoters(post);
			Assert.Equal(4, result.Votes);
		}

		[Fact]
		public async Task GetVotersFiltersNullNames()
		{
			var service = GetService();
			var post = new Post { PostID = 1 };
			var voters = new Dictionary<int, string> { { 1, "Foo" }, { 2, "Dude" }, { 3, null }, { 4, "Chica" } };
			_postRepo.Setup(x => x.GetVotes(post.PostID)).ReturnsAsync(voters);
			var result = await service.GetVoters(post);
			Assert.Equal(3, result.Voters.Count);
			Assert.False(result.Voters.ContainsValue(null));
		}

		[Fact]
		public async Task GetVotedIDsPassesUserID()
		{
			var service = GetService();
			var user = new User { UserID = 123 };
			await service.GetVotedPostIDs(user, new List<Post>());
			_postRepo.Verify(x => x.GetVotedPostIDs(user.UserID, It.IsAny<List<int>>()), Times.Once());
		}

		[Fact]
		public async Task GetVotedIDsPassesPostIDList()
		{
			var service = GetService();
			var user = new User { UserID = 123 };
			var list = new List<Post> {new Post { PostID = 4 }, new Post { PostID = 5 }, new Post { PostID = 8 } };
			List<int> returnedList = null;
			_postRepo.Setup(x => x.GetVotedPostIDs(user.UserID, It.IsAny<List<int>>())).Callback<int, List<int>>((u, l) => returnedList = l);
			await service.GetVotedPostIDs(user, list);
			Assert.Equal(3, returnedList.Count);
			Assert.Equal(4, returnedList[0]);
			Assert.Equal(5, returnedList[1]);
			Assert.Equal(8, returnedList[2]);
		}

		[Fact]
		public async Task GetVotedIDsReturnsRepoObject()
		{
			var service = GetService();
			var list = new List<int>();
			_postRepo.Setup(x => x.GetVotedPostIDs(It.IsAny<int>(), It.IsAny<List<int>>())).ReturnsAsync(list);
			var result = await service.GetVotedPostIDs(new User { UserID = 123 }, new List<Post>());
			Assert.Same(list, result);
		}

		[Fact]
		public async Task GetVotedIDsReturnsEmptyListWithNullUser()
		{
			var service = GetService();
			var result = await service.GetVotedPostIDs(null, new List<Post>());
			Assert.Empty(result);
		}

		[Fact]
		public async Task VoteUpDoesntCallPublisherWhenUserFromPostDoesNotExist()
		{
			var service = GetService();
			_userService.Setup(x => x.GetUser(It.IsAny<int>())).ReturnsAsync((User) null);
			_postRepo.Setup(x => x.GetVotes(It.IsAny<int>())).ReturnsAsync(new Dictionary<int, string>());
			await service.VotePost(new Post { PostID = 123 }, new User { UserID = 456 }, "", "", "");
			_eventPub.Verify(x => x.ProcessEvent(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<string>(), false), Times.Never());
		}

		[Fact]
		public async Task VoteUpCallsEventPub()
		{
			var service = GetService();
			var voteUpUser = new User { UserID = 777 };
			_userService.Setup(x => x.GetUser(voteUpUser.UserID)).ReturnsAsync(voteUpUser);
			_postRepo.Setup(x => x.GetVotes(It.IsAny<int>())).ReturnsAsync(new Dictionary<int, string>());
			await service.VotePost(new Post { PostID = 123, UserID = voteUpUser.UserID }, new User { UserID = 456 }, "", "", "");
			_eventPub.Verify(x => x.ProcessEvent(It.IsAny<string>(), voteUpUser, EventDefinitionService.StaticEventIDs.PostVote, false), Times.Once());
		}

		[Fact]
		public async Task VoteUpDoesNotCallWhenUserIsPoster()
		{
			var service = GetService();
			var voteUpUser = new User { UserID = 456 };
			_userService.Setup(x => x.GetUser(voteUpUser.UserID)).ReturnsAsync(voteUpUser);
			_postRepo.Setup(x => x.GetVotes(It.IsAny<int>())).ReturnsAsync(new Dictionary<int, string>());
			await service.VotePost(new Post { PostID = 123, UserID = voteUpUser.UserID }, new User { UserID = 456 }, "", "", "");
			_eventPub.Verify(x => x.ProcessEvent(It.IsAny<string>(), It.IsAny<User>(), EventDefinitionService.StaticEventIDs.PostVote, false), Times.Never());
		}

		[Fact]
		public async Task VoteUpPassesPubStringWithFormattedStuff()
		{
			var service = GetService();
			var voteUpUser = new User { UserID = 777 };
			const string userUrl = "http://abc";
			const string topicUrl = "http://def";
			const string title = "blah blah blah";
			_userService.Setup(x => x.GetUser(voteUpUser.UserID)).ReturnsAsync(voteUpUser);
			_postRepo.Setup(x => x.GetVotes(It.IsAny<int>())).ReturnsAsync(new Dictionary<int, string>());
			var message = String.Empty;
			_eventPub.Setup(x => x.ProcessEvent(It.IsAny<string>(), It.IsAny<User>(), EventDefinitionService.StaticEventIDs.PostVote, false)).Callback<string, User, string, bool>((x, y, z, a) => message = x);
			await service.VotePost(new Post { PostID = 123, UserID = voteUpUser.UserID }, new User { UserID = 456 }, userUrl, topicUrl, title);
			Assert.Contains(userUrl, message);
			Assert.Contains(topicUrl, message);
			Assert.Contains(title, message);
		}

		[Fact]
		public void GenerateParsedTextPreviewCallsForumCodeForPlainText()
		{
			var service = GetService();
			const string input = "ohgorigh";
			const string output = "90eyuw";
			_textParsingService.Setup(x => x.ForumCodeToHtml(input)).Returns(output);
			var result = service.GenerateParsedTextPreview(input, true);
			Assert.Equal(output, result);
			_textParsingService.Verify(x => x.ForumCodeToHtml(input), Times.Once());
		}

		[Fact]
		public void GenerateParsedTextPreviewCallsHtmlForRichText()
		{
			var service = GetService();
			const string input = "ohgorigh";
			const string output = "90eyuw";
			_textParsingService.Setup(x => x.ClientHtmlToHtml(input)).Returns(output);
			var result = service.GenerateParsedTextPreview(input, false);
			Assert.Equal(output, result);
			_textParsingService.Verify(x => x.ClientHtmlToHtml(input), Times.Once());
		}
	}
}
