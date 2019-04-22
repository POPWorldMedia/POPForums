using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Mvc.Areas.Forums.Authorization;
using PopForums.Mvc.Areas.Forums.Models;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Controllers
{
	[Authorize(Policy = PermanentRoles.Admin, AuthenticationSchemes = PopForumsAuthorizationDefaults.AuthenticationScheme)]
	[Area("Forums")]
	[Produces("application/json")]
	[ApiController]
	public class AdminApiController : Controller
	{
		private readonly ISettingsManager _settingsManager;
		private readonly ICategoryService _categoryService;
		private readonly IForumService _forumService;
		private readonly IUserService _userService;
		private readonly ISearchService _searchService;
		private readonly IProfileService _profileService;

		public AdminApiController(ISettingsManager settingsManager, ICategoryService categoryService, IForumService forumService, IUserService userService, ISearchService searchService, IProfileService profileService)
		{
			_settingsManager = settingsManager;
			_categoryService = categoryService;
			_forumService = forumService;
			_userService = userService;
			_searchService = searchService;
			_profileService = profileService;
		}

		// ********** settings

		[HttpGet("/Forums/AdminApi/GetSettings")]
		public ActionResult<Settings> GetSettings()
		{
			var settings = _settingsManager.Current;
			return settings;
		}

		[HttpPost("/Forums/AdminApi/SaveSettings")]
		public ActionResult<Settings> SaveSettings([FromBody]Settings settings)
		{
			_settingsManager.Save(settings);
			var newSettings = _settingsManager.Current;
			return newSettings;
		}

		// ********** categories

		[HttpGet("/Forums/AdminApi/GetCategories")]
		public ActionResult<List<Category>> GetCategories()
		{
			var categories = _categoryService.GetAll();
			return categories;
		}

		[HttpPost("/Forums/AdminApi/AddCategory")]
		public ActionResult<List<Category>> AddCategory([FromBody]Category category)
		{
			_categoryService.Create(category.Title);
			var categories = _categoryService.GetAll();
			return categories;
		}

		[HttpPost("/Forums/AdminApi/DeleteCategory/{id}")]
		public ActionResult<List<Category>> DeleteCategory(int id)
		{
			_categoryService.Delete(id);
			var categories = _categoryService.GetAll();
			return categories;
		}

		[HttpPost("/Forums/AdminApi/MoveCategoryUp/{id}")]
		public ActionResult<List<Category>> MoveCategoryUp(int id)
		{
			_categoryService.MoveCategoryUp(id);
			var categories = _categoryService.GetAll();
			return categories;
		}

		[HttpPost("/Forums/AdminApi/MoveCategoryDown/{id}")]
		public ActionResult<List<Category>> MoveCategoryDown(int id)
		{
			_categoryService.MoveCategoryDown(id);
			var categories = _categoryService.GetAll();
			return categories;
		}

		[HttpPost("/Forums/AdminApi/EditCategory")]
		public ActionResult<List<Category>> EditCategory([FromBody]Category category)
		{
			_categoryService.UpdateTitle(category.CategoryID, category.Title);
			var categories = _categoryService.GetAll();
			return categories;
		}

		// ********** forums

		[HttpGet("/Forums/AdminApi/GetForums")]
		public ActionResult<List<CategoryContainerWithForums>> GetForums()
		{
			var forums = _forumService.GetCategoryContainersWithForums();
			return forums;
		}

		[HttpPost("/Forums/AdminApi/MoveForumUp/{id}")]
		public ActionResult<List<CategoryContainerWithForums>> MoveForumUp(int id)
		{
			_forumService.MoveForumUp(id);
			var forums = _forumService.GetCategoryContainersWithForums();
			return forums;
		}

		[HttpPost("/Forums/AdminApi/MoveForumDown/{id}")]
		public ActionResult<List<CategoryContainerWithForums>> MoveForumDown(int id)
		{
			_forumService.MoveForumDown(id);
			var forums = _forumService.GetCategoryContainersWithForums();
			return forums;
		}

		[HttpPost("/Forums/AdminApi/SaveForum")]
		public ActionResult<List<CategoryContainerWithForums>> SaveForum([FromBody]Forum forumEdit)
		{
			if (forumEdit.CategoryID == 0)
				forumEdit.CategoryID = null;
			if (forumEdit.ForumID == 0)
				_forumService.Create(forumEdit.CategoryID, forumEdit.Title, forumEdit.Description, forumEdit.IsVisible, forumEdit.IsArchived, -1, forumEdit.ForumAdapterName, forumEdit.IsQAForum);
			else
			{
				var forum = _forumService.Get(forumEdit.ForumID);
				if (forum == null)
					return NotFound();
				_forumService.Update(forum, forumEdit.CategoryID, forumEdit.Title, forumEdit.Description, forumEdit.IsVisible, forumEdit.IsArchived, forumEdit.ForumAdapterName, forumEdit.IsQAForum);
			}
			var forums = _forumService.GetCategoryContainersWithForums();
			return forums;
		}

		// ********** forum permissions

		[HttpGet("/Forums/AdminApi/GetForumPermissions/{id}")]
		public ActionResult<ForumPermissionContainer> GetForumPermissions(int id)
		{
			var forum = _forumService.Get(id);
			if (forum == null)
				return NotFound();
			var container = new ForumPermissionContainer
			{
				ForumID = forum.ForumID,
				AllRoles = _userService.GetAllRoles(),
				PostRoles = _forumService.GetForumPostRoles(forum),
				ViewRoles = _forumService.GetForumViewRoles(forum)
			};
			return container;
		}

		[HttpPost("/Forums/AdminApi/ModifyForumRoles")]
		public NoContentResult ModifyForumRoles(ModifyForumRolesContainer container)
		{
			_forumService.ModifyForumRoles(container);
			return NoContent();
		}

		// ********** search

		[HttpGet("/Forums/AdminApi/GetJunkWords")]
		public ActionResult<IEnumerable<string>> GetJunkWords()
		{
			var words = _searchService.GetJunkWords();
			return words;
		}

		[HttpPost("/Forums/AdminApi/CreateJunkWord/{word}")]
		public NoContentResult CreateJunkWord(string word)
		{
			_searchService.CreateJunkWord(word);
			return NoContent();
		}

		[HttpPost("/Forums/AdminApi/DeleteJunkWord/{word}")]
		public NoContentResult DeleteJunkWord(string word)
		{
			_searchService.DeleteJunkWord(word);
			return NoContent();
		}

		// ********** edit user

		[HttpPost("/Forums/AdminApi/EditUserSearch")]
		public ActionResult<List<User>> EditUserSearch(UserSearch userSearch)
		{
			List<User> users;
			switch (userSearch.SearchType)
			{
				case UserSearch.UserSearchType.Email:
					users = _userService.SearchByEmail(userSearch.SearchText);
					break;
				case UserSearch.UserSearchType.Name:
					users = _userService.SearchByName(userSearch.SearchText);
					break;
				case UserSearch.UserSearchType.Role:
					users = _userService.SearchByRole(userSearch.SearchText);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(userSearch));
			}
			return users;
		}

		[HttpGet("/Forums/AdminApi/GetUser/{id}")]
		public ActionResult<UserEdit> GetUser(int id)
		{
			var user = _userService.GetUser(id);
			if (user == null)
				return NotFound();
			var profile = _profileService.GetProfileForEdit(user);
			var model = new UserEdit(user, profile);
			return model;
		}
	}
}
