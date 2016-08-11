using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PopForums.Models;
using PopForums.Services;
using PopForums.Web.Areas.Forums.Services;

namespace PopForums.Web.Areas.Forums.Controllers
{
	[Area("Forums")]
	public class PrivateMessagesController : Controller
	{
		public PrivateMessagesController(IPrivateMessageService privateMessageService, IUserService userService, IUserRetrievalShim userRetrievalShim)
		{
			_privateMessageService = privateMessageService;
			_userService = userService;
			_userRetrievalShim = userRetrievalShim;
		}

		private readonly IPrivateMessageService _privateMessageService;
		private readonly IUserService _userService;
		private readonly IUserRetrievalShim _userRetrievalShim;

		public static string Name = "PrivateMessages";

		public ActionResult Index(int page = 1)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return Forbid();
			PagerContext pagerContext;
			var privateMessages = _privateMessageService.GetPrivateMessages(user, PrivateMessageBoxType.Inbox, page, out pagerContext);
			ViewBag.PagerContext = pagerContext;
			return View(privateMessages);
		}

		public ActionResult Archive(int page = 1)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return Forbid();
			PagerContext pagerContext;
			var privateMessages = _privateMessageService.GetPrivateMessages(user, PrivateMessageBoxType.Archive, page, out pagerContext);
			ViewBag.PagerContext = pagerContext;
			return View(privateMessages);
		}

		public ActionResult View(int id)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return Forbid();
			var pm = _privateMessageService.Get(id);
			if (!_privateMessageService.IsUserInPM(user, pm))
				return Forbid();
			var model = new PrivateMessageView
			{
				PrivateMessage = pm,
				Posts = _privateMessageService.GetPosts(pm)
			};
			_privateMessageService.MarkPMRead(user, pm);
			return View(model);
		}

		public ActionResult Create(int? id)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return Forbid();
			ViewBag.UserIDs = " ";
			if (id.HasValue)
			{
				var targetUser = _userService.GetUser(id.Value);
				ViewBag.UserIDs = targetUser.UserID.ToString(CultureInfo.InvariantCulture);
				ViewBag.UserID = targetUser.UserID.ToString(CultureInfo.InvariantCulture);
				ViewBag.TargetUserID = targetUser.UserID;
				ViewBag.TargetUserName = targetUser.Name;
			}
			return View();
		}

		[HttpPost]
		public ActionResult CreateOne(string subject, string fullText, int userID)
		{
			return Create(subject, fullText, userID.ToString(CultureInfo.InvariantCulture));
		}

		[HttpPost]
		public ActionResult Create(string subject, string fullText, string userIDs)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return Forbid();
			if (String.IsNullOrWhiteSpace(userIDs) || String.IsNullOrWhiteSpace(subject) || String.IsNullOrWhiteSpace(fullText))
			{
				ViewBag.Warning = Resources.PMCreateWarnings;
				return View("Create");
			}
			var ids = userIDs.Split(new[] { ',' }).Select(i => Convert.ToInt32(i));
			var users = ids.Select(id => _userService.GetUser(id)).ToList();
			_privateMessageService.Create(subject, fullText, user, users);
			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult Reply(int id, string fullText)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return Forbid();
			var pm = _privateMessageService.Get(id);
			if (!_privateMessageService.IsUserInPM(user, pm))
				return Forbid();
			_privateMessageService.Reply(pm, fullText, user);
			return RedirectToAction("View", new { id });
		}

		public JsonResult GetNames(string id)
		{
			var users = _userService.SearchByName(id);
			var projection = users.Select(u => new { u.UserID, value = u.Name });
			return Json(projection);
		}

		[HttpPost]
		public ActionResult ArchivePM(int id)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return Forbid();
			var pm = _privateMessageService.Get(id);
			if (!_privateMessageService.IsUserInPM(user, pm))
				return Forbid();
			_privateMessageService.Archive(user, pm);
			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult UnarchivePM(int id)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return Forbid();
			var pm = _privateMessageService.Get(id);
			if (!_privateMessageService.IsUserInPM(user, pm))
				return Forbid();
			_privateMessageService.Unarchive(user, pm);
			return RedirectToAction("Archive");
		}

		public ContentResult NewPMCount()
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return Content(String.Empty);
			var count = _privateMessageService.GetUnreadCount(user);
			return Content(count.ToString());
		}
	}
}
