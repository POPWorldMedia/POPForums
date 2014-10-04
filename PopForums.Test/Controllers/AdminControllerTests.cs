using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using PopForums.Configuration;
using PopForums.Controllers;
using PopForums.Models;
using PopForums.ScoringGame;
using PopForums.Services;

namespace PopForums.Test.Controllers
{
	[TestFixture]
	public class AdminControllerTests
	{
		private Mock<ISettingsManager> _settings;
		private Mock<IUserService> _userService;
		private Mock<IProfileService> _profileService;
		private Mock<ICategoryService> _catService;
		private Mock<IForumService> _forumService;
		private Mock<ISearchService> _searchService;
		private Mock<ISecurityLogService> _securityLogService;
		private Mock<IErrorLog> _errorLog;
		private Mock<IBanService> _banService;
		private Mock<IModerationLogService> _modLogService;
		private Mock<IIPHistoryService> _ipHistoryService;
		private Mock<IImageService> _imageService;
		private Mock<IMailingListService> _mailingListService;
		private Mock<IEventDefinitionService> _eventDefService;
		private Mock<IAwardDefinitionService> _awardDefService;
		private Mock<IEventPublisher> _eventPublisher;

		private TestableAdminController GetController()
		{
			_userService = new Mock<IUserService>();
			_profileService = new Mock<IProfileService>();
			_settings = new Mock<ISettingsManager>();
			_catService = new Mock<ICategoryService>();
			_searchService = new Mock<ISearchService>();
			_forumService = new Mock<IForumService>();
			_securityLogService = new Mock<ISecurityLogService>();
			_errorLog = new Mock<IErrorLog>();
			_banService = new Mock<IBanService>();
			_modLogService = new Mock<IModerationLogService>();
			_ipHistoryService = new Mock<IIPHistoryService>();
			_imageService = new Mock<IImageService>();
			_mailingListService = new Mock<IMailingListService>();
			_eventDefService = new Mock<IEventDefinitionService>();
			_awardDefService = new Mock<IAwardDefinitionService>();
			_eventPublisher = new Mock<IEventPublisher>();
			_settings.Setup(s => s.Current).Returns(new Settings());
			return new TestableAdminController(_userService.Object, _profileService.Object, _settings.Object, _catService.Object, _forumService.Object, _searchService.Object, _securityLogService.Object, _errorLog.Object, _banService.Object, _modLogService.Object, _ipHistoryService.Object, _imageService.Object, _mailingListService.Object, _eventDefService.Object, _awardDefService.Object, _eventPublisher.Object);
		}

		private class TestableAdminController : AdminController
		{
			public TestableAdminController(IUserService userService, IProfileService profileService, ISettingsManager settingsManager, ICategoryService categoryService, IForumService forumService, ISearchService searchService, ISecurityLogService securityLogService, IErrorLog errorLog, IBanService banService, IModerationLogService modLogService, IIPHistoryService ipHistoryService, IImageService imageService, IMailingListService mailingListService, IEventDefinitionService eventDefinitonService, IAwardDefinitionService awardDefinitionService, IEventPublisher eventPublisher) : base(userService, profileService, settingsManager, categoryService, forumService, searchService, securityLogService, errorLog, banService, modLogService, ipHistoryService, imageService, mailingListService, eventDefinitonService, awardDefinitionService, eventPublisher) { }

			public void SetUser(User user)
			{
				HttpContext.User = user;
			}
		}

		[Test]
		public void SetupIndex()
		{
			var controller = GetController();
			var result = controller.Index();
			Assert.AreEqual(_settings.Object.Current, result.ViewData.Model);
		}

		[Test]
		public void PostIndex()
		{
			var controller = GetController();
			var formCollection = new FormCollection {{"blah", "meh"}};
			var dictionary = new Dictionary<string, object> {{"blah", "meh"}};
			var result = controller.Index(formCollection);
			Assert.AreEqual(_settings.Object.Current, result.ViewData.Model);
			_settings.Verify(s => s.SaveCurrent(dictionary), Times.Once());
		}

		[Test]
		public void SetupEmail()
		{
			var controller = GetController();
			var result = controller.Email();
			Assert.AreEqual(_settings.Object.Current, result.ViewData.Model);
		}

		[Test]
		public void PostEmail()
		{
			var controller = GetController();
			var formCollection = new FormCollection { { "blah", "meh" } };
			var dictionary = new Dictionary<string, object> { { "blah", "meh" } };
			var result = controller.Email(formCollection);
			Assert.AreEqual(_settings.Object.Current, result.ViewData.Model);
			_settings.Verify(s => s.SaveCurrent(dictionary), Times.Once());
		}

		[Test]
		public void SetupCategories()
		{
			var controller = GetController();
			var cats = new List<Category>();
			_catService.Setup(c => c.GetAll()).Returns(cats);
			var result = controller.Categories();
			_catService.Verify(c => c.GetAll(), Times.Once());
			Assert.AreSame(cats, result.ViewData.Model);
		}

		[Test]
		public void AddCategory()
		{
			const string newTitle = "new title";
			var controller = GetController();
			var result = controller.AddCategory(newTitle);
			_catService.Verify(c => c.Create(newTitle), Times.Once());
			Assert.IsInstanceOf<RedirectToRouteResult>(result);
			Assert.AreEqual("Categories", result.RouteValues["Action"]);
		}

		[Test]
		public void DeleteCategory()
		{
			var cat = new Category(123) {Title = "the cat", SortOrder = 2};
			var controller = GetController();
			_catService.Setup(c => c.Get(cat.CategoryID)).Returns(cat);
			var result = controller.DeleteCategory(cat.CategoryID);
			_catService.Verify(c => c.Get(cat.CategoryID), Times.Once());
			_catService.Verify(c => c.Delete(cat), Times.Once());
			Assert.IsInstanceOf<RedirectToRouteResult>(result);
			Assert.AreEqual("Categories", result.RouteValues["Action"]);
		}

		[Test]
		public void GetCategoryList()
		{
			var controller = GetController();
			var list = new List<Category>();
			_catService.Setup(c => c.GetAll()).Returns(list);
			var result = controller.CategoryList();
			_catService.Verify(c => c.GetAll(), Times.Once());
			Assert.AreSame(list, result.ViewData.Model);
		}

		[Test]
		public void MoveCategoryUp()
		{
			var cat = new Category(123) { Title = "the cat", SortOrder = 2 };
			var controller = GetController();
			_catService.Setup(c => c.Get(cat.CategoryID)).Returns(cat);
			controller.MoveCategoryUp(cat.CategoryID);
			_catService.Verify(c => c.MoveCategoryUp(cat), Times.Once());
		}

		[Test]
		public void MoveCategoryUpDoesNotExist()
		{
			var controller = GetController();
			_catService.Setup(c => c.Get(It.IsAny<int>())).Returns((Category)null);
			var result = controller.MoveCategoryUp(123);
			_catService.Verify(c => c.MoveCategoryUp(It.IsAny<Category>()), Times.Never());
			Assert.IsFalse(((BasicJsonMessage) result.Data).Result);
		}

		[Test]
		public void MoveCategoryDown()
		{
			var cat = new Category(123) { Title = "the cat", SortOrder = 2 };
			var controller = GetController();
			_catService.Setup(c => c.Get(cat.CategoryID)).Returns(cat);
			controller.MoveCategoryDown(cat.CategoryID);
			_catService.Verify(c => c.MoveCategoryDown(cat), Times.Once());
		}

		[Test]
		public void MoveCategoryDownDoesNotExist()
		{
			var controller = GetController();
			_catService.Setup(c => c.Get(It.IsAny<int>())).Returns((Category)null);
			var result = controller.MoveCategoryDown(123);
			_catService.Verify(c => c.MoveCategoryDown(It.IsAny<Category>()), Times.Never());
			Assert.IsFalse(((BasicJsonMessage)result.Data).Result);
		}

		[Test]
		public void EditCategoryView()
		{
			var cat = new Category(123) { Title = "the cat", SortOrder = 2 };
			var controller = GetController();
			_catService.Setup(c => c.Get(cat.CategoryID)).Returns(cat);
			var result = controller.EditCategory(cat.CategoryID);
			_catService.Verify(c => c.Get(cat.CategoryID), Times.Once());
			Assert.AreSame(cat, result.ViewData.Model);
		}

		[Test]
		public void EditCategory()
		{
			var cat = new Category(123) { Title = "the cat", SortOrder = 2 };
			var controller = GetController();
			_catService.Setup(c => c.Get(cat.CategoryID)).Returns(cat);
			var result = controller.EditCategory(cat.CategoryID, "blah");
			_catService.Verify(c => c.Get(cat.CategoryID), Times.Once());
			_catService.Verify(c => c.UpdateTitle(cat, "blah"), Times.Once());
			Assert.IsInstanceOf<RedirectToRouteResult>(result);
		}

		[Test]
		public void Forums()
		{
			var controller = GetController();
			var container = new CategorizedForumContainer(new List<Category>(), new List<Forum>());
			_forumService.Setup(f => f.GetCategorizedForumContainer()).Returns(container);
			var result = controller.Forums();
			_forumService.Verify(f => f.GetCategorizedForumContainer(), Times.Once());
			Assert.AreSame(container, result.ViewData.Model);
		}

		[Test]
		public void AddForumView()
		{
			var controller = GetController();
			_catService.Setup(c => c.GetAll()).Returns(new List<Category>());
			var result = controller.AddForum();
			Assert.NotNull(result.ViewData["categoryID"]);
			Assert.IsInstanceOf<SelectList>(result.ViewData["categoryID"]);
			_catService.Verify(c => c.GetAll(), Times.Once());
		}

		[Test]
		public void AddForumPost()
		{
			var controller = GetController();
			_forumService.Setup(f => f.Create(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), true)).Returns(new Forum(123));
			var result = controller.AddForum(123, "title", "desc", true, false, ".", true);
			_forumService.Verify(f => f.Create(123, "title", "desc", true, false, -2, ".", true), Times.Once());
			Assert.AreEqual("Forums", result.RouteValues["action"]);
			Assert.IsInstanceOf<RedirectToRouteResult>(result);
		}

		[Test]
		public void EditForumView()
		{
			var forum = new Forum(123);
			var controller = GetController();
			_forumService.Setup(f => f.Get(forum.ForumID)).Returns(forum);
			_catService.Setup(c => c.GetAll()).Returns(new List<Category>());
			var result = controller.EditForum(forum.ForumID);
			_forumService.Verify(f => f.Get(forum.ForumID), Times.Once());
			Assert.AreSame(forum, result.ViewData.Model);
		}

		[Test]
		public void EditForumPost()
		{
			var forum = new Forum(123);
			var controller = GetController();
			_forumService.Setup(f => f.Get(forum.ForumID)).Returns(forum);
			var result = controller.EditForum(forum.ForumID, null, "title", "desc", true, true, ".", true);
			_forumService.Verify(f => f.Update(forum, null, "title", "desc", true, true, ".", true), Times.Once());
			Assert.AreEqual("Forums", result.RouteValues["action"]);
			Assert.IsInstanceOf<RedirectToRouteResult>(result);
		}

		[Test]
		public void CategorizedForums()
		{
			var controller = GetController();
			var container = new CategorizedForumContainer(new List<Category>(), new List<Forum>());
			_forumService.Setup(f => f.GetCategorizedForumContainer()).Returns(container);
			var result = controller.CategorizedForums();
			_forumService.Verify(f => f.GetCategorizedForumContainer(), Times.Once());
			Assert.AreSame(container, result.ViewData.Model);
		}

		[Test]
		public void MoveForumUp()
		{
			var forum = new Forum(123);
			var controller = GetController();
			_forumService.Setup(f => f.Get(forum.ForumID)).Returns(forum);
			controller.MoveForumUp(forum.ForumID);
			_forumService.Verify(c => c.MoveForumUp(forum), Times.Once());
		}

		[Test]
		public void MoveForumUpDoesNotExist()
		{
			var controller = GetController();
			_forumService.Setup(c => c.Get(It.IsAny<int>())).Returns((Forum)null);
			var result = controller.MoveForumUp(123);
			_forumService.Verify(c => c.MoveForumUp(It.IsAny<Forum>()), Times.Never());
			Assert.IsFalse(((BasicJsonMessage)result.Data).Result);
		}

		[Test]
		public void MoveForumDown()
		{
			var forum = new Forum(123);
			var controller = GetController();
			_forumService.Setup(f => f.Get(forum.ForumID)).Returns(forum);
			controller.MoveForumDown(forum.ForumID);
			_forumService.Verify(c => c.MoveForumDown(forum), Times.Once());
		}

		[Test]
		public void MoveForumDownDoesNotExist()
		{
			var controller = GetController();
			_forumService.Setup(c => c.Get(It.IsAny<int>())).Returns((Forum)null);
			var result = controller.MoveForumDown(123);
			_forumService.Verify(c => c.MoveForumDown(It.IsAny<Forum>()), Times.Never());
			Assert.IsFalse(((BasicJsonMessage)result.Data).Result);
		}

		[Test]
		public void ForumPermissions()
		{
			var controller = GetController();
			var container = new CategorizedForumContainer(new List<Category>(), new List<Forum>());
			_forumService.Setup(f => f.GetCategorizedForumContainer()).Returns(container);
			var result = controller.ForumPermissions();
			_forumService.Verify(f => f.GetCategorizedForumContainer(), Times.Once());
			Assert.AreSame(container, result.ViewData.Model);
		}

		[Test]
		public void ForumRoles()
		{
			var controller = GetController();
			var roles = new List<string> {"1", "2"};
			var viewRoles = new List<string> {"3"};
			var postRoles = new List<string> {"4"};
			var forum = new Forum(123);
			_forumService.Setup(f => f.Get(forum.ForumID)).Returns(forum);
			_userService.Setup(u => u.GetAllRoles()).Returns(roles);
			_forumService.Setup(f => f.GetForumPostRoles(forum)).Returns(postRoles);
			_forumService.Setup(f => f.GetForumViewRoles(forum)).Returns(viewRoles);

			var result = controller.ForumRoles(forum.ForumID);

			_forumService.Verify(f => f.Get(forum.ForumID), Times.Once());
			_userService.Verify(u => u.GetAllRoles(), Times.Once());
			_forumService.Verify(f => f.GetForumPostRoles(forum), Times.Once());
			_forumService.Verify(f => f.GetForumViewRoles(forum), Times.Once());
			var container = (ForumPermissionContainer)result.Data;
			Assert.AreEqual(forum.ForumID, container.ForumID);
			Assert.AreSame(roles, container.AllRoles);
			Assert.AreSame(viewRoles, container.ViewRoles);
			Assert.AreSame(postRoles, container.PostRoles);
		}

		[Test]
		public void ModifyForumFail()
		{
			var controller = GetController();
			_forumService.Setup(f => f.Get(It.IsAny<int>())).Returns((Forum) null);
			Assert.Throws<Exception>(() => controller.ModifyForumRoles(1, AdminController.ModifyForumRolesType.AddView));
		}

		[Test]
		public void ModifyAddPostRole()
		{
			var forum = new Forum(1);
			var controller = GetController();
			_forumService.Setup(f => f.Get(1)).Returns(forum);
			controller.ModifyForumRoles(1, AdminController.ModifyForumRolesType.AddPost, "blah");
			_forumService.Verify(f => f.AddPostRole(forum, "blah"), Times.Once());
		}

		[Test]
		public void ModifyRemovePostRole()
		{
			var forum = new Forum(1);
			var controller = GetController();
			_forumService.Setup(f => f.Get(1)).Returns(forum);
			controller.ModifyForumRoles(1, AdminController.ModifyForumRolesType.RemovePost, "blah");
			_forumService.Verify(f => f.RemovePostRole(forum, "blah"), Times.Once());
		}

		[Test]
		public void ModifyAddViewRole()
		{
			var forum = new Forum(1);
			var controller = GetController();
			_forumService.Setup(f => f.Get(1)).Returns(forum);
			controller.ModifyForumRoles(1, AdminController.ModifyForumRolesType.AddView, "blah");
			_forumService.Verify(f => f.AddViewRole(forum, "blah"), Times.Once());
		}

		[Test]
		public void ModifyRemoveViewRole()
		{
			var forum = new Forum(1);
			var controller = GetController();
			_forumService.Setup(f => f.Get(1)).Returns(forum);
			controller.ModifyForumRoles(1, AdminController.ModifyForumRolesType.RemoveView, "blah");
			_forumService.Verify(f => f.RemoveViewRole(forum, "blah"), Times.Once());
		}

		[Test]
		public void ModifyRemoveAllViewRoles()
		{
			var forum = new Forum(1);
			var controller = GetController();
			_forumService.Setup(f => f.Get(1)).Returns(forum);
			controller.ModifyForumRoles(1, AdminController.ModifyForumRolesType.RemoveAllView);
			_forumService.Verify(f => f.RemoveAllViewRoles(forum), Times.Once());
		}

		[Test]
		public void ModifyRemoveAllPostRoles()
		{
			var forum = new Forum(1);
			var controller = GetController();
			_forumService.Setup(f => f.Get(1)).Returns(forum);
			controller.ModifyForumRoles(1, AdminController.ModifyForumRolesType.RemoveAllPost);
			_forumService.Verify(f => f.RemoveAllPostRoles(forum), Times.Once());
		}

		[Test]
		public void UserRoles()
		{
			var controller = GetController();
			var list = new List<string>();
			_userService.Setup(u => u.GetAllRoles()).Returns(list);
			var result = controller.UserRoles();
			_userService.Verify(u => u.GetAllRoles(), Times.Once());
			Assert.AreSame(list, result.ViewData.Model);
		}

		[Test]
		public void CreateRole()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(r => r.UserHostAddress).Returns("123");
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.CreateRole("blah");
			_userService.Verify(u => u.CreateRole("blah", It.IsAny<User>(), It.IsAny<string>()), Times.Once());
			Assert.IsInstanceOf<RedirectToRouteResult>(result);
		}

		[Test]
		public void DeleteRole()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(r => r.UserHostAddress).Returns("123");
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.DeleteRole("blah");
			_userService.Verify(u => u.DeleteRole("blah", It.IsAny<User>(), It.IsAny<string>()), Times.Once());
			Assert.IsInstanceOf<RedirectToRouteResult>(result);
		}

		[Test]
		public void Search()
		{
			var controller = GetController();
			const int interval = 123;
			_settings.Setup(s => s.Current.SearchIndexingInterval).Returns(interval);
			var list = new List<string>();
			_searchService.Setup(s => s.GetJunkWords()).Returns(list);
			var result = controller.Search();
			Assert.AreEqual(interval, result.ViewData["Interval"]);
			Assert.AreSame(list, result.ViewData["JunkWords"]);
		}

		[Test]
		public void PostSearch()
		{
			var controller = GetController();
			var formCollection = new FormCollection { { "blah", "meh" } };
			var dictionary = new Dictionary<string, object> { { "blah", "meh" } };
			controller.Search(formCollection);
			_settings.Verify(s => s.SaveCurrent(dictionary), Times.Once());
		}

		[Test]
		public void CreateJunkWord()
		{
			var controller = GetController();
			var result = controller.CreateJunkWord("blah");
			_searchService.Verify(s => s.CreateJunkWord("blah"), Times.Once());
			Assert.IsInstanceOf<RedirectToRouteResult>(result);
		}

		[Test]
		public void DeleteJunkWord()
		{
			var controller = GetController();
			var result = controller.DeleteJunkWord("blah");
			_searchService.Verify(s => s.DeleteJunkWord("blah"), Times.Once());
			Assert.IsInstanceOf<RedirectToRouteResult>(result);
		}

		[Test]
		public void EditUserSearchPersistsValues()
		{
			var controller = GetController();
			var search = new UserSearch {SearchText = "blah", SearchType = UserSearch.UserSearchType.Name};
			var result = controller.EditUserSearch(search);
			Assert.AreEqual("blah", result.ViewData["SearchText"]);
			Assert.AreEqual("Name", result.ViewData["UserSearchType"].ToString());
		}

		[Test]
		public void EditUserSearchName()
		{
			var controller = GetController();
			var list = new List<User>();
			var search = new UserSearch { SearchText = "blah", SearchType = UserSearch.UserSearchType.Name };
			_userService.Setup(u => u.SearchByName("blah")).Returns(list);
			var result = controller.EditUserSearch(search);
			_userService.Verify(u => u.SearchByName("blah"), Times.Once());
			Assert.AreSame(list, result.ViewData.Model);
		}

		[Test]
		public void EditUserSearchEmail()
		{
			var controller = GetController();
			var list = new List<User>();
			var search = new UserSearch { SearchText = "blah", SearchType = UserSearch.UserSearchType.Email };
			_userService.Setup(u => u.SearchByEmail("blah")).Returns(list);
			var result = controller.EditUserSearch(search);
			_userService.Verify(u => u.SearchByEmail("blah"), Times.Once());
			Assert.AreSame(list, result.ViewData.Model);
		}

		[Test]
		public void EditUserSearchRole()
		{
			var controller = GetController();
			var list = new List<User>();
			var search = new UserSearch { SearchText = "blah", SearchType = UserSearch.UserSearchType.Role };
			_userService.Setup(u => u.SearchByRole("blah")).Returns(list);
			var result = controller.EditUserSearch(search);
			_userService.Verify(u => u.SearchByRole("blah"), Times.Once());
			Assert.AreSame(list, result.ViewData.Model);
		}

		[Test]
		public void EditUserView()
		{
			var controller = GetController();
			var user = new User(1, new DateTime(2000,1,1));
			user.Roles = new List<string>();
			_userService.Setup(u => u.GetUser(1)).Returns(user);
			_profileService.Setup(p => p.GetProfileForEdit(user)).Returns(new Profile(1));
			controller.EditUser(1);
			_userService.Verify(u => u.GetUser(1), Times.Once());
			_profileService.Verify(p => p.GetProfileForEdit(user), Times.Once());
		}

		[Test]
		public void EditUser()
		{
			var controller = GetController();
			var context = new HttpContextHelper();
			var mockFileCollection = new Mock<HttpFileCollectionBase>();
			var avatar = new Mock<HttpPostedFileBase>();
			var photo = new Mock<HttpPostedFileBase>();
			mockFileCollection.Setup(f => f["avatarFile"]).Returns(avatar.Object);
			mockFileCollection.Setup(f => f["photoFile"]).Returns(photo.Object);
			context.MockRequest.Setup(r => r.Files).Returns(mockFileCollection.Object);
			controller.ControllerContext = new ControllerContext(context.MockContext.Object, new RouteData(), controller);
			var targetUser = new User(1, DateTime.MinValue);
			var user = new User(2, DateTime.MinValue);
			var userEdit = new UserEdit {DeleteAvatar = true, DeleteImage = true};
			_userService.Setup(u => u.GetUser(1)).Returns(targetUser);
			controller.SetUser(user);
			controller.EditUser(1, userEdit);
			_userService.Verify(u => u.EditUser(targetUser, userEdit, true, true, avatar.Object, photo.Object, It.IsAny<string>(), user), Times.Once());
		}

		[Test]
		public void DeleteUser()
		{
			var controller = GetController();
			var context = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(context.MockContext.Object, new RouteData(), controller);
			var targetUser = new User(1, DateTime.MinValue);
			var user = new User(2, DateTime.MinValue);
			_userService.Setup(u => u.GetUser(1)).Returns(targetUser);
			controller.SetUser(user);
			controller.DeleteUser(1);
			_userService.Verify(u => u.DeleteUser(targetUser, user, It.IsAny<string>(), false));
		}

		[Test]
		public void DeleteAndBanUser()
		{
			var controller = GetController();
			var context = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(context.MockContext.Object, new RouteData(), controller);
			var targetUser = new User(1, DateTime.MinValue);
			var user = new User(2, DateTime.MinValue);
			_userService.Setup(u => u.GetUser(1)).Returns(targetUser);
			controller.SetUser(user);
			controller.DeleteAndBanUser(1);
			_userService.Verify(u => u.DeleteUser(targetUser, user, It.IsAny<string>(), true));
		}

		[Test]
		public void EditUserFailNoUser()
		{
			var controller = GetController();
			_userService.Setup(u => u.GetUser(1)).Returns((User)null);
			Assert.Throws<Exception>(() => controller.EditUser(1));
		}

		[Test]
		public void SecurityLogName()
		{
			var controller = GetController();
			var list = new List<SecurityLogEntry>();
			_securityLogService.Setup(s => s.GetLogEntriesByUserName("blah", DateTime.MinValue, DateTime.MaxValue)).Returns(list);
			var result = controller.SecurityLog(DateTime.MinValue, DateTime.MaxValue, "Name", "blah");
			_securityLogService.Verify(s => s.GetLogEntriesByUserName("blah", DateTime.MinValue, DateTime.MaxValue), Times.Once());
			Assert.AreSame(list, result.ViewData.Model);
		}

		[Test]
		public void SecurityLogUserID()
		{
			var controller = GetController();
			var list = new List<SecurityLogEntry>();
			_securityLogService.Setup(s => s.GetLogEntriesByUserID(1, DateTime.MinValue, DateTime.MaxValue)).Returns(list);
			var result = controller.SecurityLog(DateTime.MinValue, DateTime.MaxValue, "UserID", "1");
			_securityLogService.Verify(s => s.GetLogEntriesByUserID(1, DateTime.MinValue, DateTime.MaxValue), Times.Once());
			Assert.AreSame(list, result.ViewData.Model);
		}

		[Test]
		public void SecurityLogFail()
		{
			var controller = GetController();
			Assert.Throws<ArgumentOutOfRangeException>(() => controller.SecurityLog(DateTime.MinValue, DateTime.MaxValue, "badsearchtype", "ohnoes!"));
		}

		[Test]
		public void EventDefsGetsAllFromService()
		{
			var controller = GetController();
			var list = new List<EventDefinition>();
			_eventDefService.Setup(x => x.GetAll()).Returns(list);
			var result = controller.EventDefinitions();
			Assert.AreSame(list, result.Model);
		}

		[Test]
		public void AddEventCallsAdd()
		{
			var controller = GetController();
			var eventDef = new EventDefinition();
			controller.AddEvent(eventDef);
			_eventDefService.Verify(x => x.Create(eventDef), Times.Once());
		}

		[Test]
		public void DeleteEventCallsDelete()
		{
			var controller = GetController();
			controller.DeleteEvent("goaway");
			_eventDefService.Verify(x => x.Delete("goaway"), Times.Once());
		}

		[Test]
		public void AwardDefsGetsAllFromService()
		{
			var controller = GetController();
			var list = new List<AwardDefinition>();
			_awardDefService.Setup(x => x.GetAll()).Returns(list);
			var result = controller.AwardDefinitions();
			Assert.AreSame(list, result.Model);
		}

		[Test]
		public void AddAwardCallsAdd()
		{
			var controller = GetController();
			var awardDef = new AwardDefinition();
			controller.AddAward(awardDef);
			_awardDefService.Verify(x => x.Create(awardDef), Times.Once());
		}

		[Test]
		public void DeleteAwardCallsDelete()
		{
			var controller = GetController();
			controller.DeleteAward("bye");
			_awardDefService.Verify(x => x.Delete("bye"), Times.Once());
		}

		[Test]
		public void AwardGetsFromRepo()
		{
			var controller = GetController();
			var awardDef = new AwardDefinition {AwardDefinitionID = "qwerty"};
			_awardDefService.Setup(x => x.Get(awardDef.AwardDefinitionID)).Returns(awardDef);
			_eventDefService.Setup(x => x.GetAll()).Returns(new List<EventDefinition>());
			var result = controller.Award(awardDef.AwardDefinitionID);
			Assert.AreSame(awardDef, result.Model);
		}

		[Test]
		public void AwardGetsSelectListOfEvents()
		{
			var controller = GetController();
			var events = new List<EventDefinition>();
			_eventDefService.Setup(x => x.GetAll()).Returns(events);
			_awardDefService.Setup(x => x.Get(It.IsAny<string>())).Returns(new AwardDefinition());
			var result = controller.Award("qwerty");
			var selectList = (SelectList) result.ViewBag.EventList;
			Assert.AreSame(events, selectList.Items);
		}

		[Test]
		public void AwardGetsConditions()
		{
			var controller = GetController();
			var awardDef = new AwardDefinition { AwardDefinitionID = "qwerty" };
			_awardDefService.Setup(x => x.Get(awardDef.AwardDefinitionID)).Returns(awardDef);
			_eventDefService.Setup(x => x.GetAll()).Returns(new List<EventDefinition>());
			var conditions = new List<AwardCondition>();
			_awardDefService.Setup(x => x.GetConditions(awardDef.AwardDefinitionID)).Returns(conditions);
			var result = controller.Award(awardDef.AwardDefinitionID);
			Assert.AreSame(conditions, result.ViewBag.Conditions);
		}

		[Test]
		public void DeleteAwardConditionCallsRepo()
		{
			var controller = GetController();
			var condition = new AwardCondition {AwardDefinitionID = "qwert", EventDefinitionID = "oiu"};
			controller.DeleteAwardCondition(condition.AwardDefinitionID, condition.EventDefinitionID);
			_awardDefService.Verify(x => x.DeleteCondition(condition.AwardDefinitionID, condition.EventDefinitionID), Times.Once());
		}

		[Test]
		public void AddAwardConditionCallsRepo()
		{
			var controller = GetController();
			var condition = new AwardCondition();
			controller.AddAwardCondition(condition);
			_awardDefService.Verify(x => x.AddCondition(condition), Times.Once());
		}
	}
}
