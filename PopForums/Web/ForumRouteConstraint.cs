using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using PopForums.Repositories;

namespace PopForums.Web
{
	public class ForumRouteConstraint : IRouteConstraint
	{
		public ForumRouteConstraint(IForumRepository forumRepository)
		{
			_forumRepository = forumRepository;
		}

		private readonly IForumRepository _forumRepository;

		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			if (!values.Keys.Contains("urlName"))
				return false;
			IEnumerable<String> forumUrlNames;
			try
			{
				forumUrlNames = _forumRepository.GetAllForumUrlNames();
			}
			catch
			{
				throw new Exception("Can't read forum URL names from data store.");
			}
			if (forumUrlNames.Contains(values["urlName"]))
				return true;
			return false;
		}
	}
}