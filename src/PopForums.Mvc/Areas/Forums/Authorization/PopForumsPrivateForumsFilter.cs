using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PopForums.Configuration;
using PopForums.Mvc.Areas.Forums.Controllers;
using PopForums.Mvc.Areas.Forums.Services;

namespace PopForums.Mvc.Areas.Forums.Authorization
{
	public class PopForumsPrivateForumsFilter : IActionFilter
	{
		private readonly IUserRetrievalShim _userRetrievalShim;
		private readonly ISettingsManager _settingsManager;

		public PopForumsPrivateForumsFilter(IUserRetrievalShim userRetrievalShim, ISettingsManager settingsManager)
		{
			_userRetrievalShim = userRetrievalShim;
			_settingsManager = settingsManager;
		}

		public void OnActionExecuting(ActionExecutingContext context)
		{
			if (!_settingsManager.Current.IsPrivateForumInstance)
				return;
			if (_userRetrievalShim.GetUser() == null)
				context.Result = new RedirectToActionResult("Login", AccountController.Name, null);
		}

		public void OnActionExecuted(ActionExecutedContext context)
		{
		}
	}
}