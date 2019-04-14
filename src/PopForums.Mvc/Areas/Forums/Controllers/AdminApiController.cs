using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopForums.Models;
using PopForums.Mvc.Areas.Forums.Authorization;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Controllers
{
	[Authorize(Policy = PermanentRoles.Admin, AuthenticationSchemes = PopForumsAuthorizationDefaults.AuthenticationScheme)]
	[Area("Forums")]
	[Produces("application/json")]
	public class AdminApiController : ControllerBase
	{
		private readonly ICategoryService _categoryService;

		public AdminApiController(ICategoryService categoryService)
		{
			_categoryService = categoryService;
		}

		// ********** categories

		[HttpGet]
		public ActionResult<List<Category>> GetCategories()
		{
			var categories = _categoryService.GetAll();
			return categories;
		}

		[HttpPost]
		public ActionResult<List<Category>> AddCategory(string title)
		{
			_categoryService.Create(title);
			var categories = _categoryService.GetAll();
			return categories;
		}

		[HttpPost]
		public ActionResult<List<Category>> DeleteCategory(int id)
		{
			_categoryService.Delete(id);
			var categories = _categoryService.GetAll();
			return categories;
		}

		[HttpPost]
		public ActionResult<List<Category>> MoveCategoryUp(int id)
		{
			_categoryService.MoveCategoryUp(id);
			var categories = _categoryService.GetAll();
			return categories;
		}

		[HttpPost]
		public ActionResult<List<Category>> MoveCategoryDown(int id)
		{
			_categoryService.MoveCategoryDown(id);
			var categories = _categoryService.GetAll();
			return categories;
		}

		[HttpPost]
		public ActionResult<List<Category>> EditCategory(int categoryID, string newTitle)
		{
			_categoryService.UpdateTitle(categoryID, newTitle);
			var categories = _categoryService.GetAll();
			return categories;
		}
	}
}
