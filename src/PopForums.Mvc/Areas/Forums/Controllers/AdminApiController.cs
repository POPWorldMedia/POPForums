using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Mvc.Areas.Forums.Authorization;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Controllers
{
	[Authorize(Policy = PermanentRoles.Admin, AuthenticationSchemes = PopForumsAuthorizationDefaults.AuthenticationScheme)]
	[Area("Forums")]
	[Produces("application/json")]
	[ApiController]
	public class AdminApiController : ControllerBase
	{
		private readonly ISettingsManager _settingsManager;
		private readonly ICategoryService _categoryService;
		private readonly IForumService _forumService;

		public AdminApiController(ISettingsManager settingsManager, ICategoryService categoryService, IForumService forumService)
		{
			_settingsManager = settingsManager;
			_categoryService = categoryService;
			_forumService = forumService;
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

		// ********** settings

		[HttpGet("/Forums/AdminApi/GetForums")]
		public ActionResult<CategorizedForumContainer> GetForums()
		{
			var forums = _forumService.GetCategorizedForumContainer();
			return forums;
		}
	}
}
