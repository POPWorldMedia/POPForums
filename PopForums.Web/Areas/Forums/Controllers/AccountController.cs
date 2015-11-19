using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc;
using PopForums.Configuration;
using PopForums.Email;
using PopForums.Extensions;
using PopForums.ExternalLogin;
using PopForums.Feeds;
using PopForums.Models;
using PopForums.ScoringGame;
using PopForums.Services;
using PopForums.Web.Areas.Forums.Services;
using PopForums.Web.Extensions;

namespace PopForums.Web.Areas.Forums.Controllers
{
	[Area("Forums")]
	public class AccountController : Controller
	{
		public AccountController(IUserService userService, IProfileService profileService, INewAccountMailer newAccountMailer, ISettingsManager settingsManager, IPostService postService, ITopicService topicService, IForumService forumService, ILastReadService lastReadService, IClientSettingsMapper clientSettingsMapper, IUserEmailer userEmailer, IImageService imageService, IFeedService feedService, IUserAwardService userAwardService, IExternalUserAssociationManager externalUserAssociationManager, IUserRetrievalShim userRetrievalShim)
		{
			_userService = userService;
			_settingsManager = settingsManager;
			_profileService = profileService;
			_newAccountMailer = newAccountMailer;
			_postService = postService;
			_topicService = topicService;
			_forumService = forumService;
			_lastReadService = lastReadService;
			_clientSettingsMapper = clientSettingsMapper;
			_userEmailer = userEmailer;
			_imageService = imageService;
			_feedService = feedService;
			_userAwardService = userAwardService;
			_externalUserAssociationManager = externalUserAssociationManager;
			_userRetrievalShim = userRetrievalShim;
		}

		public static string Name = "Account";
		public static string CoppaDateKey = "CoppaDateKey";
		public static string TosKey = "TosKey";
		public static string ServerTimeZoneKey = "ServerTimeZoneKey";

		private readonly IUserService _userService;
		private readonly ISettingsManager _settingsManager;
		private readonly IProfileService _profileService;
		private readonly INewAccountMailer _newAccountMailer;
		private readonly IPostService _postService;
		private readonly ITopicService _topicService;
		private readonly IForumService _forumService;
		private readonly ILastReadService _lastReadService;
		private readonly IClientSettingsMapper _clientSettingsMapper;
		private readonly IUserEmailer _userEmailer;
		private readonly IImageService _imageService;
		private readonly IFeedService _feedService;
		private readonly IUserAwardService _userAwardService;
		private readonly IExternalUserAssociationManager _externalUserAssociationManager;
		private readonly IUserRetrievalShim _userRetrievalShim;

		public ViewResult Create()
		{
			SetupCreateData();
			var signupData = new SignupData
			{
				IsDaylightSaving = true,
				IsSubscribed = true,
				TimeZone = _settingsManager.Current.ServerTimeZone
			};
			return View(signupData);
		}

		private void SetupCreateData()
		{
			ViewData[CoppaDateKey] = SignupData.GetCoppaDate();
			ViewData[TosKey] = _settingsManager.Current.TermsOfService;
			ViewData[ServerTimeZoneKey] = _settingsManager.Current.ServerTimeZone;
		}

		// TODO: create
		//[HttpPost]
		//public async Task<ViewResult> Create(SignupData signupData)
		//{
		//	var ip = HttpContext.Connection.RemoteIpAddress.ToString();
		//	signupData.Validate(ModelState, _userService, ip);
		//	if (ModelState.IsValid)
		//	{
		//		var user = _userService.CreateUser(signupData, ip);
		//		_profileService.Create(user, signupData);
		//		var verifyUrl = this.FullUrlHelper("Verify", "Account");
		//		var result = _newAccountMailer.Send(user, verifyUrl);
		//		if (result != System.Net.Mail.SmtpStatusCode.Ok)
		//			ViewData["EmailProblem"] = Resources.EmailProblemAccount + result + ".";
		//		if (_settingsManager.Current.IsNewUserApproved)
		//		{
		//			ViewData["Result"] = Resources.AccountReady;
		//			_userService.Login(user, ip);
		//		}
		//		else
		//			ViewData["Result"] = Resources.AccountReadyCheckEmail;
				
		//		var authResult = await _externalAuthentication.GetAuthenticationResult(authentication);
		//		if (authResult != null)
		//			_externalUserAssociationManager.Associate(user, authResult, ip);

		//		// TODO: do login here!

		//		return View("AccountCreated");
		//	}
		//	SetupCreateData();
		//	return View(signupData);
		//}

		public ViewResult Verify(string id)
		{
			var authKey = Guid.Empty;
			if (!String.IsNullOrWhiteSpace(id) && !Guid.TryParse(id, out authKey))
				return View("VerifyFail");
			if (String.IsNullOrWhiteSpace(id))
				return View();
			var user = _userService.VerifyAuthorizationCode(authKey, HttpContext.Connection.RemoteIpAddress.ToString());
			if (user == null)
				return View("VerifyFail");
			ViewData["Result"] = Resources.AccountVerified;
			_userService.Login(user, HttpContext.Connection.RemoteIpAddress.ToString());
			return View();
		}

		[HttpPost]
		public RedirectToActionResult VerifyCode(string authorizationCode)
		{
			return RedirectToAction("Verify", new { id = authorizationCode });
		}

		public ViewResult RequestCode(string email)
		{
			var user = _userService.GetUserByEmail(email);
			if (user == null)
			{
				ViewData["Result"] = Resources.NoUserFoundWithEmail;
				return View("Verify", new { id = String.Empty });
			}
			var verifyUrl = this.FullUrlHelper("Verify", "Account");
			var result = _newAccountMailer.Send(user, verifyUrl);
			if (result != System.Net.Mail.SmtpStatusCode.Ok)
				ViewData["EmailProblem"] = Resources.EmailProblemAccount + result + ".";
			else
				ViewData["Result"] = Resources.VerificationEmailSent;
			return View("Verify", new { id = String.Empty });
		}

		public ViewResult Forgot()
		{
			return View();
		}

		[HttpPost]
		public ViewResult Forgot(FormCollection collection)
		{
			var user = _userService.GetUserByEmail(collection["Email"]);
			if (user == null)
			{
				ViewBag.Result = Resources.EmailNotFound;
			}
			else
			{
				ViewBag.Result = Resources.ForgotInstructionsSent;
				var resetLink = this.FullUrlHelper("ResetPassword", "Account");
				_userService.GeneratePasswordResetEmail(user, resetLink);
			}
			return View();
		}

		public ViewResult ResetPassword(string id)
		{
			var authKey = Guid.Empty;
			if (!String.IsNullOrWhiteSpace(id) && !Guid.TryParse(id, out authKey))
				this.Forbidden("Forbidden", null);
			var user = _userService.GetUserByAuhtorizationKey(authKey);
			var container = new PasswordResetContainer();
			if (user == null)
				container.IsValidUser = false;
			else
				container.IsValidUser = true;
			return View(container);
		}

		[HttpPost]
		public ActionResult ResetPassword(string id, PasswordResetContainer resetContainer)
		{
			var authKey = Guid.Empty;
			if (!String.IsNullOrWhiteSpace(id) && !Guid.TryParse(id, out authKey))
				this.Forbidden("Forbidden", null);
			var user = _userService.GetUserByAuhtorizationKey(authKey);
			resetContainer.IsValidUser = true;
			if (resetContainer.Password != resetContainer.PasswordRetype)
				ModelState.AddModelError("PasswordRetype", Resources.RetypePasswordMustMatch);
			string errorMessage;
			_userService.IsPasswordValid(resetContainer.Password, out errorMessage);
			if (!String.IsNullOrWhiteSpace(errorMessage))
				ModelState.AddModelError("Password", errorMessage);
			if (!ModelState.IsValid)
				return View(resetContainer);
			_userService.ResetPassword(user, resetContainer.Password, HttpContext.Connection.RemoteIpAddress.ToString());
			return RedirectToAction("ResetPasswordSuccess");
		}

		public ViewResult ResetPasswordSuccess()
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return this.Forbidden("Forbidden", null);
			return View();
		}

		public ViewResult EditProfile()
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return View("EditAccountNoUser");
			var profile = _profileService.GetProfileForEdit(user);
			var userEdit = new UserEditProfile(profile);
			return View(userEdit);
		}

		[HttpPost]
		public ViewResult EditProfile(UserEditProfile userEdit)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return View("EditAccountNoUser");
			_userService.EditUserProfile(user, userEdit);
			ViewBag.Result = Resources.ProfileUpdated;
			return View(userEdit);
		}

		public ViewResult Security()
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return View("EditAccountNoUser");
			var isNewUserApproved = _settingsManager.Current.IsNewUserApproved;
			var userEdit = new UserEditSecurity(user, isNewUserApproved);
			return View(userEdit);
		}

		[HttpPost]
		public ViewResult ChangePassword(UserEditSecurity userEdit)
		{
			string errorMessage;
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return View("EditAccountNoUser");
			if (!_userService.VerifyPassword(user, userEdit.OldPassword))
				ViewBag.PasswordResult = Resources.OldPasswordIncorrect;
			else if (!userEdit.NewPasswordsMatch())
				ViewBag.PasswordResult = Resources.RetypePasswordMustMatch;
			else if (!_userService.IsPasswordValid(userEdit.NewPassword, out errorMessage))
				ViewBag.PasswordResult = errorMessage;
			else
			{
				_userService.SetPassword(user, userEdit.NewPassword, HttpContext.Connection.RemoteIpAddress.ToString(), user);
				ViewBag.PasswordResult = Resources.NewPasswordSaved;
			}
			return View("Security", new UserEditSecurity { NewEmail = String.Empty, NewEmailRetype = String.Empty, IsNewUserApproved = _settingsManager.Current.IsNewUserApproved });
		}

		[HttpPost]
		public ViewResult ChangeEmail(UserEditSecurity userEdit)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return View("EditAccountNoUser");
			if (String.IsNullOrWhiteSpace(userEdit.NewEmail) || !userEdit.NewEmail.IsEmailAddress())
				ViewBag.EmailResult = Resources.ValidEmailAddressRequired;
			else if (userEdit.NewEmail != userEdit.NewEmailRetype)
				ViewBag.EmailResult = Resources.EmailsMustMatch;
			else if (_userService.IsEmailInUseByDifferentUser(user, userEdit.NewEmail))
				ViewBag.EmailResult = Resources.EmailInUse;
			else
			{
				_userService.ChangeEmail(user, userEdit.NewEmail, user, HttpContext.Connection.RemoteIpAddress.ToString());
				if (_settingsManager.Current.IsNewUserApproved)
					ViewBag.EmailResult = Resources.EmailChangeSuccess;
				else
				{
					ViewBag.EmailResult = Resources.VerificationEmailSent;
					var verifyUrl = this.FullUrlHelper("Verify", "Account");
					var result = _newAccountMailer.Send(user, verifyUrl);
					if (result != System.Net.Mail.SmtpStatusCode.Ok)
						ViewBag.EmailResult = Resources.EmailProblemAccount + result;
				}
			}
			return View("Security", new UserEditSecurity { NewEmail = String.Empty, NewEmailRetype = String.Empty, IsNewUserApproved = _settingsManager.Current.IsNewUserApproved });
		}

		public ViewResult ManagePhotos()
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return View("EditAccountNoUser");
			var profile = _profileService.GetProfile(user);
			var userEdit = new UserEditPhoto(profile);
			if (profile.ImageID.HasValue)
				userEdit.IsImageApproved = _imageService.IsUserImageApproved(profile.ImageID.Value);
			return View(userEdit);
		}

		// TODO: manage photos
		//[HttpPost]
		//public ActionResult ManagePhotos(UserEditPhoto userEdit)
		//{
		//	var user = this.CurrentUser();
		//	if (user == null)
		//		return View("EditAccountNoUser");
		//	var avatarFile = Request.Files["avatarFile"];
		//	var photoFile = Request.Files["photoFile"];
		//	_userService.EditUserProfileImages(user, userEdit.DeleteAvatar, userEdit.DeleteImage, avatarFile, photoFile);
		//	return RedirectToAction("ManagePhotos");
		//}

		public ViewResult MiniProfile(int id)
		{
			var user = _userService.GetUser(id);
			if (user == null)
				return View("MiniUserNotFound");
			var profile = _profileService.GetProfile(user);
			UserImage userImage = null;
			if (profile.ImageID.HasValue)
				userImage = _imageService.GetUserImage(profile.ImageID.Value);
			var model = new DisplayProfile(user, profile, userImage);
			model.PostCount = _postService.GetPostCount(user);
			return View(model);
		}

		public ViewResult ViewProfile(int id)
		{
			var user = _userService.GetUser(id);
			if (user == null)
				return this.NotFound("NotFound", null);
			var profile = _profileService.GetProfile(user);
			UserImage userImage = null;
			if (profile.ImageID.HasValue)
				userImage = _imageService.GetUserImage(profile.ImageID.Value);
			var model = new DisplayProfile(user, profile, userImage);
			model.PostCount = _postService.GetPostCount(user);
			model.Feed = _feedService.GetFeed(user);
			model.UserAwards = _userAwardService.GetAwards(user);
			return View(model);
		}

		public ViewResult Posts(int id, int page = 1)
		{
			var postUser = _userService.GetUser(id);
			if (postUser == null)
				return this.NotFound("NotFound", null);
			var includeDeleted = false;
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user != null && user.IsInRole(PermanentRoles.Moderator))
				includeDeleted = true;
			var titles = _forumService.GetAllForumTitles();
			PagerContext pagerContext;
			var topics = _topicService.GetTopics(user, postUser, includeDeleted, page, out pagerContext);
			var container = new PagedTopicContainer { ForumTitles = titles, PagerContext = pagerContext, Topics = topics };
			_lastReadService.GetTopicReadStatus(user, container);
			ViewBag.PostUserName = postUser.Name;
			return View(container);
		}

		public JsonResult ClientSettings()
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return Json(_clientSettingsMapper.GetDefault());
			var profile = _profileService.GetProfile(user);
			return Json(_clientSettingsMapper.GetClientSettings(profile));
		}

		public ViewResult Login()
		{
			string link;
			if (Request == null || string.IsNullOrWhiteSpace(Request.Headers["Referer"]))
				link = Url.Action("Index", HomeController.Name);
			else
			{
				link = Request.Headers["Referer"];
				if (!link.Contains(Request.Host.Value))
					link = Url.Action("Index", HomeController.Name);
			}
			ViewBag.Referrer = link;

			var externalLoginList = GetExternalLoginList();

			return View(externalLoginList);
		}

		private List<AuthenticationDescription> GetExternalLoginList()
		{
			var schemes = HttpContext.Authentication.GetAuthenticationSchemes()
				.Where(x => !string.IsNullOrWhiteSpace(x.DisplayName)).ToList();
			return schemes;
		}

		public ViewResult EmailUser(int id)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return this.Forbidden("Forbidden", null);
			var toUser = _userService.GetUser(id);
			if (toUser == null)
				return this.NotFound("NotFound", null);
			if (!_userEmailer.IsUserEmailable(toUser))
				return this.Forbidden("Forbidden", null);
			ViewBag.IP = HttpContext.Connection.RemoteIpAddress.ToString();
			return View(toUser);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ViewResult EmailUser(int id, string subject, string text)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return this.Forbidden("Forbidden", null);
			var toUser = _userService.GetUser(id);
			if (toUser == null)
				return this.NotFound("NotFound", null);
			if (!_userEmailer.IsUserEmailable(toUser))
				return this.Forbidden("Forbidden", null);
			if (String.IsNullOrWhiteSpace(subject) || String.IsNullOrWhiteSpace(text))
			{
				ViewBag.EmailResult = Resources.PMCreateWarnings;
				ViewBag.IP = HttpContext.Connection.RemoteIpAddress.ToString();
				return View(toUser);
			}
			_userEmailer.ComposeAndQueue(toUser, user, HttpContext.Connection.RemoteIpAddress.ToString(), subject, text);
			return View("EmailSent");
		}

		public ViewResult Unsubscribe(int id, string key)
		{
			var user = _userService.GetUser(id);
			if (user == null || !_profileService.Unsubscribe(user, key))
				return View("UnsubscribeFailure");
			return View();
		}

		public ViewResult ExternalLogins()
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return View("EditAccountNoUser");
			var externalAssociations = _externalUserAssociationManager.GetExternalUserAssociations(user);
			ViewBag.Referrer = Url.Action("ExternalLogins");
			return View(externalAssociations);
		}

		public ActionResult RemoveExternalLogin(int id)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return View("EditAccountNoUser");
			_externalUserAssociationManager.RemoveAssociation(user, id, HttpContext.Connection.RemoteIpAddress.ToString());
			return RedirectToAction("ExternalLogins");
		}
	}
}
