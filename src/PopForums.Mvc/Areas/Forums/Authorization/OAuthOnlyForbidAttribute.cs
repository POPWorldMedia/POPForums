namespace PopForums.Mvc.Areas.Forums.Authorization;

public class OAuthOnlyForbidAttribute(IConfig config) : Attribute, IResourceFilter
{
	public void OnResourceExecuting(ResourceExecutingContext context)
	{
		if (config.IsOAuthOnly)
			context.Result = new ForbidResult();
	}

	public void OnResourceExecuted(ResourceExecutedContext context)
	{
	}
}