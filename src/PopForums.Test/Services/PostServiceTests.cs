namespace PopForums.Test.Services;

public class PostServiceTests
{
	private IPostRepository _postRepo;
	private IProfileRepository _profileRepo;
	private ISettingsManager _settingsManager;
	private Settings _settings;
	private ITopicService _topicService;
	private ITextParsingService _textParsingService;
	private IModerationLogService _modLogService;
	private IForumService _forumService;
	private IEventPublisher _eventPub;
	private IUserService _userService;
	private ISearchIndexQueueRepository _searchIndexQueue;
	private ITenantService _tenantService;
	private INotificationAdapter _notificationAdapter;

	private PostService GetService()
	{
		_postRepo = Substitute.For<IPostRepository>();
		_profileRepo = Substitute.For<IProfileRepository>();
		_settingsManager = Substitute.For<ISettingsManager>();
		_settings = Substitute.For<Settings>();
		_topicService = Substitute.For<ITopicService>();
		_textParsingService = Substitute.For<ITextParsingService>();
		_modLogService = Substitute.For<IModerationLogService>();
		_forumService = Substitute.For<IForumService>();
		_eventPub = Substitute.For<IEventPublisher>();
		_userService = Substitute.For<IUserService>();
		_searchIndexQueue = Substitute.For<ISearchIndexQueueRepository>();
		_tenantService = Substitute.For<ITenantService>();
		_notificationAdapter = Substitute.For<INotificationAdapter>();
		_settingsManager.Current.Returns(_settings);
		return new PostService(_postRepo, _profileRepo, _settingsManager, _topicService, _textParsingService, _modLogService, _forumService, _eventPub, _userService, _searchIndexQueue, _tenantService, _notificationAdapter);
	}

	[Fact]
	public async Task GetPostsPageSizeAndStartRowCalcdCorrectly()
	{
		var topic = new Topic { TopicID = 1, ReplyCount = 20};
		var postService = GetService();
		_settings.PostsPerPage.Returns(2);
		var (_, pagerContext) = await postService.GetPosts(topic, false, 4);
		await _postRepo.Received().Get(1, false, 7, 2);
		await _postRepo.DidNotReceive().GetReplyCount(Arg.Any<int>(), Arg.Any<bool>());
		Assert.Equal(11, pagerContext.PageCount);
		Assert.Equal(2, pagerContext.PageSize);
	}

	[Fact]
	public async Task GetPostsReplyCountCalledOnIncludeDeleted()
	{
		var topic = new Topic { TopicID = 1, ReplyCount = 20 };
		var postService = GetService();
		_settings.PostsPerPage.Returns(2);
		_postRepo.GetReplyCount(topic.TopicID, true).Returns(Task.FromResult(21));
		var (_, pagerContext) = await postService.GetPosts(topic, true, 4);
		await _postRepo.Received().GetReplyCount(topic.TopicID, true);
		Assert.Equal(11, pagerContext.PageCount);
	}

	[Fact]
	public async Task GetPostsPagerContextConstructed()
	{
		var topic = new Topic { TopicID = 1, ReplyCount = 20 };
		var postService = GetService();
		_settings.PostsPerPage.Returns(3);
		var (_, pagerContext) = await postService.GetPosts(topic, false, 4);
		Assert.Equal(7, pagerContext.PageCount);
		Assert.Equal(4, pagerContext.PageIndex);
	}

	[Fact]
	public async Task GetPostsHitsRepo()
	{
		var topic = new Topic { TopicID = 1, ReplyCount = 20 };
		var posts = new List<Post>();
		var postService = GetService();
		_settings.PostsPerPage.Returns(3);
		_postRepo.Get(1, false, Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(posts));
		var result = await postService.GetPosts(topic, false, 4);
		Assert.Same(posts, result.Item1);
	}

	[Fact]
	public async Task GetCallsRepoAndReturns()
	{
		var postService = GetService();
		var postID = 123;
		var post = new Post {PostID = postID};
		_postRepo.Get(postID).Returns(Task.FromResult(post));
		var postResult = await postService.Get(postID);
		await _postRepo.Received().Get(postID);
		Assert.Same(postResult, post);
	}

	[Fact]
	public async Task GetPostCountCallsRepo()
	{
		var postService = GetService();
		_postRepo.GetPostCount(123).Returns(Task.FromResult(456));
		var user = new User { UserID = 123 };
		var result = await postService.GetPostCount(user);
		await _postRepo.Received(1).GetPostCount(123);
		Assert.Equal(456, result);
	}

	[Fact]
	public async Task GetPostForEditPlainText()
	{
		var service = GetService();
		var post = new Post { PostID = 123, Title = "mah title", FullText = "not", ShowSig = true, IsFirstInTopic = true };
		var user = new User { UserID = 456 };
		_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile {IsPlainText = true}));
		_textParsingService.HtmlToForumCode("not").Returns("new text");
		var postEdit = await service.GetPostForEdit(post, user);
		Assert.Equal("mah title", postEdit.Title);
		Assert.Equal("new text", postEdit.FullText);
		Assert.True(postEdit.IsFirstInTopic);
		Assert.True(postEdit.ShowSig);
		Assert.True(postEdit.IsPlainText);
		_textParsingService.Received(1).HtmlToForumCode("not");
	}

	[Fact]
	public async Task GetPostForEditNotPlainText()
	{
		var service = GetService();
		var post = new Post { PostID = 123, Title = "mah title", FullText = "not", ShowSig = true, IsFirstInTopic = true };
		var user = new User { UserID = 456 };
		_profileRepo.GetProfile(user.UserID).Returns(Task.FromResult(new Profile { IsPlainText = false }));
		_textParsingService.HtmlToClientHtml("not").Returns("new text");
		var postEdit = await service.GetPostForEdit(post, user);
		Assert.Equal("mah title", postEdit.Title);
		Assert.Equal("new text", postEdit.FullText);
		Assert.True(postEdit.IsFirstInTopic);
		Assert.True(postEdit.ShowSig);
		Assert.False(postEdit.IsPlainText);
		_textParsingService.Received(1).HtmlToClientHtml("not");
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
		_topicService.Get(topic.TopicID).Returns(Task.FromResult(topic));
		_forumService.Get(forum.ForumID).Returns(Task.FromResult(forum));
		await service.Delete(post, user);
		await _topicService.Received(1).DeleteTopic(topic, user);
	}

	[Fact]
	public async Task DeleteCallLogs()
	{
		var forum = new Forum { ForumID = 5 };
		var topic = new Topic { TopicID = 4, ForumID = forum.ForumID };
		var service = GetService();
		var user = new User { UserID = 123 };
		var post = new Post { PostID = 67, UserID = user.UserID, IsFirstInTopic = false, TopicID = topic.TopicID };
		_topicService.Get(topic.TopicID).Returns(Task.FromResult(topic));
		_forumService.Get(forum.ForumID).Returns(Task.FromResult(forum));
		await service.Delete(post, user);
		await _modLogService.Received(1).LogPost(user, ModerationType.PostDelete, post, String.Empty, String.Empty);
	}

	[Fact]
	public async Task DeleteSetsEditFields()
	{
		var forum = new Forum { ForumID = 5 };
		var topic = new Topic { TopicID = 4, ForumID = forum.ForumID };
		var service = GetService();
		var user = new User { UserID = 123 };
		var post = new Post { PostID = 67, UserID = user.UserID, IsFirstInTopic = false, TopicID = topic.TopicID, IsEdited = false };
		_topicService.Get(topic.TopicID).Returns(Task.FromResult(topic));
		_forumService.Get(forum.ForumID).Returns(Task.FromResult(forum));
		var editedPost = new Post();
		await _postRepo.Update(Arg.Do<Post>(x => editedPost = x));
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
		_topicService.Get(topic.TopicID).Returns(Task.FromResult(topic));
		_forumService.Get(forum.ForumID).Returns(Task.FromResult(forum));
		var persistedPost = new Post();
		await _postRepo.Update(Arg.Do<Post>(x => persistedPost = x));
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
		_topicService.Get(topic.TopicID).Returns(Task.FromResult(topic));
		_forumService.Get(forum.ForumID).Returns(Task.FromResult(forum));
		var payload = new SearchIndexPayload();
		await _searchIndexQueue.Enqueue(Arg.Do<SearchIndexPayload>(x => payload = x));

		await service.Delete(post, user);

		await _topicService.Received(1).RecalculateReplyCount(topic);
		await _topicService.Received().UpdateLast(topic);
		_forumService.Received(1).UpdateCounts(forum);
		await _forumService.Received(1).UpdateLast(forum);
		await _searchIndexQueue.Received().Enqueue(payload);
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
		_topicService.Get(topic.TopicID).Returns(Task.FromResult(topic));
		_forumService.Get(forum.ForumID).Returns(Task.FromResult(forum));
		await service.Undelete(post, user);
		await _modLogService.Received(1).LogPost(user, ModerationType.PostUndelete, post, String.Empty, String.Empty);
	}

	[Fact]
	public async Task UndeleteSetsEditFields()
	{
		var forum = new Forum { ForumID = 5 };
		var topic = new Topic { TopicID = 4, ForumID = forum.ForumID };
		var service = GetService();
		var user = new User { UserID = 123, Roles = new List<string> { PermanentRoles.Moderator } };
		var post = new Post { PostID = 67, TopicID = topic.TopicID, IsEdited = false, UserID = user.UserID };
		_topicService.Get(topic.TopicID).Returns(Task.FromResult(topic));
		_forumService.Get(forum.ForumID).Returns(Task.FromResult(forum));
		var editedPost = new Post();
		await _postRepo.Update(Arg.Do<Post>(x => editedPost = x));
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
		_topicService.Get(topic.TopicID).Returns(Task.FromResult(topic));
		_forumService.Get(forum.ForumID).Returns(Task.FromResult(forum));
		var persistedPost = new Post();
		await _postRepo.Update(Arg.Do<Post>(x => persistedPost = x));
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
		_topicService.Get(topic.TopicID).Returns(Task.FromResult(topic));
		_forumService.Get(forum.ForumID).Returns(Task.FromResult(forum));
		var payload = new SearchIndexPayload();
		await _searchIndexQueue.Enqueue(Arg.Do<SearchIndexPayload>(x => payload = x));

		await service.Undelete(post, user);

		await _topicService.Received(1).RecalculateReplyCount(topic);
		await _topicService.Received().UpdateLast(topic);
		_forumService.Received(1).UpdateCounts(forum);
		await _forumService.Received(1).UpdateLast(forum);
		await _searchIndexQueue.Received().Enqueue(payload);
		Assert.Equal(topic.TopicID, payload.TopicID);
	}

	[Fact]
	public async Task GetPostsToEndSkipsLoadedPosts()
	{
		var service = GetService();
		var posts = new List<Post> {new Post { PostID = 1 }, new Post { PostID = 2 }, new Post { PostID = 3 }, new Post { PostID = 4 } };
		_postRepo.Get(Arg.Any<int>(), Arg.Any<bool>()).Returns(Task.FromResult(posts));
		_settingsManager.Current.PostsPerPage.Returns(5);
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
		_postRepo.Get(Arg.Any<int>(), Arg.Any<bool>()).Returns(Task.FromResult(posts));
		_settingsManager.Current.PostsPerPage.Returns(3);
		var (_, pagerContext) = await service.GetPosts(new Topic { TopicID = 123 }, 2, true);
		Assert.Equal(3, pagerContext.PageCount);
		Assert.Equal(3, pagerContext.PageIndex);
	}

	[Fact]
	public async Task ToggleVoteCallsRepoWithPostIDAndUserID()
	{
		var service = GetService();
		var post = new Post { PostID = 1 };
		var user = new User { UserID = 2 };
		_postRepo.GetVotes(post.PostID).Returns(Task.FromResult(new Dictionary<int, string>()));
		await service.ToggleVoteReturnCountAndIsVoted(post, user, "abc", "def", "ghi");
		await _postRepo.Received().VotePost(post.PostID, user.UserID);
	}

	[Fact]
	public async Task ToggleVoteCalcsAndSetsCount()
	{
		var service = GetService();
		var post = new Post { PostID = 1 };
		var user = new User { UserID = 2 };
		const int votes = 32;
		_postRepo.GetVotes(post.PostID).Returns(Task.FromResult(new Dictionary<int, string>()));
		_postRepo.CalculateVoteCount(post.PostID).Returns(Task.FromResult(votes));
		await service.ToggleVoteReturnCountAndIsVoted(post, user, "abc", "def", "ghi");
		await _postRepo.Received().SetVoteCount(post.PostID, votes);
	}

	[Fact]
	public async Task GetVoteCountCallsRepoAndReturns()
	{
		var service = GetService();
		var post = new Post { PostID = 1 };
		const int votes = 32;
		_postRepo.GetVoteCount(post.PostID).Returns(Task.FromResult(votes));
		var result = await service.GetVoteCount(post);
		Assert.Equal(votes, result);
	}

	[Fact]
	public async Task GetVotersReturnsContainerWithPostID()
	{
		var service = GetService();
		var post = new Post { PostID = 1 };
		_postRepo.GetVotes(post.PostID).Returns(Task.FromResult(new Dictionary<int, string>()));
		var result = await service.GetVoters(post);
		Assert.Equal(post.PostID, result.PostID);
	}

	[Fact]
	public async Task GetVotersReturnsContainerWithTotalVotes()
	{
		var service = GetService();
		var post = new Post { PostID = 1 };
		var voters = new Dictionary<int, string> {{1, "Foo"}, {2, "Dude"}, {3, null}, {4, "Chica"}};
		_postRepo.GetVotes(post.PostID).Returns(Task.FromResult(voters));
		var result = await service.GetVoters(post);
		Assert.Equal(4, result.Votes);
	}

	[Fact]
	public async Task GetVotersFiltersNullNames()
	{
		var service = GetService();
		var post = new Post { PostID = 1 };
		var voters = new Dictionary<int, string> { { 1, "Foo" }, { 2, "Dude" }, { 3, null }, { 4, "Chica" } };
		_postRepo.GetVotes(post.PostID).Returns(Task.FromResult(voters));
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
		await _postRepo.Received().GetVotedPostIDs(user.UserID, Arg.Any<List<int>>());
	}

	[Fact]
	public async Task GetVotedIDsPassesPostIDList()
	{
		var service = GetService();
		var user = new User { UserID = 123 };
		var list = new List<Post> {new Post { PostID = 4 }, new Post { PostID = 5 }, new Post { PostID = 8 } };
		List<int> returnedList = null;
		await _postRepo.GetVotedPostIDs(user.UserID, Arg.Do<List<int>>(x => returnedList = x));
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
		_postRepo.GetVotedPostIDs(Arg.Any<int>(), Arg.Any<List<int>>()).Returns(Task.FromResult(list));
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
	public async Task ToggleVoteDoesntCallPublisherWhenUserFromPostDoesNotExist()
	{
		var service = GetService();
		_userService.GetUser(Arg.Any<int>()).Returns((User) null);
		_postRepo.GetVotes(Arg.Any<int>()).Returns(Task.FromResult(new Dictionary<int, string>()));
		await service.ToggleVoteReturnCountAndIsVoted(new Post { PostID = 123 }, new User { UserID = 456 }, "", "", "");
		await _eventPub.DidNotReceive().ProcessEvent(Arg.Any<string>(), Arg.Any<User>(), Arg.Any<string>(), false);
	}

	[Fact]
	public async Task ToggleVoteCallsEventPub()
	{
		var service = GetService();
		var voteUpUser = new User { UserID = 777 };
		_userService.GetUser(voteUpUser.UserID).Returns(Task.FromResult(voteUpUser));
		_postRepo.GetVotes(Arg.Any<int>()).Returns(Task.FromResult(new Dictionary<int, string>()));
		await service.ToggleVoteReturnCountAndIsVoted(new Post { PostID = 123, UserID = voteUpUser.UserID }, new User { UserID = 456 }, "", "", "");
		await _eventPub.Received().ProcessEvent(Arg.Any<string>(), voteUpUser, EventDefinitionService.StaticEventIDs.PostVote, false);
	}

	[Fact]
	public async Task ToggleVoteCallsNotification()
	{
		var service = GetService();
		var voteUpUser = new User { UserID = 777, Name = "Diana" };
		var title = "the title";
		_userService.GetUser(voteUpUser.UserID).Returns(Task.FromResult(voteUpUser));
		_postRepo.GetVotes(Arg.Any<int>()).Returns(Task.FromResult(new Dictionary<int, string>()));

		await service.ToggleVoteReturnCountAndIsVoted(new Post { PostID = 123, UserID = voteUpUser.UserID }, new User { UserID = 456, Name = "Voter" }, "", "", title);

		await _notificationAdapter.Received().Vote("Voter", title, 123, voteUpUser.UserID);
	}

	[Fact]
	public async Task ToggleVoteDoesNotCallWhenUserIsPoster()
	{
		var service = GetService();
		var voteUpUser = new User { UserID = 456 };
		_userService.GetUser(voteUpUser.UserID).Returns(Task.FromResult(voteUpUser));
		_postRepo.GetVotes(Arg.Any<int>()).Returns(Task.FromResult(new Dictionary<int, string>()));
		await service.ToggleVoteReturnCountAndIsVoted(new Post { PostID = 123, UserID = voteUpUser.UserID }, new User { UserID = 456 }, "", "", "");
		await _eventPub.DidNotReceive().ProcessEvent(Arg.Any<string>(), Arg.Any<User>(), EventDefinitionService.StaticEventIDs.PostVote, false);
	}

	[Fact]
	public async Task ToggleVotePassesPubStringWithFormattedStuff()
	{
		var service = GetService();
		var voteUpUser = new User { UserID = 777 };
		const string userUrl = "http://abc";
		const string topicUrl = "http://def";
		const string title = "blah blah blah";
		_userService.GetUser(voteUpUser.UserID).Returns(Task.FromResult(voteUpUser));
		_postRepo.GetVotes(Arg.Any<int>()).Returns(Task.FromResult(new Dictionary<int, string>()));
		var message = String.Empty;
		await _eventPub.ProcessEvent(Arg.Do<string>(x => message = x), Arg.Any<User>(), EventDefinitionService.StaticEventIDs.PostVote, false);
		await service.ToggleVoteReturnCountAndIsVoted(new Post { PostID = 123, UserID = voteUpUser.UserID }, new User { UserID = 456 }, userUrl, topicUrl, title);
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
		_textParsingService.ForumCodeToHtml(input).Returns(output);
		var result = service.GenerateParsedTextPreview(input, true);
		Assert.Equal(output, result);
		_textParsingService.Received().ForumCodeToHtml(input);
	}

	[Fact]
	public void GenerateParsedTextPreviewCallsHtmlForRichText()
	{
		var service = GetService();
		const string input = "ohgorigh";
		const string output = "90eyuw";
		_textParsingService.ClientHtmlToHtml(input).Returns(output);
		var result = service.GenerateParsedTextPreview(input, false);
		Assert.Equal(output, result);
		_textParsingService.Received().ClientHtmlToHtml(input);
	}
}