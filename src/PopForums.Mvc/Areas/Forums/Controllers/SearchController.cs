using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PopForums.Models;
using PopForums.Mvc.Areas.Forums.Services;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Controllers
{
	[Area("Forums")]
	public class SearchController : Controller
	{
		public SearchController(ISearchService searchService, IForumService forumService, ILastReadService lastReadService, IUserRetrievalShim userRetrievalShim)
		{
			_searchService = searchService;
			_forumService = forumService;
			_lastReadService = lastReadService;
			_userRetrievalShim = userRetrievalShim;
		}

		private readonly ISearchService _searchService;
		private readonly IForumService _forumService;
		private readonly ILastReadService _lastReadService;
		private readonly IUserRetrievalShim _userRetrievalShim;

		public static string Name = "Search";

		public ViewResult Index()
		{
			var container = new PagedTopicContainer { PagerContext = new PagerContext { PageCount = 0, PageIndex = 1 }, Topics = new List<Topic>() };
			ViewBag.SearchTypes = new SelectList(Enum.GetValues(typeof(SearchType)));
			return View(container);
		}

		[HttpPost]
		public ActionResult Process(IFormCollection collection)
		{
			var query = Request.Form["Query"];
			var searchType = Request.Form["SearchType"];
			return RedirectToAction("Result", new { query, searchType });
		}
		
		public ViewResult Result(string query, SearchType searchType = SearchType.Rank, int page = 1)
		{
			ViewBag.SearchTypes = new SelectList(Enum.GetValues(typeof(SearchType)));
			ViewBag.Query = query;
			ViewBag.SearchType = searchType;
			var includeDeleted = false;
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user != null && user.IsInRole(PermanentRoles.Moderator))
				includeDeleted = true;
			var titles = _forumService.GetAllForumTitles();
			PagerContext pagerContext;
			var topics = _searchService.GetTopics(query, searchType, user, includeDeleted, page, out pagerContext);
			var container = new PagedTopicContainer { ForumTitles = titles, PagerContext = pagerContext, Topics = topics.Data };
			ViewBag.IsError = !topics.IsValid;
			if (topics.IsValid)
				_lastReadService.GetTopicReadStatus(user, container);
			return View("Index", container);
		}
	}
}