namespace PopForums.Test.Mvc.Controllers;

public class AdminApiControllerTests
{
	private ISettingsManager _settingsManager;
	private ICategoryService _categoryService;
	private IForumService _forumService;
	private IUserService _userService;
	private ISearchService _searchService;
	private IProfileService _profileService;
	private IUserRetrievalShim _userRetrievalShim;
	private IImageService _imageService;
	private IBanService _banService;
	private IMailingListService _mailingListService;
	private IEventDefinitionService _eventDefService;
	private IAwardDefinitionService _awardDefService;
	private IEventPublisher _eventPublisher;
	private IIPHistoryService _ipHistoryService;
	private ISecurityLogService _securityLogService;
	private IModerationLogService _moderationLogService;
	private IErrorLog _errorLog;
	private IServiceHeartbeatService _serviceHeartbeatService;

	private AdminApiController GetController()
	{
		_settingsManager = Substitute.For<ISettingsManager>();
		_categoryService = Substitute.For<ICategoryService>();
		_forumService = Substitute.For<IForumService>();
		_userService = Substitute.For<IUserService>();
		_searchService = Substitute.For<ISearchService>();
		_profileService = Substitute.For<IProfileService>();
		_userRetrievalShim = Substitute.For<IUserRetrievalShim>();
		_imageService = Substitute.For<IImageService>();
		_banService = Substitute.For<IBanService>();
		_mailingListService = Substitute.For<IMailingListService>();
		_eventDefService = Substitute.For<IEventDefinitionService>();
		_awardDefService = Substitute.For<IAwardDefinitionService>();
		_eventPublisher = Substitute.For<IEventPublisher>();
		_ipHistoryService = Substitute.For<IIPHistoryService>();
		_securityLogService = Substitute.For<ISecurityLogService>();
		_moderationLogService = Substitute.For<IModerationLogService>();
		_errorLog = Substitute.For<IErrorLog>();
		_serviceHeartbeatService = Substitute.For<IServiceHeartbeatService>();
		return new AdminApiController(_settingsManager, _categoryService, _forumService, _userService, _searchService, _profileService, _userRetrievalShim, _imageService, _banService, _mailingListService, _eventDefService, _awardDefService, _eventPublisher, _ipHistoryService, _securityLogService, _moderationLogService, _errorLog, _serviceHeartbeatService);
	}

	public class SaveForum : AdminApiControllerTests
	{
		[Fact]
		public async Task CallsCreateIfForumIDIsZero()
		{
			var controller = GetController();
			var forum = new Forum {ForumID = 0, CategoryID = 1, Title = "tt", Description = "dd", IsVisible = true, IsArchived = true, IsQAForum = true, ForumAdapterName = "ff"};

			await controller.SaveForum(forum);

			await _forumService.Received().Create(forum.CategoryID, forum.Title, forum.Description, forum.IsVisible, forum.IsArchived, -1, forum.ForumAdapterName, forum.IsQAForum);
			await _forumService.DidNotReceive().Update(Arg.Any<Forum>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<bool>());
		}

		[Fact]
		public async Task CallsUpdateIfForumIDIsNotZero()
		{
			var controller = GetController();
			var forum = new Forum { ForumID = 123, CategoryID = 1, Title = "tt", Description = "dd", IsVisible = true, IsArchived = true, IsQAForum = true, ForumAdapterName = "ff" };
			var retrievedForum = new Forum();
			_forumService.Get(forum.ForumID).Returns(Task.FromResult(retrievedForum));

			await controller.SaveForum(forum);

			await _forumService.Received().Update(retrievedForum, forum.CategoryID, forum.Title, forum.Description, forum.IsVisible, forum.IsArchived, forum.ForumAdapterName, forum.IsQAForum);
			await _forumService.DidNotReceive().Create(Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>());
		}
		[Fact]
		public async Task ReturnsNotFoundIfForumIsNotReal()
		{
			var controller = GetController();
			_forumService.Get(Arg.Any<int>()).Returns((Forum)null);

			var result = await controller.SaveForum(new Forum{ForumID = 123});

			await _forumService.DidNotReceive().Update(Arg.Any<Forum>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<bool>());
			await _forumService.DidNotReceive().Create(Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>());
			Assert.IsType<NotFoundResult>(result.Result);
		}
	}

	public class GetForumPermissions : AdminApiControllerTests
	{
		[Fact]
		public async Task ContainerIsComposed()
		{
			var controller = GetController();
			var forum = new Forum{ForumID = 123};
			_forumService.Get(forum.ForumID).Returns(Task.FromResult(forum));
			var all = new List<string> {"a", "b"};
			_userService.GetAllRoles().Returns(Task.FromResult(all));
			var allView = new List<string> {"c", "d"};
			_forumService.GetForumViewRoles(forum).Returns(Task.FromResult(allView));
			var allPost = new List<string> {"e", "f"};
			_forumService.GetForumPostRoles(forum).Returns(Task.FromResult(allPost));

			var container = await controller.GetForumPermissions(forum.ForumID);

			Assert.Equal(forum.ForumID, container.Value.ForumID);
			Assert.Same(all, container.Value.AllRoles);
			Assert.Same(allView, container.Value.ViewRoles);
			Assert.Same(allPost, container.Value.PostRoles);
		}
	}

	public class EditUserSearch : AdminApiControllerTests
	{
		[Fact]
		public async Task NameSearchCallsNameSearch()
		{
			var controller = GetController();
			var text = "abc";
			var list = new List<User>();
			_userService.SearchByName(text).Returns(Task.FromResult(list));

			var result = await controller.EditUserSearch(new UserSearch {SearchText = text, SearchType = UserSearch.UserSearchType.Name});

			await _userService.Received().SearchByName(text);
			Assert.Same(list, result.Value);
		}

		[Fact]
		public async Task EmailSearchCallsEmailSearch()
		{
			var controller = GetController();
			var text = "abc";
			var list = new List<User>();
			_userService.SearchByEmail(text).Returns(Task.FromResult(list));

			var result = await controller.EditUserSearch(new UserSearch { SearchText = text, SearchType = UserSearch.UserSearchType.Email });

			await _userService.Received().SearchByEmail(text);
			Assert.Same(list, result.Value);
		}

		[Fact]
		public async Task RoleSearchCallsRoleSearch()
		{
			var controller = GetController();
			var text = "abc";
			var list = new List<User>();
			_userService.SearchByRole(text).Returns(Task.FromResult(list));

			var result = await controller.EditUserSearch(new UserSearch { SearchText = text, SearchType = UserSearch.UserSearchType.Role });

			await _userService.Received().SearchByRole(text);
			Assert.Same(list, result.Value);
		}
	}
}