using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Ninject;
using PopForums.Configuration;
using PopForums.Email;
using PopForums.Extensions;
using PopForums.ExternalLogin;
using PopForums.Feeds;
using PopForums.Models;
using PopForums.ScoringGame;
using PopForums.Services;
using PopForums.Web;
using FormCollection = System.Web.Mvc.FormCollection;

namespace PopForums.Controllers
{
	public class AccountController : Controller
	{
		public AccountController()
		{
			var container = PopForumsActivation.Kernel;
			UserService = container.Get<IUserService>();
			SettingsManager = container.Get<ISettingsManager>();
			ProfileService = container.Get<IProfileService>();
			NewAccountMailer = container.Get<INewAccountMailer>();
			PostService = container.Get<IPostService>();
			TopicService = container.Get<ITopicService>();
			ForumService = container.Get<IForumService>();
			LastReadService = container.Get<ILastReadService>();
			ClientSettingsMapper = container.Get<IClientSettingsMapper>();
			UserEmailer = container.Get<IUserEmailer>();
			ImageService = container.Get<IImageService>();
			FeedService = container.Get<IFeedService>();
			UserAwardService = container.Get<IUserAwardService>();
			OwinContext = container.Get<IOwinContext>();
			ExternalAuthentication = container.Get<IExternalAuthentication>();
			UserAssociationManager = container.Get<IUserAssociationManager>();
		}

		protected internal AccountController(IUserService userService, IProfileService profileService, INewAccountMailer newAccountMailer, ISettingsManager settingsManager, IPostService postService, ITopicService topicService, IForumService forumService, ILastReadService lastReadService, IClientSettingsMapper clientSettingsMapper, IUserEmailer userEmailer, IImageService imageService, IFeedService feedService, IUserAwardService userAwardService, IOwinContext owinContext, IExternalAuthentication externalAuthentication, IUserAssociationManager userAssociationManager)
		{
			UserService = userService;
			SettingsManager = settingsManager;
			ProfileService = profileService;
			NewAccountMailer = newAccountMailer;
			PostService = postService;
			TopicService = topicService;
			ForumService = forumService;
			LastReadService = lastReadService;
			ClientSettingsMapper = clientSettingsMapper;
			UserEmailer = userEmailer;
			ImageService = imageService;
			FeedService = feedService;
			UserAwardService = userAwardService;
			OwinContext = owinContext;
			ExternalAuthentication = externalAuthentication;
			UserAssociationManager = userAssociationManager;
		}

		public static string Name = "Account";
		public static string CoppaDateKey = "CoppaDateKey";
		public static string TosKey = "TosKey";
		public static string ServerTimeZoneKey = "ServerTimeZoneKey";

		public IUserService UserService { get; private set; }
		public ISettingsManager SettingsManager { get; private set; }
		public IProfileService ProfileService { get; private set; }
		public INewAccountMailer NewAccountMailer { get; private set; }
		public IPostService PostService { get; private set; }
		public ITopicService TopicService { get; private set; }
		public IForumService ForumService { get; private set; }
		public ILastReadService LastReadService { get; private set; }
		public IClientSettingsMapper ClientSettingsMapper { get; private set; }
		public IUserEmailer UserEmailer { get; private set; }
		public IImageService ImageService { get; private set; }
		public IFeedService FeedService { get; private set; }
		public IUserAwardService UserAwardService { get; private set; }
		public IOwinContext OwinContext { get; private set; }
		public IExternalAuthentication ExternalAuthentication { get; private set; }
		public IUserAssociationManager UserAssociationManager { get; private set; }

		public ViewResult Create()
		{
			SetupCreateData();
			var signupData = new SignupData
			                 	{
			                 		IsDaylightSaving = true,
			                 		IsSubscribed = true,
									TimeZone = SettingsManager.Current.ServerTimeZone
			                 	};
			return View(signupData);
		}

		private void SetupCreateData()
		{
			ViewData[CoppaDateKey] = SignupData.GetCoppaDate();
			ViewData[TosKey] = SettingsManager.Current.TermsOfService;
			ViewData[ServerTimeZoneKey] = SettingsManager.Current.ServerTimeZone;
		}

		[HttpPost]
		public async Task<ViewResult> Create(SignupData signupData)
		{
			signupData.Validate(ModelState, UserService, HttpContext.Request.UserHostAddress);
			if (ModelState.IsValid)
			{
				var user = UserService.CreateUser(signupData, HttpContext.Request.UserHostAddress);
				ProfileService.Create(user, signupData);
				var verifyUrl = this.FullUrlHelper("Verify", "Account");
				var result = NewAccountMailer.Send(user, verifyUrl);
				if (result != System.Net.Mail.SmtpStatusCode.Ok)
					ViewData["EmailProblem"] = Resources.EmailProblemAccount + result + ".";
				if (SettingsManager.Current.IsNewUserApproved)
				{
					ViewData["Result"] = Resources.AccountReady;
					UserService.Login(user.Email, signupData.Password, false, HttpContext);
				}
				else
					ViewData["Result"] = Resources.AccountReadyCheckEmail;

				var authentication = OwinContext.Authentication;
				var authResult = await ExternalAuthentication.GetAuthenticationResult(authentication);
				if (authResult != null)
					UserAssociationManager.Associate(user, authResult);

				return View("AccountCreated");
			}
			SetupCreateData();
			return View(signupData);
		}

		public ViewResult Verify(string id)
		{
			var authKey = Guid.Empty;
			if (!String.IsNullOrWhiteSpace(id) && !Guid.TryParse(id, out authKey))
				return View("VerifyFail");
			if (String.IsNullOrWhiteSpace(id))
				return View();
			var user = UserService.VerifyAuthorizationCode(authKey, HttpContext.Request.UserHostAddress);
			if (user == null)
				return View("VerifyFail");
			ViewData["Result"] = Resources.AccountVerified;
			UserService.Login(user, HttpContext);
			return View();
		}

		[HttpPost]
		public RedirectToRouteResult VerifyCode(string authorizationCode)
		{
			return RedirectToAction("Verify", new {id = authorizationCode});
		}

		public ViewResult RequestCode(string email)
		{
			var user = UserService.GetUserByEmail(email);
			if (user == null)
			{
				ViewData["Result"] = Resources.NoUserFoundWithEmail;
				return View("Verify", new { id = String.Empty });
			}
			var verifyUrl = this.FullUrlHelper("Verify", "Account");
			var result = NewAccountMailer.Send(user, verifyUrl);
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
			var user = UserService.GetUserByEmail(collection["Email"]);
			if (user == null)
			{
				ViewBag.Result = Resources.EmailNotFound;
			}
			else
			{
				ViewBag.Result = Resources.ForgotInstructionsSent;
				var resetLink = this.FullUrlHelper("ResetPassword", "Account");
				UserService.GeneratePasswordResetEmail(user, resetLink);
			}
			return View();
		}

		public ViewResult ResetPassword(string id)
		{
			var authKey = Guid.Empty;
			if (!String.IsNullOrWhiteSpace(id) && !Guid.TryParse(id, out authKey))
				this.Forbidden("Forbidden", null);
			var user = UserService.GetUserByAuhtorizationKey(authKey);
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
			var user = UserService.GetUserByAuhtorizationKey(authKey);
			resetContainer.IsValidUser = true;
			if (resetContainer.Password != resetContainer.PasswordRetype)
				ModelState.AddModelError("PasswordRetype", Resources.RetypePasswordMustMatch);
			UserService.IsPasswordValid(resetContainer.Password, ModelState);
			if (!ModelState.IsValid)
				return View(resetContainer);
			UserService.ResetPassword(user, resetContainer.Password, HttpContext);
			return RedirectToAction("ResetPasswordSuccess");
		}

		public ViewResult ResetPasswordSuccess()
		{
			var user = this.CurrentUser();
			if (user == null)
				return this.Forbidden("Forbidden", null);
			return View();
		}

		public ViewResult EditProfile()
		{
			var user = this.CurrentUser();
			if (user == null)
				return View("EditAccountNoUser");
			var profile = ProfileService.GetProfileForEdit(user);
			var userEdit = new UserEditProfile(profile);
			return View(userEdit);
		}

		[HttpPost]
		public ViewResult EditProfile(UserEditProfile userEdit)
		{
			var user = this.CurrentUser();
			if (user == null)
				return View("EditAccountNoUser");
			UserService.EditUserProfile(user, userEdit);
			ViewBag.Result = Resources.ProfileUpdated;
			return View(userEdit);
		}

		public ViewResult Security()
		{
			var user = this.CurrentUser();
			if (user == null)
				return View("EditAccountNoUser");
			var userEdit = new UserEditSecurity(user);
			return View(userEdit);
		}

		[HttpPost]
		public ViewResult ChangePassword(UserEditSecurity userEdit)
		{
			var modelState = new ModelStateDictionary();
			var user = this.CurrentUser();
			if (user == null)
				return View("EditAccountNoUser");
			if (!UserService.VerifyPassword(user, userEdit.OldPassword))
				ViewBag.PasswordResult = Resources.OldPasswordIncorrect;
			else if (!userEdit.NewPasswordsMatch())
				ViewBag.PasswordResult = Resources.RetypePasswordMustMatch;
			else if (!UserService.IsPasswordValid(userEdit.NewPassword, modelState))
				ViewBag.PasswordResult = modelState["Password"];
			else
			{
				UserService.SetPassword(user, userEdit.NewPassword, HttpContext.Request.UserHostAddress, user);
				ViewBag.PasswordResult = Resources.NewPasswordSaved;
			}
			return View("Security");
		}

		[HttpPost]
		public ViewResult ChangeEmail(UserEditSecurity userEdit)
		{
			var user = this.CurrentUser();
			if (user == null)
				return View("EditAccountNoUser");
			if (!userEdit.NewEmail.IsEmailAddress())
				ViewBag.EmailResult = Resources.ValidEmailAddressRequired;
			else if (userEdit.NewEmail != userEdit.NewEmailRetype)
				ViewBag.EmailResult = Resources.EmailsMustMatch;
			else if (UserService.IsEmailInUseByDifferentUser(user, userEdit.NewEmail))
				ViewBag.EmailResult = Resources.EmailInUse;
			else
			{
				UserService.ChangeEmail(user, userEdit.NewEmail, user, HttpContext.Request.UserHostAddress);
				if (SettingsManager.Current.IsNewUserApproved)
					ViewBag.EmailResult = Resources.EmailChangeSuccess;
				else
				{
					ViewBag.EmailResult = Resources.VerificationEmailSent;
					var verifyUrl = this.FullUrlHelper("Verify", "Account");
					var result = NewAccountMailer.Send(user, verifyUrl);
					if (result != System.Net.Mail.SmtpStatusCode.Ok)
						ViewBag.EmailResult = Resources.EmailProblemAccount + result;
				}
			}
			return View("Security", new UserEditSecurity { NewEmail = String.Empty, NewEmailRetype = String.Empty });
		}

		public ViewResult ManagePhotos()
		{
			var user = this.CurrentUser();
			if (user == null)
				return View("EditAccountNoUser");
			var profile = ProfileService.GetProfile(user);
			var userEdit = new UserEditPhoto(profile);
			if (profile.ImageID.HasValue)
				userEdit.IsImageApproved = ImageService.IsUserImageApproved(profile.ImageID.Value);
			return View(userEdit);
		}

		[HttpPost]
		public ActionResult ManagePhotos(UserEditPhoto userEdit)
		{
			var user = this.CurrentUser();
			if (user == null)
				return View("EditAccountNoUser");
			var avatarFile = Request.Files["avatarFile"];
			var photoFile = Request.Files["photoFile"];
			UserService.EditUserProfileImages(user, userEdit.DeleteAvatar, userEdit.DeleteImage, avatarFile, photoFile);
			return RedirectToAction("ManagePhotos");
		}

		public ViewResult MiniProfile(int id)
		{
			var user = UserService.GetUser(id);
			if (user == null)
				return View("MiniUserNotFound");
			var profile = ProfileService.GetProfile(user);
			UserImage userImage = null;
			if (profile.ImageID.HasValue)
				userImage = ImageService.GetUserImage(profile.ImageID.Value);
			var model = new DisplayProfile(user, profile, userImage);
			model.PostCount = PostService.GetPostCount(user);
			return View(model);
		}

		public ViewResult ViewProfile(int id)
		{
			var user = UserService.GetUser(id);
			if (user == null)
				return this.NotFound("NotFound", null);
			var profile = ProfileService.GetProfile(user);
			UserImage userImage = null;
			if (profile.ImageID.HasValue)
				userImage = ImageService.GetUserImage(profile.ImageID.Value);
			var model = new DisplayProfile(user, profile, userImage);
			model.PostCount = PostService.GetPostCount(user);
			model.Feed = FeedService.GetFeed(user);
			model.UserAwards = UserAwardService.GetAwards(user);
			return View(model);
		}

		public ViewResult Posts(int id, int page = 1)
		{
			var postUser = UserService.GetUser(id);
			if (postUser == null)
				return this.NotFound("NotFound", null);
			var includeDeleted = false;
			var user = this.CurrentUser();
			if (user != null && user.IsInRole(PermanentRoles.Moderator))
				includeDeleted = true;
			var titles = ForumService.GetAllForumTitles();
			PagerContext pagerContext;
			var topics = TopicService.GetTopics(user, postUser, includeDeleted, page, out pagerContext);
			var container = new PagedTopicContainer { ForumTitles = titles, PagerContext = pagerContext, Topics = topics };
			LastReadService.GetTopicReadStatus(user, container);
			ViewBag.PostUserName = postUser.Name;
			return View(container);
		}

		public JsonResult ClientSettings()
		{
			var user = this.CurrentUser();
			if (user == null)
				return Json(ClientSettingsMapper.GetDefault(), JsonRequestBehavior.AllowGet);
			var profile = ProfileService.GetProfile(user);
			return Json(ClientSettingsMapper.GetClientSettings(profile), JsonRequestBehavior.AllowGet);
		}

		public ViewResult Login()
		{
			string link;
			if (Request == null || Request.UrlReferrer == null || Request.Url == null)
				link = Url.Action("Index", ForumHomeController.Name);
			else
			{
				link = Request.UrlReferrer.ToString();
				if (!link.Contains(Request.Url.Host))
					link = Url.Action("Index", ForumHomeController.Name);
			}
			ViewBag.Referrer = link;

			var externalLoginList = new List<AuthenticationDescription>(HttpContext.GetOwinContext().Authentication.GetAuthenticationTypes((Func<AuthenticationDescription, bool>) (d =>
				{
				  if (d.Properties != null)
					return d.Properties.ContainsKey("Caption");
					return false;
				})));

			return View(externalLoginList);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ExternalLogin(string provider, string returnUrl)
		{
			return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { loginProvider = provider, ReturnUrl = returnUrl }));
		}

		public async Task<ActionResult> ExternalLoginCallback(string loginProvider, string returnUrl)
		{
			var authentication = OwinContext.Authentication;
			var authResult = await ExternalAuthentication.GetAuthenticationResult(authentication);
			var matchResult = UserAssociationManager.ExternalUserAssociationCheck(authResult);
			if (matchResult.Successful)
			{
				UserService.Login(matchResult.User, HttpContext);
				return Redirect(returnUrl);
			}

			// TODO: offer standard login to associate, or go to create

			return RedirectToAction("Create");
		}

		public ViewResult EmailUser(int id)
		{
			var user = this.CurrentUser();
			if (user == null)
				return this.Forbidden("Forbidden", null);
			var toUser = UserService.GetUser(id);
			if (toUser == null)
				return this.NotFound("NotFound", null);
			if (!UserEmailer.IsUserEmailable(toUser))
				return this.Forbidden("Forbidden", null);
			ViewBag.IP = Request.UserHostAddress;
			return View(toUser);
		}

		[HttpPost]
		public ViewResult EmailUser(int id, string subject, string text)
		{
			var user = this.CurrentUser();
			if (user == null)
				return this.Forbidden("Forbidden", null);
			var toUser = UserService.GetUser(id);
			if (toUser == null)
				return this.NotFound("NotFound", null);
			if (!UserEmailer.IsUserEmailable(toUser))
				return this.Forbidden("Forbidden", null);
			UserEmailer.ComposeAndQueue(toUser, user, Request.UserHostAddress, subject, text);
			return View("EmailSent");
		}

		public ViewResult Unsubscribe(int id, string key)
		{
			var user = UserService.GetUser(id);
			if (user == null || !ProfileService.Unsubscribe(user, key))
				return View("UnsubscribeFailure");
			return View();
		}
	}
}
