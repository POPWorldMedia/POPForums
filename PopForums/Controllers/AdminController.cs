using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Ninject;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.ScoringGame;
using PopForums.Services;
using PopForums.Web;

namespace PopForums.Controllers
{
	[Admin]
	public class AdminController : Controller
	{
		public AdminController()
		{
			var container = PopForumsActivation.Kernel;
			UserService = container.Get<IUserService>();
			ProfileService = container.Get<IProfileService>();
			SettingsManager = container.Get<ISettingsManager>();
			CategoryService = container.Get<ICategoryService>();
			ForumService = container.Get<IForumService>();
			SearchService = container.Get<ISearchService>();
			SecurityLogService = container.Get<ISecurityLogService>();
			ErrorLogService = container.Get<IErrorLog>();
			BanService = container.Get<IBanService>();
			ModerationLogService = container.Get<IModerationLogService>();
			IPHistoryService = container.Get<IIPHistoryService>();
			ImageService = container.Get<IImageService>();
			MailingListService = container.Get<IMailingListService>();
			EventDefinitionService = container.Get<IEventDefinitionService>();
			AwardDefinitionService = container.Get<IAwardDefinitionService>();
			EventPublisher = container.Get<IEventPublisher>();
		}

		protected internal AdminController(IUserService userService, IProfileService profileService, ISettingsManager settingsManager, ICategoryService categoryService, IForumService forumService, ISearchService searchService, ISecurityLogService securityLogService, IErrorLog errorLog, IBanService banService, IModerationLogService modLogService, IIPHistoryService ipHistoryService, IImageService imageService, IMailingListService mailingListService, IEventDefinitionService eventDefinitionService, IAwardDefinitionService awardDefinitionService, IEventPublisher eventPublisher)
		{
			UserService = userService;
			ProfileService = profileService;
			SettingsManager = settingsManager;
			CategoryService = categoryService;
			ForumService = forumService;
			SearchService = searchService;
			SecurityLogService = securityLogService;
			ErrorLogService = errorLog;
			BanService = banService;
			ModerationLogService = modLogService;
			IPHistoryService = ipHistoryService;
			ImageService = imageService;
			MailingListService = mailingListService;
			EventDefinitionService = eventDefinitionService;
			AwardDefinitionService = awardDefinitionService;
			EventPublisher = eventPublisher;
		}

		public static string Name = "Admin";
		public static string TimeZonesKey = "TimeZonesKey";

		public IUserService UserService { get; private set; }
		public IProfileService ProfileService { get; private set; }
		public ISettingsManager SettingsManager { get; private set; }
		public ICategoryService CategoryService { get; private set; }
		public IForumService ForumService { get; private set; }
		public ISearchService SearchService { get; private set; }
		public ISecurityLogService SecurityLogService { get; private set; }
		public IErrorLog ErrorLogService { get; private set; }
		public IBanService BanService { get; private set; }
		public IModerationLogService ModerationLogService { get; private set; }
		public IIPHistoryService IPHistoryService { get; private set; }
		public IImageService ImageService { get; private set; }
		public IMailingListService MailingListService { get; private set; }
		public IEventDefinitionService EventDefinitionService { get; private set; }
		public IAwardDefinitionService AwardDefinitionService { get; private set; }
		public IEventPublisher EventPublisher { get; private set; }

		private void SaveFormValuesToSettings(FormCollection collection)
		{
			ViewData["PostResult"] = Resources.SettingsSaved;
			var dictionary = new Dictionary<string, object>();
			collection.CopyTo(dictionary);
			SettingsManager.SaveCurrent(dictionary);
		}

		public ViewResult Index()
		{
			return View(SettingsManager.Current);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ViewResult Index(FormCollection collection)
		{
			SaveFormValuesToSettings(collection);
			ViewData[TimeZonesKey] = DataCollections.TimeZones();
			return View(SettingsManager.Current);
		}

		public ViewResult ExternalLogins()
		{
			return View(SettingsManager.Current);
		}

		[HttpPost]
		public ViewResult ExternalLogins(FormCollection collection)
		{
			SaveFormValuesToSettings(collection);
			return View(SettingsManager.Current);
		}

		public ViewResult Email()
		{
			return View(SettingsManager.Current);
		}

		[HttpPost]
		public ViewResult Email(FormCollection collection)
		{
			SaveFormValuesToSettings(collection);
			return View(SettingsManager.Current);
		}

		public ViewResult Categories()
		{
			return View(CategoryService.GetAll());
		}

		[HttpPost]
		public RedirectToRouteResult AddCategory(string newCategoryTitle)
		{
			CategoryService.Create(newCategoryTitle);
			return RedirectToAction("Categories");
		}

		public ViewResult CategoryList()
		{
			return View(CategoryService.GetAll());
		}

		[HttpPost]
		public RedirectToRouteResult DeleteCategory(int categoryID)
		{
			var category = CategoryService.Get(categoryID);
			if (category == null)
				throw new Exception(String.Format("Category with ID {0} does not exist.", categoryID));
			CategoryService.Delete(category);
			return RedirectToAction("Categories");
		}

		[HttpPost]
		public JsonResult MoveCategoryUp(int categoryID)
		{
			var category = CategoryService.Get(categoryID);
			if (category == null)
				return Json(new BasicJsonMessage { Result = false, Message = "That category doesn't exist" });
			CategoryService.MoveCategoryUp(category);
			return Json(new BasicJsonMessage { Result = true });
		}

		[HttpPost]
		public JsonResult MoveCategoryDown(int categoryID)
		{
			var category = CategoryService.Get(categoryID);
			if (category == null)
				return Json(new BasicJsonMessage { Result = false, Message = "That category doesn't exist" });
			CategoryService.MoveCategoryDown(category);
			return Json(new BasicJsonMessage { Result = true });
		}

		public ViewResult EditCategory(int id)
		{
			var category = CategoryService.Get(id);
			if (category == null)
				throw new Exception(String.Format("Category with ID {0} does not exist.", id));
			return View(category);
		}

		[HttpPost]
		public RedirectToRouteResult EditCategory(int categoryID, string newTitle)
		{
			var category = CategoryService.Get(categoryID);
			if (category == null)
				throw new Exception(String.Format("Category with ID {0} does not exist.", categoryID));
			CategoryService.UpdateTitle(category, newTitle);
			return RedirectToAction("Categories");
		}

		public ViewResult Forums()
		{
			return View(ForumService.GetCategorizedForumContainer());
		}

		public ViewResult AddForum()
		{
			SetupCategoryDropDown();
			return View();
		}

		private void SetupCategoryDropDown(int categoryID = 0)
		{
			var categories = CategoryService.GetAll();
			categories.Insert(0, new Category(0) {Title = "Uncategorized"});
			var selectList = new SelectList(categories, "CategoryID", "Title", categoryID);
			ViewData["categoryID"] = selectList;
		}

		[HttpPost]
		public RedirectToRouteResult AddForum(int? categoryID, string title, string description, bool isVisible, bool isArchived, string forumAdapterName)
		{
			ForumService.Create(categoryID, title, description, isVisible, isArchived, -2, forumAdapterName);
			return RedirectToAction("Forums");
		}

		public ViewResult EditForum(int id)
		{
			var forum = ForumService.Get(id);
			SetupCategoryDropDown(forum.CategoryID.HasValue ? forum.CategoryID.Value : 0);
			return View(forum);
		}

		[HttpPost]
		public RedirectToRouteResult EditForum(int id, int? categoryID, string title, string description, bool isVisible, bool isArchived, string forumAdapterName)
		{
			var forum = ForumService.Get(id);
			ForumService.Update(forum, categoryID, title, description, isVisible, isArchived, forumAdapterName);
			return RedirectToAction("Forums");
		}

		public ViewResult CategorizedForums()
		{
			return View(ForumService.GetCategorizedForumContainer());
		}

		[HttpPost]
		public JsonResult MoveForumUp(int forumID)
		{
			var forum = ForumService.Get(forumID);
			if (forum == null)
				return Json(new BasicJsonMessage { Result = false, Message = "That forum doesn't exist" });
			ForumService.MoveForumUp(forum);
			return Json(new BasicJsonMessage { Result = true });
		}

		[HttpPost]
		public JsonResult MoveForumDown(int forumID)
		{
			var forum = ForumService.Get(forumID);
			if (forum == null)
				return Json(new BasicJsonMessage { Result = false, Message = "That forum doesn't exist" });
			ForumService.MoveForumDown(forum);
			return Json(new BasicJsonMessage { Result = true });
		}

		public ViewResult ForumPermissions()
		{
			return View(ForumService.GetCategorizedForumContainer());
		}

		public JsonResult ForumRoles(int id)
		{
			var forum = ForumService.Get(id);
			if (forum == null)
				throw new Exception(String.Format("ForumID {0} not found.", id));
			var container = new ForumPermissionContainer
			    {
					ForumID = forum.ForumID,
			        AllRoles = UserService.GetAllRoles(),
					PostRoles = ForumService.GetForumPostRoles(forum),
					ViewRoles = ForumService.GetForumViewRoles(forum)
			    };
			return Json(container, JsonRequestBehavior.AllowGet);
		}

		public enum ModifyForumRolesType
		{
			AddView, RemoveView, AddPost, RemovePost, RemoveAllView, RemoveAllPost
		}

		public EmptyResult ModifyForumRoles(int forumID, ModifyForumRolesType modifyType, string role = null)
		{
			var forum = ForumService.Get(forumID);
			if (forum == null)
				throw new Exception(String.Format("ForumID {0} not found.", forumID));
			switch (modifyType)
			{
				case ModifyForumRolesType.AddPost:
					ForumService.AddPostRole(forum, role);
					break;
				case ModifyForumRolesType.RemovePost:
					ForumService.RemovePostRole(forum, role);
					break;
				case ModifyForumRolesType.AddView:
					ForumService.AddViewRole(forum, role);
					break;
				case ModifyForumRolesType.RemoveView:
					ForumService.RemoveViewRole(forum, role);
					break;
				case ModifyForumRolesType.RemoveAllPost:
					ForumService.RemoveAllPostRoles(forum);
					break;
				case ModifyForumRolesType.RemoveAllView:
					ForumService.RemoveAllViewRoles(forum);
					break;
				default:
					throw new Exception("ModifyForumRoles doesn't know what to do.");
			}
			return new EmptyResult();
		}

		public ViewResult UserRoles()
		{
			var roles = UserService.GetAllRoles();
			roles.Remove("Admin");
			roles.Remove("Moderator");
			return View(roles);
		}

		[HttpPost]
		public RedirectToRouteResult CreateRole(string newRole)
		{
			var user = this.CurrentUser();
			UserService.CreateRole(newRole, user, HttpContext.Request.UserHostAddress);
			return RedirectToAction("UserRoles");
		}

		[HttpPost]
		public RedirectToRouteResult DeleteRole(string roleToDelete)
		{
			var user = this.CurrentUser();
			UserService.DeleteRole(roleToDelete, user, HttpContext.Request.UserHostAddress);
			return RedirectToAction("UserRoles");
		}

		public ViewResult Search()
		{
			ViewData["Interval"] = SettingsManager.Current.SearchIndexingInterval;
			ViewData["JunkWords"] = SearchService.GetJunkWords();
			return View();
		}

		[HttpPost]
		public ViewResult Search(FormCollection collection)
		{
			SaveFormValuesToSettings(collection);
			ViewData["Interval"] = SettingsManager.Current.SearchIndexingInterval;
			ViewData["JunkWords"] = SearchService.GetJunkWords();
			return View();
		}

		public RedirectToRouteResult CreateJunkWord(string newWord)
		{
			SearchService.CreateJunkWord(newWord);
			return RedirectToAction("Search");
		}

		public RedirectToRouteResult DeleteJunkWord(string deleteWord)
		{
			SearchService.DeleteJunkWord(deleteWord);
			return RedirectToAction("Search");
		}

		public ViewResult EditUserSearch()
		{
			return View();
		}

		[HttpPost]
		public ViewResult EditUserSearch(UserSearch userSearch)
		{
			ViewBag.SearchText = userSearch.SearchText;
			ViewBag.UserSearchType = userSearch.SearchType;
			switch(userSearch.SearchType)
			{
				case UserSearch.UserSearchType.Email:
					return View(UserService.SearchByEmail(userSearch.SearchText));
				case UserSearch.UserSearchType.Name:
					return View(UserService.SearchByName(userSearch.SearchText));
				case UserSearch.UserSearchType.Role:
					return View(UserService.SearchByRole(userSearch.SearchText));
				default:
					throw new ArgumentOutOfRangeException("userSearch");
			}
		}

		public ActionResult EditUser(int id)
		{
			var user = UserService.GetUser(id);
			if (user == null)
				throw new Exception(String.Format("UserID {0} not found.", id));
			var profile = ProfileService.GetProfileForEdit(user);
			var model = new UserEdit(user, profile);
			return View(model);
		}

		[HttpPost]
		public ActionResult EditUser(int id, UserEdit userEdit)
		{
			var user = this.CurrentUser();
			var targetUser = UserService.GetUser(id);
			if (targetUser == null)
				throw new Exception(String.Format("UserID {0} not found.", id));
			var avatarFile = Request.Files["avatarFile"];
			var photoFile = Request.Files["photoFile"];
			UserService.EditUser(targetUser, userEdit, userEdit.DeleteAvatar, userEdit.DeleteImage, avatarFile, photoFile, HttpContext.Request.UserHostAddress, user);
			return RedirectToAction("EditUserSearch");
		}

		[HttpPost]
		public RedirectToRouteResult DeleteUser(int id)
		{
			return DeleteUser(id, false);
		}

		[HttpPost]
		public RedirectToRouteResult DeleteAndBanUser(int id)
		{
			return DeleteUser(id, true);
		}

		private RedirectToRouteResult DeleteUser(int id, bool ban)
		{
			var targetUser = UserService.GetUser(id);
			if (targetUser == null)
				throw new Exception(String.Format("UserID {0} not found.", id));
			UserService.DeleteUser(targetUser, this.CurrentUser(), HttpContext.Request.UserHostAddress, ban);
			return RedirectToAction("EditUserSearch");
		}

		public ViewResult SecurityLog()
		{
			return View();
		}

		[HttpPost]
		public ViewResult SecurityLog(DateTime startDate, DateTime endDate, string searchType, string searchTerm)
		{
			List<SecurityLogEntry> list;
			switch (searchType.ToLower())
			{
				case "userid":
					list = SecurityLogService.GetLogEntriesByUserID(Convert.ToInt32(searchTerm), startDate, endDate);
					break;
				case "name":
					list = SecurityLogService.GetLogEntriesByUserName(searchTerm, startDate, endDate);
					break;
				default:
					throw new ArgumentOutOfRangeException("searchTerm");
			}
			return View(list);
		}

		public ViewResult ErrorLog(int page = 1)
		{
			PagerContext pagerContext;
			var errors = ErrorLogService.GetErrors(page, 20, out pagerContext);
			ViewBag.PagerContext = pagerContext;
			return View(errors);
		}

		[HttpPost]
		public RedirectToRouteResult DeleteAllErrorLog()
		{
			ErrorLogService.DeleteAllErrors();
			return RedirectToAction("ErrorLog");
		}

		[HttpPost]
		public RedirectToRouteResult DeleteErrorLog(int id)
		{
			ErrorLogService.DeleteError(id);
			return RedirectToAction("ErrorLog");
		}

		public ViewResult Ban()
		{
			ViewBag.EmailList = BanService.GetEmailBans();
			ViewBag.IPList = BanService.GetIPBans();
			return View();
		}

		[HttpPost]
		public RedirectToRouteResult BanIP(string ip)
		{
			BanService.BanIP(ip);
			return RedirectToAction("Ban");
		}

		[HttpPost]
		public RedirectToRouteResult RemoveIPBan(string ip)
		{
			BanService.RemoveIPBan(ip);
			return RedirectToAction("Ban");
		}

		[HttpPost]
		public RedirectToRouteResult BanEmail(string email)
		{
			BanService.BanEmail(email);
			return RedirectToAction("Ban");
		}

		[HttpPost]
		public RedirectToRouteResult RemoveEmailBan(string email)
		{
			BanService.RemoveEmailBan(email);
			return RedirectToAction("Ban");
		}

		public ViewResult Services()
		{
			var services = PopForumsActivation.ApplicationServices;
			return View(services);
		}

		public ViewResult ModerationLog()
		{
			return View();
		}

		[HttpPost]
		public ViewResult ModerationLog(DateTime start, DateTime end)
		{
			var list = ModerationLogService.GetLog(start, end);
			return View(list);
		}

		public ViewResult IPHistory()
		{
			return View();
		}

		[HttpPost]
		public ViewResult IPHistory(string ip, DateTime start, DateTime end)
		{
			var list = IPHistoryService.GetHistory(ip, start, end);
			return View(list);
		}

		public ViewResult UserImageApprove()
		{
			ViewBag.IsNewUserImageApproved = SettingsManager.Current.IsNewUserImageApproved;
			var dictionary = new Dictionary<UserImage, User>();
			var unapprovedImages = ImageService.GetUnapprovedUserImages();
			var users = UserService.GetUsersFromIDs(unapprovedImages.Select(i => i.UserID).ToList());
			foreach (var image in unapprovedImages)
			{
				dictionary.Add(image, users.Single(u => u.UserID == image.UserID));
			}
			return View(dictionary);
		}

		[HttpPost]
		public RedirectToRouteResult ApproveUserImage(int id)
		{
			ImageService.ApproveUserImage(id);
			return RedirectToAction("UserImageApprove");
		}

		[HttpPost]
		public RedirectToRouteResult DeleteUserImage(int id)
		{
			ImageService.DeleteUserImage(id);
			return RedirectToAction("UserImageApprove");
		}

		public ViewResult EmailUsers()
		{
			return View();
		}

		[ValidateInput(false)]
		[HttpPost]
		public ViewResult EmailUsers(string subject, string body, string htmlBody)
		{
			if (String.IsNullOrWhiteSpace(subject) || String.IsNullOrWhiteSpace(body))
			{
				ViewBag.Result = Resources.SubjectAndBodyNotEmpty;
				return View();
			}
			Func<User, string> unsubscribeLinkGenerator =
				 user => this.FullUrlHelper("Unsubscribe", AccountController.Name, new { id = user.UserID, key = ProfileService.GetUnsubscribeHash(user) });
			MailingListService.MailUsers(subject, body, htmlBody, unsubscribeLinkGenerator);
			return View("EmailUsersSuccessful");
		}

		public ViewResult EventDefinitions()
		{
			var model = EventDefinitionService.GetAll();
			return View(model);
		}

		[HttpPost]
		public ActionResult AddEvent(EventDefinition eventDefinition)
		{
			EventDefinitionService.Create(eventDefinition);
			return RedirectToAction("EventDefinitions");
		}

		[HttpPost]
		public ActionResult DeleteEvent(string id)
		{
			EventDefinitionService.Delete(id);
			return RedirectToAction("EventDefinitions");
		}

		public ViewResult AwardDefinitions()
		{
			var model = AwardDefinitionService.GetAll();
			return View(model);
		}

		[HttpPost]
		public ActionResult AddAward(AwardDefinition awardDefinition)
		{
			AwardDefinitionService.Create(awardDefinition);
			return RedirectToAction("Award", new { id = awardDefinition.AwardDefinitionID });
		}

		[HttpPost]
		public ActionResult DeleteAward(string id)
		{
			AwardDefinitionService.Delete(id);
			return RedirectToAction("AwardDefinitions");
		}

		public ViewResult Award(string id)
		{
			var award = AwardDefinitionService.Get(id);
			if (award == null)
				return this.NotFound("NotFound", null);
			var selectList = new SelectList(EventDefinitionService.GetAll(), "EventDefinitionID", "EventDefinitionID");
			ViewBag.EventList = selectList;
			ViewBag.Conditions = AwardDefinitionService.GetConditions(id);
			return View(award);
		}

		[HttpPost]
		public ActionResult DeleteAwardCondition(string awardDefinitionID, string eventDefinitionID)
		{
			AwardDefinitionService.DeleteCondition(awardDefinitionID, eventDefinitionID);
			return RedirectToAction("Award", new { id = awardDefinitionID});
		}

		[HttpPost]
		public ActionResult AddAwardCondition(AwardCondition awardCondition)
		{
			AwardDefinitionService.AddCondition(awardCondition);
			return RedirectToAction("Award", new { id = awardCondition.AwardDefinitionID });
		}

		public ViewResult ManualEvent()
		{
			var selectList = new SelectList(EventDefinitionService.GetAll(), "EventDefinitionID", "EventDefinitionID");
			ViewBag.EventList = selectList;
			return View();
		}

		[HttpPost]
		public ActionResult ManualEvent(int userID, string feedMessage, int points)
		{
			var user = UserService.GetUser(userID);
			if (user != null)
				EventPublisher.ProcessManualEvent(feedMessage, user, points);
			return RedirectToAction("ManualEvent");
		}

		[ValidateInput(false)]
		[HttpPost]
		public ActionResult ManualExistingEvent(int userID, string feedMessage, string eventDefinitionID)
		{
			var user = UserService.GetUser(userID);
			var eventDefinition = EventDefinitionService.GetEventDefinition(eventDefinitionID);
			if (user != null && eventDefinition != null)
				EventPublisher.ProcessEvent(feedMessage, user, eventDefinition.EventDefinitionID, false);
			return RedirectToAction("ManualEvent");
		}

		public JsonResult GetNames(string id)
		{
			var users = UserService.SearchByName(id);
			var projection = users.Select(u => new { u.UserID, value = u.Name });
			return Json(projection, JsonRequestBehavior.AllowGet);
		}

		public ViewResult ScoringGame()
		{
			ViewData["Interval"] = SettingsManager.Current.ScoringGameCalculatorInterval;
			return View();
		}

		[HttpPost]
		public ViewResult ScoringGame(FormCollection collection)
		{
			SaveFormValuesToSettings(collection);
			ViewData["Interval"] = SettingsManager.Current.ScoringGameCalculatorInterval;
			return View();
		}
	}
}
