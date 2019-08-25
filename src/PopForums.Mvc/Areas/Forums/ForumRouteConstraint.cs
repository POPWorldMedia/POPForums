using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PopForums.Repositories;

namespace PopForums.Mvc.Areas.Forums
{
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
			IEnumerable<String> forumUrlNames;
			try
			{
				forumUrlNames = _forumRepository.GetAllForumUrlNames().Result;
			}
			catch (Exception exc)
			{
				throw new Exception("Can't read forum URL names from data store.", exc);
			}
			if (forumUrlNames.Contains(values["urlName"]))
				return true;
			return false;
		}
	}
}