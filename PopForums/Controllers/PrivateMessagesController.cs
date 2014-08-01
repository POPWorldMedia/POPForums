using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Services;
using PopForums.Web;

namespace PopForums.Controllers
{
	public class PrivateMessagesController : Controller
	{
		public PrivateMessagesController()
		{
			var serviceLocator = PopForumsActivation.ServiceLocator;
			_privateMessageService = serviceLocator.GetInstance<IPrivateMessageService>();
			_userService = serviceLocator.GetInstance<IUserService>();
		}

		protected internal PrivateMessagesController(IPrivateMessageService privateMessageService, IUserService userService)
		{
			_privateMessageService = privateMessageService;
			_userService = userService;
		}

		private readonly IPrivateMessageService _privateMessageService;
		private readonly IUserService _userService;

		public static string Name = "PrivateMessages";

		public ViewResult Index(int page = 1)
		{
			var user = this.CurrentUser();
			if (user == null)
				return this.Forbidden("Forbidden", null);
			PagerContext pagerContext;
			var privateMessages = _privateMessageService.GetPrivateMessages(user, PrivateMessageBoxType.Inbox, page, out pagerContext);
			ViewBag.PagerContext = pagerContext;
			return View(privateMessages);
		}

		public ViewResult Archive(int page = 1)
		{
			var user = this.CurrentUser();
			if (user == null)
				return this.Forbidden("Forbidden", null);
			PagerContext pagerContext;
			var privateMessages = _privateMessageService.GetPrivateMessages(user, PrivateMessageBoxType.Archive, page, out pagerContext);
			ViewBag.PagerContext = pagerContext;
			return View(privateMessages);
		}

		public ViewResult View(int id)
		{
			var user = this.CurrentUser();
			if (user == null)
				return this.Forbidden("Forbidden", null);
			var pm = _privateMessageService.Get(id);
			if (!_privateMessageService.IsUserInPM(user, pm))
				return this.Forbidden("Forbidden", null);
			var model = new PrivateMessageView
			            	{
			            		PrivateMessage = pm,
			            		Posts = _privateMessageService.GetPosts(pm)
			            	};
			_privateMessageService.MarkPMRead(user, pm);
			return View(model);
		}

		public ViewResult Create(int? id)
		{
			var user = this.CurrentUser();
			if (user == null)
				return this.Forbidden("Forbidden", null);
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
			var user = this.CurrentUser();
			if (user == null)
				return this.Forbidden("Forbidden", null);
			if (String.IsNullOrWhiteSpace(userIDs) || String.IsNullOrWhiteSpace(subject) || String.IsNullOrWhiteSpace(fullText))
			{
				ViewBag.Warning = Resources.PMCreateWarnings;
				return View("Create");
			}
			var ids = userIDs.Split(new[] {','}).Select(i => Convert.ToInt32(i));
			var users = ids.Select(id => _userService.GetUser(id)).ToList();
			_privateMessageService.Create(subject, fullText, user, users);
			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult Reply(int id, string fullText)
		{
			var user = this.CurrentUser();
			if (user == null)
				return this.Forbidden("Forbidden", null);
			var pm = _privateMessageService.Get(id);
			if (!_privateMessageService.IsUserInPM(user, pm))
				return this.Forbidden("Forbidden", null);
			_privateMessageService.Reply(pm, fullText, user);
			return RedirectToAction("View", new { id });
		}

		public JsonResult GetNames(string id)
		{
			var users = _userService.SearchByName(id);
			var projection = users.Select(u => new {u.UserID, value = u.Name});
			return Json(projection, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult ArchivePM(int id)
		{
			var user = this.CurrentUser();
			if (user == null)
				return this.Forbidden("Forbidden", null);
			var pm = _privateMessageService.Get(id);
			if (!_privateMessageService.IsUserInPM(user, pm))
				return this.Forbidden("Forbidden", null);
			_privateMessageService.Archive(user, pm);
			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult UnarchivePM(int id)
		{
			var user = this.CurrentUser();
			if (user == null)
				return this.Forbidden("Forbidden", null);
			var pm = _privateMessageService.Get(id);
			if (!_privateMessageService.IsUserInPM(user, pm))
				return this.Forbidden("Forbidden", null);
			_privateMessageService.Unarchive(user, pm);
			return RedirectToAction("Archive");
		}

		public ContentResult NewPMCount()
		{
			var user = this.CurrentUser();
			if (user == null)
				return Content(String.Empty);
			var count = _privateMessageService.GetUnreadCount(user);
			return Content(count.ToString());
		}
	}
}
