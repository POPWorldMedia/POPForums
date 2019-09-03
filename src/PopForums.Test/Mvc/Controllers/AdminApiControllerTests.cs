using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Mvc.Areas.Forums.Controllers;
using PopForums.Mvc.Areas.Forums.Services;
using PopForums.ScoringGame;
using PopForums.Services;
using Xunit;

namespace PopForums.Test.Mvc.Controllers
{
	public class AdminApiControllerTests
	{
		private Mock<ISettingsManager> _settingsManager;
		private Mock<ICategoryService> _categoryService;
		private Mock<IForumService> _forumService;
		private Mock<IUserService> _userService;
		private Mock<ISearchService> _searchService;
		private Mock<IProfileService> _profileService;
		private Mock<IUserRetrievalShim> _userRetrievalShim;
		private Mock<IImageService> _imageService;
		private Mock<IBanService> _banService;
		private Mock<IMailingListService> _mailingListService;
		private Mock<IEventDefinitionService> _eventDefService;
		private Mock<IAwardDefinitionService> _awardDefService;
		private Mock<IEventPublisher> _eventPublisher;
		private Mock<IIPHistoryService> _ipHistoryService;
		private Mock<ISecurityLogService> _securityLogService;
		private Mock<IModerationLogService> _moderationLogService;
		private Mock<IErrorLog> _errorLog;
		private Mock<IServiceHeartbeatService> _serviceHeartbeatService;

		private AdminApiController GetController()
		{
			_settingsManager = new Mock<ISettingsManager>();
			_categoryService = new Mock<ICategoryService>();
			_forumService = new Mock<IForumService>();
			_userService = new Mock<IUserService>();
			_searchService = new Mock<ISearchService>();
			_profileService = new Mock<IProfileService>();
			_userRetrievalShim = new Mock<IUserRetrievalShim>();
			_imageService = new Mock<IImageService>();
			_banService = new Mock<IBanService>();
			_mailingListService = new Mock<IMailingListService>();
			_eventDefService = new Mock<IEventDefinitionService>();
			_awardDefService = new Mock<IAwardDefinitionService>();
			_eventPublisher = new Mock<IEventPublisher>();
			_ipHistoryService = new Mock<IIPHistoryService>();
			_securityLogService = new Mock<ISecurityLogService>();
			_moderationLogService = new Mock<IModerationLogService>();
			_errorLog = new Mock<IErrorLog>();
			_serviceHeartbeatService = new Mock<IServiceHeartbeatService>();
			return new AdminApiController(_settingsManager.Object, _categoryService.Object, _forumService.Object, _userService.Object, _searchService.Object, _profileService.Object, _userRetrievalShim.Object, _imageService.Object, _banService.Object, _mailingListService.Object, _eventDefService.Object, _awardDefService.Object, _eventPublisher.Object, _ipHistoryService.Object, _securityLogService.Object, _moderationLogService.Object, _errorLog.Object, _serviceHeartbeatService.Object);
		}

		public class SaveForum : AdminApiControllerTests
		{
			[Fact]
			public async Task CallsCreateIfForumIDIsZero()
			{
				var controller = GetController();
				var forum = new Forum {ForumID = 0, CategoryID = 1, Title = "tt", Description = "dd", IsVisible = true, IsArchived = true, IsQAForum = true, ForumAdapterName = "ff"};

				await controller.SaveForum(forum);

				_forumService.Verify(x => x.Create(forum.CategoryID, forum.Title, forum.Description, forum.IsVisible, forum.IsArchived, -1, forum.ForumAdapterName, forum.IsQAForum), Times.Once);
				_forumService.Verify(x => x.Update(It.IsAny<Forum>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
			}

			[Fact]
			public async Task CallsUpdateIfForumIDIsNotZero()
			{
				var controller = GetController();
				var forum = new Forum { ForumID = 123, CategoryID = 1, Title = "tt", Description = "dd", IsVisible = true, IsArchived = true, IsQAForum = true, ForumAdapterName = "ff" };
				var retrievedForum = new Forum();
				_forumService.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(retrievedForum);

				await controller.SaveForum(forum);

				_forumService.Verify(x => x.Update(retrievedForum, forum.CategoryID, forum.Title, forum.Description, forum.IsVisible, forum.IsArchived, forum.ForumAdapterName, forum.IsQAForum), Times.Once);
				_forumService.Verify(x => x.Create(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
			}
			[Fact]
			public async Task ReturnsNotFoundIfForumIsNotReal()
			{
				var controller = GetController();
				_forumService.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync((Forum)null);

				var result = await controller.SaveForum(new Forum{ForumID = 123});

				_forumService.Verify(x => x.Update(It.IsAny<Forum>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
				_forumService.Verify(x => x.Create(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
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
				_forumService.Setup(x => x.Get(forum.ForumID)).ReturnsAsync(forum);
				var all = new List<string> {"a", "b"};
				_userService.Setup(x => x.GetAllRoles()).ReturnsAsync(all);
				var allView = new List<string> {"c", "d"};
				_forumService.Setup(x => x.GetForumViewRoles(forum)).ReturnsAsync(allView);
				var allPost = new List<string> {"e", "f"};
				_forumService.Setup(x => x.GetForumPostRoles(forum)).ReturnsAsync(allPost);

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
				_userService.Setup(x => x.SearchByName(text)).ReturnsAsync(list);

				var result = await controller.EditUserSearch(new UserSearch {SearchText = text, SearchType = UserSearch.UserSearchType.Name});

				_userService.Verify(x => x.SearchByName(text), Times.Once);
				Assert.Same(list, result.Value);
			}

			[Fact]
			public async Task EmailSearchCallsEmailSearch()
			{
				var controller = GetController();
				var text = "abc";
				var list = new List<User>();
				_userService.Setup(x => x.SearchByEmail(text)).ReturnsAsync(list);

				var result = await controller.EditUserSearch(new UserSearch { SearchText = text, SearchType = UserSearch.UserSearchType.Email });

				_userService.Verify(x => x.SearchByEmail(text), Times.Once);
				Assert.Same(list, result.Value);
			}

			[Fact]
			public async Task RoleSearchCallsRoleSearch()
			{
				var controller = GetController();
				var text = "abc";
				var list = new List<User>();
				_userService.Setup(x => x.SearchByRole(text)).ReturnsAsync(list);

				var result = await controller.EditUserSearch(new UserSearch { SearchText = text, SearchType = UserSearch.UserSearchType.Role });

				_userService.Verify(x => x.SearchByRole(text), Times.Once);
				Assert.Same(list, result.Value);
			}
		}
	}
}