using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Mvc.Areas.Forums.Controllers;
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

		private AdminApiController GetController()
		{
			_settingsManager = new Mock<ISettingsManager>();
			_categoryService = new Mock<ICategoryService>();
			_forumService = new Mock<IForumService>();
			_userService = new Mock<IUserService>();
			_searchService = new Mock<ISearchService>();
			_profileService = new Mock<IProfileService>();
			return new AdminApiController(_settingsManager.Object, _categoryService.Object, _forumService.Object, _userService.Object, _searchService.Object, _profileService.Object);
		}

		public class SaveForum : AdminApiControllerTests
		{
			[Fact]
			public void CallsCreateIfForumIDIsZero()
			{
				var controller = GetController();
				var forum = new Forum {ForumID = 0, CategoryID = 1, Title = "tt", Description = "dd", IsVisible = true, IsArchived = true, IsQAForum = true, ForumAdapterName = "ff"};

				controller.SaveForum(forum);

				_forumService.Verify(x => x.Create(forum.CategoryID, forum.Title, forum.Description, forum.IsVisible, forum.IsArchived, -1, forum.ForumAdapterName, forum.IsQAForum), Times.Once);
				_forumService.Verify(x => x.Update(It.IsAny<Forum>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
			}

			[Fact]
			public void CallsUpdateIfForumIDIsNotZero()
			{
				var controller = GetController();
				var forum = new Forum { ForumID = 123, CategoryID = 1, Title = "tt", Description = "dd", IsVisible = true, IsArchived = true, IsQAForum = true, ForumAdapterName = "ff" };
				var retrievedForum = new Forum();
				_forumService.Setup(x => x.Get(forum.ForumID)).Returns(retrievedForum);

				controller.SaveForum(forum);

				_forumService.Verify(x => x.Update(retrievedForum, forum.CategoryID, forum.Title, forum.Description, forum.IsVisible, forum.IsArchived, forum.ForumAdapterName, forum.IsQAForum), Times.Once);
				_forumService.Verify(x => x.Create(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
			}
			[Fact]
			public void ReturnsNotFoundIfForumIsNotReal()
			{
				var controller = GetController();
				_forumService.Setup(x => x.Get(It.IsAny<int>())).Returns((Forum)null);

				var result = controller.SaveForum(new Forum{ForumID = 123});

				_forumService.Verify(x => x.Update(It.IsAny<Forum>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
				_forumService.Verify(x => x.Create(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
				Assert.IsType<NotFoundResult>(result.Result);
			}
		}

		public class GetForumPermissions : AdminApiControllerTests
		{
			[Fact]
			public void ContainerIsComposed()
			{
				var controller = GetController();
				var forum = new Forum{ForumID = 123};
				_forumService.Setup(x => x.Get(forum.ForumID)).Returns(forum);
				var all = new List<string> {"a", "b"};
				_userService.Setup(x => x.GetAllRoles()).Returns(all);
				var allView = new List<string> {"c", "d"};
				_forumService.Setup(x => x.GetForumViewRoles(forum)).Returns(allView);
				var allPost = new List<string> {"e", "f"};
				_forumService.Setup(x => x.GetForumPostRoles(forum)).Returns(allPost);

				var container = controller.GetForumPermissions(forum.ForumID);

				Assert.Equal(forum.ForumID, container.Value.ForumID);
				Assert.Same(all, container.Value.AllRoles);
				Assert.Same(allView, container.Value.ViewRoles);
				Assert.Same(allPost, container.Value.PostRoles);
			}
		}

		public class EditUserSearch : AdminApiControllerTests
		{
			[Fact]
			public void NameSearchCallsNameSearch()
			{
				var controller = GetController();
				var text = "abc";
				var list = new List<User>();
				_userService.Setup(x => x.SearchByName(text)).Returns(list);

				var result = controller.EditUserSearch(new UserSearch {SearchText = text, SearchType = UserSearch.UserSearchType.Name});

				_userService.Verify(x => x.SearchByName(text), Times.Once);
				Assert.Same(list, result.Value);
			}

			[Fact]
			public void EmailSearchCallsEmailSearch()
			{
				var controller = GetController();
				var text = "abc";
				var list = new List<User>();
				_userService.Setup(x => x.SearchByEmail(text)).Returns(list);

				var result = controller.EditUserSearch(new UserSearch { SearchText = text, SearchType = UserSearch.UserSearchType.Email });

				_userService.Verify(x => x.SearchByEmail(text), Times.Once);
				Assert.Same(list, result.Value);
			}

			[Fact]
			public void RoleSearchCallsRoleSearch()
			{
				var controller = GetController();
				var text = "abc";
				var list = new List<User>();
				_userService.Setup(x => x.SearchByRole(text)).Returns(list);

				var result = controller.EditUserSearch(new UserSearch { SearchText = text, SearchType = UserSearch.UserSearchType.Role });

				_userService.Verify(x => x.SearchByRole(text), Times.Once);
				Assert.Same(list, result.Value);
			}
		}
	}
}