using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Ninject;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Services;
using PopForums.Web;

namespace PopForums.Controllers
{
	public class SearchController : Controller
	{
		public SearchController()
		{
			var container = PopForumsActivation.Kernel;
			_searchService = container.Get<ISearchService>();
			_forumService = container.Get<IForumService>();
			_lastReadService = container.Get<ILastReadService>();
		}

		protected internal SearchController(ISearchService searchService, IForumService forumService, ILastReadService lastReadService)
		{
			_searchService = searchService;
			_forumService = forumService;
			_lastReadService = lastReadService;
		}

		private readonly ISearchService _searchService;
		private readonly IForumService _forumService;
		private readonly ILastReadService _lastReadService;

		public static string Name = "Search";

		public ViewResult Index()
		{
			var container = new PagedTopicContainer {PagerContext = new PagerContext { PageCount = 0, PageIndex =  1 }, Topics = new List<Topic>() };
			ViewBag.SearchTypes = new SelectList(Enum.GetValues(typeof (SearchType)));
			return View(container);
		}

		[HttpPost]
		public RedirectToRouteResult Process(FormCollection collection)
		{
			var query = collection["Query"];
			var searchType = collection["SearchType"];
			return RedirectToAction("Result", new { query, searchType });
		}

		public ViewResult Result(string query, SearchType searchType, int page = 1)
		{
			ViewBag.SearchTypes = new SelectList(Enum.GetValues(typeof(SearchType)));
			ViewBag.Query = query;
			var includeDeleted = false;
			var user = this.CurrentUser();
			if (user != null && user.IsInRole(PermanentRoles.Moderator))
				includeDeleted = true;
			var titles = _forumService.GetAllForumTitles();
			PagerContext pagerContext;
			var topics = _searchService.GetTopics(query, searchType, user, includeDeleted, page, out pagerContext);
			var container = new PagedTopicContainer { ForumTitles = titles, PagerContext = pagerContext, Topics = topics };
			_lastReadService.GetTopicReadStatus(user, container);
			return View("Index", container);
		}
	}
}
