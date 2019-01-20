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

		[HttpGet]
		public ActionResult<List<Category>> GetCategories()
		{
			var categories = _categoryService.GetAll();
			return categories;
		}
	}
}
