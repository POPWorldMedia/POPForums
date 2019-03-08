using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Mvc.Areas.Forums.Authorization;
using PopForums.Mvc.Areas.Forums.Models;
using PopForums.Mvc.Areas.Forums.Services;
using PopForums.ScoringGame;
using PopForums.Services;
using PopForums.Mvc.Areas.Forums.Extensions;

namespace PopForums.Mvc.Areas.Forums.Controllers
{
	[Authorize(Policy = PermanentRoles.Admin, AuthenticationSchemes = PopForumsAuthorizationDefaults.AuthenticationScheme)]
	[Area("Forums")]
    public class AdminController : Controller
	{
		public AdminController(IUserService userService, IProfileService profileService, ISettingsManager settingsManager, ICategoryService categoryService, IForumService forumService, ISearchService searchService, ISecurityLogService securityLogService, IErrorLog errorLog, IBanService banService, IModerationLogService modLogService, IIPHistoryService ipHistoryService, IImageService imageService, IMailingListService mailingListService, IEventDefinitionService eventDefinitionService, IAwardDefinitionService awardDefinitionService, IEventPublisher eventPublisher, IUserRetrievalShim userRetrievalShim, IServiceHeartbeatService serviceHeartbeat)
		{
			_userService = userService;
			_profileService = profileService;
			_settingsManager = settingsManager;
			_categoryService = categoryService;
			_forumService = forumService;
			_searchService = searchService;
			_securityLogService = securityLogService;
			_errorLogService = errorLog;
			_banService = banService;
			_moderationLogService = modLogService;
			_ipHistoryService = ipHistoryService;
			_imageService = imageService;
			_mailingListService = mailingListService;
			_eventDefinitionService = eventDefinitionService;
			_awardDefinitionService = awardDefinitionService;
			_eventPublisher = eventPublisher;
			_userRetrievalShim = userRetrievalShim;
			_serviceHeartbeat = serviceHeartbeat;
		}

		public static string Name = "Admin";
		public static string TimeZonesKey = "TimeZonesKey";

		private readonly IUserService _userService;
		private readonly IProfileService _profileService;
		private readonly ISettingsManager _settingsManager;
		private readonly ICategoryService _categoryService;
		private readonly IForumService _forumService;
		private readonly ISearchService _searchService;
		private readonly ISecurityLogService _securityLogService;
		private readonly IErrorLog _errorLogService;
		private readonly IBanService _banService;
		private readonly IModerationLogService _moderationLogService;
		private readonly IIPHistoryService _ipHistoryService;
		private readonly IImageService _imageService;
		private readonly IMailingListService _mailingListService;
		private readonly IEventDefinitionService _eventDefinitionService;
		private readonly IAwardDefinitionService _awardDefinitionService;
		private readonly IEventPublisher _eventPublisher;
		private readonly IUserRetrievalShim _userRetrievalShim;
		private readonly IServiceHeartbeatService _serviceHeartbeat;

		private void SaveFormValuesToSettings(IFormCollection collection)
		{
			ViewData["PostResult"] = Resources.SettingsSaved;
			var dictionary = new Dictionary<string, object>();
			foreach (var item in collection)
				dictionary.Add(item.Key, item.Value);
			_settingsManager.SaveCurrent(dictionary);
		}

		public ViewResult Index()
		{
			return View(_settingsManager.Current);
		}

		public ViewResult App()
		{
			return View();
		}

		[HttpPost]
		//[ValidateInput(false)] TODO: need this?
		public ViewResult Index(IFormCollection collection)
		{
			SaveFormValuesToSettings(Request.Form);
			ViewData[TimeZonesKey] = DataCollections.TimeZones();
			return View(_settingsManager.Current);
		}

		public ViewResult ExternalLogins()
		{
			return View(_settingsManager.Current);
		}

		[HttpPost]
		public ViewResult ExternalLogins(IFormCollection collection)
		{
			// TODO: This requires an app restart to register the new social logins
			SaveFormValuesToSettings(Request.Form);
			_settingsManager.FlushCurrent();
			ViewData["PostResult"] = Resources.SettingsSaved + ". " + Resources.AppRestartRequired + ".";
			return View(_settingsManager.Current);
		}

		public ViewResult Email()
		{
			return View(_settingsManager.Current);
		}

		[HttpPost]
		public ViewResult Email(IFormCollection collection)
		{
			SaveFormValuesToSettings(Request.Form);
			return View(_settingsManager.Current);
		}

		public ViewResult Categories()
		{
			return View(_categoryService.GetAll());
		}

		[HttpPost]
		public RedirectToActionResult AddCategory(string newCategoryTitle)
		{
			_categoryService.Create(newCategoryTitle);
			return RedirectToAction("Categories");
		}

		public ViewResult CategoryList()
		{
			return View(_categoryService.GetAll());
		}

		[HttpPost]
		public RedirectToActionResult DeleteCategory(int categoryID)
		{
			var category = _categoryService.Get(categoryID);
			if (category == null)
				throw new Exception(String.Format("Category with ID {0} does not exist.", categoryID));
			_categoryService.Delete(category);
			return RedirectToAction("Categories");
		}

		[HttpPost]
		public JsonResult MoveCategoryUp(int categoryID)
		{
			var category = _categoryService.Get(categoryID);
			if (category == null)
				return Json(new BasicJsonMessage { Result = false, Message = "That category doesn't exist" });
			_categoryService.MoveCategoryUp(category);
			return Json(new BasicJsonMessage { Result = true });
		}

		[HttpPost]
		public JsonResult MoveCategoryDown(int categoryID)
		{
			var category = _categoryService.Get(categoryID);
			if (category == null)
				return Json(new BasicJsonMessage { Result = false, Message = "That category doesn't exist" });
			_categoryService.MoveCategoryDown(category);
			return Json(new BasicJsonMessage { Result = true });
		}

		public ViewResult EditCategory(int id)
		{
			var category = _categoryService.Get(id);
			if (category == null)
				throw new Exception(String.Format("Category with ID {0} does not exist.", id));
			return View(category);
		}

		[HttpPost]
		public RedirectToActionResult EditCategory(int categoryID, string newTitle)
		{
			var category = _categoryService.Get(categoryID);
			if (category == null)
				throw new Exception(String.Format("Category with ID {0} does not exist.", categoryID));
			_categoryService.UpdateTitle(category, newTitle);
			return RedirectToAction("Categories");
		}

		public ViewResult Forums()
		{
			return View(_forumService.GetCategorizedForumContainer());
		}

		public ViewResult AddForum()
		{
			SetupCategoryDropDown();
			return View();
		}

		private void SetupCategoryDropDown(int categoryID = 0)
		{
			var categories = _categoryService.GetAll();
			categories.Insert(0, new Category { Title = "Uncategorized" });
			var selectList = new SelectList(categories, "CategoryID", "Title", categoryID);
			ViewData["categoryID"] = selectList;
		}

		[HttpPost]
		public RedirectToActionResult AddForum(int? categoryID, string title, string description, bool isVisible, bool isArchived, string forumAdapterName, bool isQAForum)
		{
			_forumService.Create(categoryID, title, description, isVisible, isArchived, -2, forumAdapterName, isQAForum);
			return RedirectToAction("Forums");
		}

		public ViewResult EditForum(int id)
		{
			var forum = _forumService.Get(id);
			SetupCategoryDropDown(forum.CategoryID.HasValue ? forum.CategoryID.Value : 0);
			return View(forum);
		}

		[HttpPost]
		public RedirectToActionResult EditForum(int id, int? categoryID, string title, string description, bool isVisible, bool isArchived, string forumAdapterName, bool isQAForum)
		{
			var forum = _forumService.Get(id);
			_forumService.Update(forum, categoryID, title, description, isVisible, isArchived, forumAdapterName, isQAForum);
			return RedirectToAction("Forums");
		}

		public ViewResult CategorizedForums()
		{
			return View(_forumService.GetCategorizedForumContainer());
		}

		[HttpPost]
		public JsonResult MoveForumUp(int forumID)
		{
			var forum = _forumService.Get(forumID);
			if (forum == null)
				return Json(new BasicJsonMessage { Result = false, Message = "That forum doesn't exist" });
			_forumService.MoveForumUp(forum);
			return Json(new BasicJsonMessage { Result = true });
		}

		[HttpPost]
		public JsonResult MoveForumDown(int forumID)
		{
			var forum = _forumService.Get(forumID);
			if (forum == null)
				return Json(new BasicJsonMessage { Result = false, Message = "That forum doesn't exist" });
			_forumService.MoveForumDown(forum);
			return Json(new BasicJsonMessage { Result = true });
		}

		public ViewResult ForumPermissions()
		{
			return View(_forumService.GetCategorizedForumContainer());
		}

		public JsonResult ForumRoles(int id)
		{
			var forum = _forumService.Get(id);
			if (forum == null)
				throw new Exception(String.Format("ForumID {0} not found.", id));
			var container = new ForumPermissionContainer
			{
				ForumID = forum.ForumID,
				AllRoles = _userService.GetAllRoles(),
				PostRoles = _forumService.GetForumPostRoles(forum),
				ViewRoles = _forumService.GetForumViewRoles(forum)
			};
			return Json(container);
		}

		public enum ModifyForumRolesType
		{
			AddView, RemoveView, AddPost, RemovePost, RemoveAllView, RemoveAllPost
		}

		public EmptyResult ModifyForumRoles(int forumID, ModifyForumRolesType modifyType, string role = null)
		{
			var forum = _forumService.Get(forumID);
			if (forum == null)
				throw new Exception(String.Format("ForumID {0} not found.", forumID));
			switch (modifyType)
			{
				case ModifyForumRolesType.AddPost:
					_forumService.AddPostRole(forum, role);
					break;
				case ModifyForumRolesType.RemovePost:
					_forumService.RemovePostRole(forum, role);
					break;
				case ModifyForumRolesType.AddView:
					_forumService.AddViewRole(forum, role);
					break;
				case ModifyForumRolesType.RemoveView:
					_forumService.RemoveViewRole(forum, role);
					break;
				case ModifyForumRolesType.RemoveAllPost:
					_forumService.RemoveAllPostRoles(forum);
					break;
				case ModifyForumRolesType.RemoveAllView:
					_forumService.RemoveAllViewRoles(forum);
					break;
				default:
					throw new Exception("ModifyForumRoles doesn't know what to do.");
			}
			return new EmptyResult();
		}

		public ViewResult UserRoles()
		{
			var roles = _userService.GetAllRoles();
			roles.Remove("Admin");
			roles.Remove("Moderator");
			return View(roles);
		}

		[HttpPost]
		public RedirectToActionResult CreateRole(string newRole)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			_userService.CreateRole(newRole, user, HttpContext.Connection.RemoteIpAddress.ToString());
			return RedirectToAction("UserRoles");
		}

		[HttpPost]
		public RedirectToActionResult DeleteRole(string roleToDelete)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			_userService.DeleteRole(roleToDelete, user, HttpContext.Connection.RemoteIpAddress.ToString());
			return RedirectToAction("UserRoles");
		}

		public ViewResult Search()
		{
			ViewData["Interval"] = _settingsManager.Current.SearchIndexingInterval;
			ViewData["JunkWords"] = _searchService.GetJunkWords();
			return View();
		}

		[HttpPost]
		public ViewResult Search(IFormCollection collection)
		{
			SaveFormValuesToSettings(Request.Form);
			ViewData["Interval"] = _settingsManager.Current.SearchIndexingInterval;
			ViewData["JunkWords"] = _searchService.GetJunkWords();
			return View();
		}

		public RedirectToActionResult CreateJunkWord(string newWord)
		{
			_searchService.CreateJunkWord(newWord);
			return RedirectToAction("Search");
		}

		public RedirectToActionResult DeleteJunkWord(string deleteWord)
		{
			_searchService.DeleteJunkWord(deleteWord);
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
			switch (userSearch.SearchType)
			{
				case UserSearch.UserSearchType.Email:
					return View(_userService.SearchByEmail(userSearch.SearchText));
				case UserSearch.UserSearchType.Name:
					return View(_userService.SearchByName(userSearch.SearchText));
				case UserSearch.UserSearchType.Role:
					return View(_userService.SearchByRole(userSearch.SearchText));
				default:
					throw new ArgumentOutOfRangeException("userSearch");
			}
		}

		public ActionResult EditUser(int id)
		{
			var user = _userService.GetUser(id);
			if (user == null)
				throw new Exception(String.Format("UserID {0} not found.", id));
			var profile = _profileService.GetProfileForEdit(user);
			var model = new UserEditWithFiles(user, profile);
			return View(model);
		}

		[HttpPost]
		public ActionResult EditUser(int id, UserEditWithFiles userEdit)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			var targetUser = _userService.GetUser(id);
			if (targetUser == null)
				throw new Exception(String.Format("UserID {0} not found.", id));
			var avatarFile = userEdit.AvatarFile?.OpenReadStream().ToBytes();
			var photoFile = userEdit.PhotoFile?.OpenReadStream().ToBytes();
			_userService.EditUser(targetUser, userEdit, userEdit.DeleteAvatar, userEdit.DeleteImage, avatarFile, photoFile, HttpContext.Connection.RemoteIpAddress.ToString(), user);
			return RedirectToAction("EditUserSearch");
		}

		[HttpPost]
		public RedirectToActionResult DeleteUser(int id)
		{
			return DeleteUser(id, false);
		}

		[HttpPost]
		public RedirectToActionResult DeleteAndBanUser(int id)
		{
			return DeleteUser(id, true);
		}

		private RedirectToActionResult DeleteUser(int id, bool ban)
		{
			var targetUser = _userService.GetUser(id);
			if (targetUser == null)
				throw new Exception(String.Format("UserID {0} not found.", id));
			var currentUser = _userRetrievalShim.GetUser(HttpContext);
			_userService.DeleteUser(targetUser, currentUser, HttpContext.Connection.RemoteIpAddress.ToString(), ban);
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
					list = _securityLogService.GetLogEntriesByUserID(Convert.ToInt32(searchTerm), startDate, endDate);
					break;
				case "name":
					list = _securityLogService.GetLogEntriesByUserName(searchTerm, startDate, endDate);
					break;
				default:
					throw new ArgumentOutOfRangeException("searchTerm");
			}
			return View(list);
		}

		public ViewResult ErrorLog(int page = 1)
		{
			PagerContext pagerContext;
			var errors = _errorLogService.GetErrors(page, 20, out pagerContext);
			ViewBag.PagerContext = pagerContext;
			return View(errors);
		}

		[HttpPost]
		public RedirectToActionResult DeleteAllErrorLog()
		{
			_errorLogService.DeleteAllErrors();
			return RedirectToAction("ErrorLog");
		}

		[HttpPost]
		public RedirectToActionResult DeleteErrorLog(int id)
		{
			_errorLogService.DeleteError(id);
			return RedirectToAction("ErrorLog");
		}

		public ViewResult Ban()
		{
			ViewBag.EmailList = _banService.GetEmailBans();
			ViewBag.IPList = _banService.GetIPBans();
			return View();
		}

		[HttpPost]
		public RedirectToActionResult BanIP(string ip)
		{
			_banService.BanIP(ip);
			return RedirectToAction("Ban");
		}

		[HttpPost]
		public RedirectToActionResult RemoveIPBan(string ip)
		{
			_banService.RemoveIPBan(ip);
			return RedirectToAction("Ban");
		}

		[HttpPost]
		public RedirectToActionResult BanEmail(string email)
		{
			_banService.BanEmail(email);
			return RedirectToAction("Ban");
		}

		[HttpPost]
		public RedirectToActionResult RemoveEmailBan(string email)
		{
			_banService.RemoveEmailBan(email);
			return RedirectToAction("Ban");
		}

		public ViewResult Services()
		{
			var services = _serviceHeartbeat.GetAll();
			return View(services);
		}

		[HttpPost]
		public ActionResult ServicesClearAll()
		{
			_serviceHeartbeat.ClearAll();
			return RedirectToAction("Services");
		}

		public ViewResult ModerationLog()
		{
			return View();
		}

		[HttpPost]
		public ViewResult ModerationLog(DateTime start, DateTime end)
		{
			var list = _moderationLogService.GetLog(start, end);
			return View(list);
		}

		public ViewResult IPHistory()
		{
			return View();
		}

		[HttpPost]
		public ViewResult IPHistory(string ip, DateTime start, DateTime end)
		{
			var list = _ipHistoryService.GetHistory(ip, start, end);
			return View(list);
		}

		public ViewResult UserImageApprove()
		{
			ViewBag.IsNewUserImageApproved = _settingsManager.Current.IsNewUserImageApproved;
			var dictionary = new Dictionary<UserImage, User>();
			var unapprovedImages = _imageService.GetUnapprovedUserImages();
			var users = _userService.GetUsersFromIDs(unapprovedImages.Select(i => i.UserID).ToList());
			foreach (var image in unapprovedImages)
			{
				dictionary.Add(image, users.Single(u => u.UserID == image.UserID));
			}
			return View(dictionary);
		}

		[HttpPost]
		public RedirectToActionResult ApproveUserImage(int id)
		{
			_imageService.ApproveUserImage(id);
			return RedirectToAction("UserImageApprove");
		}

		[HttpPost]
		public RedirectToActionResult DeleteUserImage(int id)
		{
			_imageService.DeleteUserImage(id);
			return RedirectToAction("UserImageApprove");
		}

		public ViewResult EmailUsers()
		{
			return View();
		}

		//[ValidateInput(false)] TODO: Need this?
		[HttpPost]
		public ViewResult EmailUsers(string subject, string body, string htmlBody)
		{
			if (String.IsNullOrWhiteSpace(subject) || String.IsNullOrWhiteSpace(body))
			{
				ViewBag.Result = Resources.SubjectAndBodyNotEmpty;
				return View();
			}
			var baseString = this.FullUrlHelper("Unsubscribe", AccountController.Name, new { id = "--id--", key = "--key--" });
			baseString = baseString.Replace("--id--", "{0}").Replace("--key--", "{1}");
			Func<User, string> unsubscribeLinkGenerator =
				user => String.Format(baseString, user.UserID, _profileService.GetUnsubscribeHash(user));
			_mailingListService.MailUsers(subject, body, htmlBody, unsubscribeLinkGenerator);
			return View("EmailUsersSuccessful");
		}

		public ViewResult EventDefinitions()
		{
			var model = _eventDefinitionService.GetAll();
			return View(model);
		}

		[HttpPost]
		public ActionResult AddEvent(EventDefinition eventDefinition)
		{
			_eventDefinitionService.Create(eventDefinition);
			return RedirectToAction("EventDefinitions");
		}

		[HttpPost]
		public ActionResult DeleteEvent(string id)
		{
			_eventDefinitionService.Delete(id);
			return RedirectToAction("EventDefinitions");
		}

		public ViewResult AwardDefinitions()
		{
			var model = _awardDefinitionService.GetAll();
			return View(model);
		}

		[HttpPost]
		public ActionResult AddAward(AwardDefinition awardDefinition)
		{
			_awardDefinitionService.Create(awardDefinition);
			return RedirectToAction("Award", new { id = awardDefinition.AwardDefinitionID });
		}

		[HttpPost]
		public ActionResult DeleteAward(string id)
		{
			_awardDefinitionService.Delete(id);
			return RedirectToAction("AwardDefinitions");
		}

		public ActionResult Award(string id)
		{
			var award = _awardDefinitionService.Get(id);
			if (award == null)
				return NotFound();
			var selectList = new SelectList(_eventDefinitionService.GetAll(), "EventDefinitionID", "EventDefinitionID");
			ViewBag.EventList = selectList;
			ViewBag.Conditions = _awardDefinitionService.GetConditions(id);
			return View(award);
		}

		[HttpPost]
		public ActionResult DeleteAwardCondition(string awardDefinitionID, string eventDefinitionID)
		{
			_awardDefinitionService.DeleteCondition(awardDefinitionID, eventDefinitionID);
			return RedirectToAction("Award", new { id = awardDefinitionID });
		}

		[HttpPost]
		public ActionResult AddAwardCondition(AwardCondition awardCondition)
		{
			_awardDefinitionService.AddCondition(awardCondition);
			return RedirectToAction("Award", new { id = awardCondition.AwardDefinitionID });
		}

		public ViewResult ManualEvent()
		{
			var selectList = new SelectList(_eventDefinitionService.GetAll(), "EventDefinitionID", "EventDefinitionID");
			ViewBag.EventList = selectList;
			return View();
		}

		[HttpPost]
		public ActionResult ManualEvent(int userID, string feedMessage, int points)
		{
			var user = _userService.GetUser(userID);
			if (user != null)
				_eventPublisher.ProcessManualEvent(feedMessage, user, points);
			return RedirectToAction("ManualEvent");
		}

		//[ValidateInput(false)] TODO: Need this?
		[HttpPost]
		public ActionResult ManualExistingEvent(int userID, string feedMessage, string eventDefinitionID)
		{
			var user = _userService.GetUser(userID);
			var eventDefinition = _eventDefinitionService.GetEventDefinition(eventDefinitionID);
			if (user != null && eventDefinition != null)
				_eventPublisher.ProcessEvent(feedMessage, user, eventDefinition.EventDefinitionID, false);
			return RedirectToAction("ManualEvent");
		}

		public JsonResult GetNames(string id)
		{
			var users = _userService.SearchByName(id);
			var projection = users.Select(u => new { u.UserID, value = u.Name });
			return Json(projection);
		}

		public ViewResult ScoringGame()
		{
			ViewData["Interval"] = _settingsManager.Current.ScoringGameCalculatorInterval;
			return View();
		}

		[HttpPost]
		public ViewResult ScoringGame(IFormCollection collection)
		{
			SaveFormValuesToSettings(Request.Form);
			ViewData["Interval"] = _settingsManager.Current.ScoringGameCalculatorInterval;
			return View();
		}
	}
}
