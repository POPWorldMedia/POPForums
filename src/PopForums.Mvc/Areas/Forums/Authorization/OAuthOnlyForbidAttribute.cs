namespace PopForums.Mvc.Areas.Forums.Authorization;

public class OAuthOnlyForbidAttribute : Attribute, IResourceFilter
{
	private readonly IConfig _config;

	public OAuthOnlyForbidAttribute(IConfig config)
	{
		_config = config;
	}

	public void OnResourceExecuting(ResourceExecutingContext context)
	{
		if (_config.IsOAuthOnly)
			context.Result = new ForbidResult();
	}

	public void OnResourceExecuted(ResourceExecutedContext context)
	{
	}
}