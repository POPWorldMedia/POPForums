using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PopForums.Configuration;

namespace PopForums.Mvc.Areas.Forums.Authorization
{
	public class PopForumsExternalLoginOnlyFilter : IActionFilter
	{
		private readonly IConfig _config;

		public PopForumsExternalLoginOnlyFilter(IConfig config)
		{
			_config = config;
		}

		public void OnActionExecuting(ActionExecutingContext context)
		{
			if (_config.ExternalLoginOnly)
				context.Result = new BadRequestResult();
		}

		public void OnActionExecuted(ActionExecutedContext context)
		{
		}
	}
}