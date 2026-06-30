namespace PopForums.Mvc.Areas.Forums;

public class ForumRouteConstraint : IRouteConstraint
{
	public ForumRouteConstraint(IForumRepository forumRepository)
	{
		_forumRepository = forumRepository;
	}

	private readonly IForumRepository _forumRepository;

	public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values,
		RouteDirection routeDirection)
	{
		if (!values.Keys.Contains("urlName"))
			return false;
		var forumUrlNames = _forumRepository.GetAllForumUrlNames();
		return forumUrlNames.Contains(values["urlName"]);
	}
}