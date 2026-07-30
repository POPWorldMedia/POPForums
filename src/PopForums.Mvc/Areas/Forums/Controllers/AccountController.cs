using PopForums.Models.Subscriptions;
using PopForums.Mvc.Areas.Forums.Authentication;
using PopIdentity;

namespace PopForums.Mvc.Areas.Forums.Controllers;

[Area("Forums")]
public class AccountController(
	IUserService userService,
	IProfileService profileService,
	INewAccountMailer newAccountMailer,
	ISettingsManager settingsManager,
	IPostService postService,
	ITopicService topicService,
	IForumService forumService,
	ILastReadService lastReadService,
	IImageService imageService,
	IFeedService feedService,
	IUserAwardService userAwardService,
	IExternalUserAssociationManager externalUserAssociationManager,
	IUserRetrievalShim userRetrievalShim,
	IExternalLoginRoutingService externalLoginRoutingService,
	IExternalLoginTempService externalLoginTempService,
	IConfig config,
	IReCaptchaService reCaptchaService,
	IOAuthOnlyService oAuthOnlyService,
	ISkuService skuService,
	IBuyService buyService,
	ISubscriptionHistoryService subscriptionHistoryService)
	: Controller
{
	public static string Name = "Account";
	public static string CoppaDateKey = "CoppaDateKey";
	public static string TosKey = "TosKey";

	[PopForumsAuthenticationIgnore]
	[TypeFilter(typeof(OAuthOnlyForbidAttribute))]
	public IActionResult Create()
	{
		SetupCreateData();
		var signupData = new SignupData
		{
			IsSubscribed = true,
			IsAutoFollowOnReply = true
		};
		var loginState = externalLoginTempService.Read();
		if (loginState?.ResultData != null)
		{
			signupData.Email = loginState.ResultData.Email;
			signupData.Name = loginState.ResultData.Name;
		}
		return View(signupData);
	}

	private void SetupCreateData()
	{
		ViewData[CoppaDateKey] = SignupData.GetCoppaDate();
		ViewData[TosKey] = settingsManager.Current.TermsOfService;
	}

	[PopForumsAuthenticationIgnore]
	[TypeFilter(typeof(OAuthOnlyForbidAttribute))]
	[HttpPost]
	public async Task<IActionResult> Create(SignupData signupData)
	{
		var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
		if (config.UseReCaptcha)
		{
			var reCaptchaResponse = await reCaptchaService.VerifyToken(signupData.Token, ip);
			if (!reCaptchaResponse.IsSuccess)
				ModelState.AddModelError("Email", Resources.BotError);
		}
		await ValidateSignupData(signupData, ModelState, ip);
		if (ModelState.IsValid)
		{
			var user = await userService.CreateUserWithProfile(signupData, ip);
			var verifyUrl = Url.Action("Verify", "Account", null, Request.Scheme);
			var result = newAccountMailer.Send(user, verifyUrl);
			if (result != SmtpStatusCode.Ok)
				ViewData["EmailProblem"] = Resources.EmailProblemAccount + (result?.ToString() ?? "App exception") + ".";
			if (settingsManager.Current.IsNewUserApproved)
			{
				ViewData["Result"] = Resources.AccountReady;
				await userService.Login(user, ip);
			}
			else
				ViewData["Result"] = Resources.AccountReadyCheckEmail;

			var loginState = externalLoginTempService.Read();
			if (loginState != null)
			{
				var externalLoginInfo = new ExternalLoginInfo(loginState.ProviderType.ToString(), loginState.ResultData.ID, loginState.ResultData.Name);
				await externalUserAssociationManager.Associate(user, externalLoginInfo, ip);
			}

			await IdentityController.PerformSignInAsync(user, HttpContext);

			return View("AccountCreated");
		}
		SetupCreateData();
		return View(signupData);
	}

	private async Task ValidateSignupData(SignupData signupData, ModelStateDictionary modelState, string ip)
	{
		if (!signupData.IsCoppa)
			modelState.AddModelError("IsCoppa", Resources.MustBe13);
		if (!signupData.IsTos)
			modelState.AddModelError("IsTos", Resources.MustAcceptTOS);
		var passwordValid = userService.IsPasswordValid(signupData.Password, out var passwordError);
		if (!passwordValid)
			modelState.AddModelError("Password", passwordError);
		if (signupData.Password != signupData.PasswordRetype)
			modelState.AddModelError("PasswordRetype", Resources.RetypeYourPassword);
		if (string.IsNullOrWhiteSpace(signupData.Name))
			modelState.AddModelError("Name", Resources.NameRequired);
		else if (await userService.IsNameInUse(signupData.Name))
			modelState.AddModelError("Name", Resources.NameInUse);
		if (string.IsNullOrWhiteSpace(signupData.Email))
			modelState.AddModelError("Email", Resources.EmailRequired);
		else
		if (!signupData.Email.IsEmailAddress())
			modelState.AddModelError("Email", Resources.ValidEmailAddressRequired);
		else if (signupData.Email != null && await userService.IsEmailInUse(signupData.Email))
			modelState.AddModelError("Email", Resources.EmailInUse);
		if (signupData.Email != null && await userService.IsEmailBanned(signupData.Email))
			modelState.AddModelError("Email", Resources.EmailBanned);
		if (await userService.IsIPBanned(ip))
			modelState.AddModelError("Email", Resources.IPBanned);
	}

	[PopForumsAuthenticationIgnore]
	[TypeFilter(typeof(OAuthOnlyForbidAttribute))]
	public async Task<ViewResult> Verify(string id)
	{
		var authKey = Guid.Empty;
		if (!string.IsNullOrWhiteSpace(id) && !Guid.TryParse(id, out authKey))
			return View("VerifyFail");
		if (string.IsNullOrWhiteSpace(id))
			return View();
		var user = await userService.VerifyAuthorizationCode(authKey, HttpContext.Connection.RemoteIpAddress?.ToString());
		if (user == null)
			return View("VerifyFail");
		ViewData["Result"] = Resources.AccountVerified;
		await userService.Login(user, HttpContext.Connection.RemoteIpAddress?.ToString());
		return View();
	}

	[PopForumsAuthenticationIgnore]
	[TypeFilter(typeof(OAuthOnlyForbidAttribute))]
	[HttpPost]
	public RedirectToActionResult VerifyCode(string authorizationCode)
	{
		return RedirectToAction("Verify", new { id = authorizationCode });
	}

	[PopForumsAuthenticationIgnore]
	[TypeFilter(typeof(OAuthOnlyForbidAttribute))]
	public async Task<ViewResult> RequestCode(string email)
	{
		var user = await userService.GetUserByEmail(email);
		if (user == null)
		{
			ViewData["Result"] = Resources.NoUserFoundWithEmail;
			return View("Verify", new { id = String.Empty });
		}
		var verifyUrl = Url.Action("Verify", "Account", null, Request.Scheme);
		var result = newAccountMailer.Send(user, verifyUrl);
		if (result != SmtpStatusCode.Ok)
			ViewData["EmailProblem"] = Resources.EmailProblemAccount + result + ".";
		else
			ViewData["Result"] = Resources.VerificationEmailSent;
		return View("Verify", new { id = String.Empty });
	}

	[PopForumsAuthenticationIgnore]
	public ViewResult Forgot()
	{
		return View();
	}

	[PopForumsAuthenticationIgnore]
	[TypeFilter(typeof(OAuthOnlyForbidAttribute))]
	[HttpPost]
	public async Task<ViewResult> Forgot(string email)
	{
		var user = await userService.GetUserByEmail(email);
		if (user == null)
		{
			ViewBag.Result = Resources.EmailNotFound;
		}
		else
		{
			ViewBag.Result = Resources.ForgotInstructionsSent;
			var resetLink = Url.Action("ResetPassword", "Account", null, Request.Scheme);
			await userService.GeneratePasswordResetEmail(user, resetLink);
		}
		return View();
	}

	[PopForumsAuthenticationIgnore]
	[TypeFilter(typeof(OAuthOnlyForbidAttribute))]
	public async Task<ActionResult> ResetPassword(string id)
	{
		var authKey = Guid.Empty;
		if (!string.IsNullOrWhiteSpace(id) && !Guid.TryParse(id, out authKey))
			return StatusCode(403);
		var user = await userService.GetUserByAuhtorizationKey(authKey);
		var container = new PasswordResetContainer();
		container.IsValidUser = user != null;
		return View(container);
	}

	[PopForumsAuthenticationIgnore]
	[TypeFilter(typeof(OAuthOnlyForbidAttribute))]
	[HttpPost]
	public async Task<ActionResult> ResetPassword(string id, PasswordResetContainer resetContainer)
	{
		var authKey = Guid.Empty;
		if (!string.IsNullOrWhiteSpace(id) && !Guid.TryParse(id, out authKey))
			return StatusCode(403);
		var user = await userService.GetUserByAuhtorizationKey(authKey);
		resetContainer.IsValidUser = true;
		if (resetContainer.Password != resetContainer.PasswordRetype)
			ModelState.AddModelError("PasswordRetype", Resources.RetypePasswordMustMatch);
		string errorMessage;
		userService.IsPasswordValid(resetContainer.Password, out errorMessage);
		if (!string.IsNullOrWhiteSpace(errorMessage))
			ModelState.AddModelError("Password", errorMessage);
		if (!ModelState.IsValid)
			return View(resetContainer);
		await userService.ResetPassword(user, resetContainer.Password, HttpContext.Connection.RemoteIpAddress?.ToString());
		return RedirectToAction("ResetPasswordSuccess");
	}

	[PopForumsAuthenticationIgnore]
	[TypeFilter(typeof(OAuthOnlyForbidAttribute))]
	public ActionResult ResetPasswordSuccess()
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return RedirectToAction("Login");
		return View();
	}

	public async Task<ViewResult> Subscriptions()
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return View("EditAccountNoUser");
		var profile = await profileService.GetProfile(user);
		var model = new SubscriptionsViewModel
		{
			IsSubscriber = user.IsSubscriber(),
			Expiration = user.SubscriptionExpiration,
			IsAutoRenewal = profile.IsAutoRenewal,
			Last4 = profile.Last4
		};
		if (!string.IsNullOrEmpty(profile.SkuID))
		{
			var sku = await skuService.Get(profile.SkuID);
			if (sku != null)
			{
				model.SkuName = sku.Name;
				model.Months = sku.Months;
			}
		}
		return View(model);
	}

	public async Task<ViewResult> BuySubscription()
	{
		var skus = await skuService.GetAllActive();
		return View(skus);
	}

	[HttpPost]
	public async Task<ActionResult> BuySubscription(BuyModel buyModel)
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return View("EditAccountNoUser");
		var result = await buyService.BuyNew(buyModel, user.UserID);
		if (!result.IsSuccessful)
		{
			ViewBag.ErrorMessage = result.Message;
			var skus = await skuService.GetAllActive();
			return View(skus);
		}
		return RedirectToAction("Subscriptions");
	}

	[HttpPost]
	public async Task<RedirectToActionResult> ToggleAutoRenewal()
	{
		var user = userRetrievalShim.GetUser();
		if (user != null)
		{
			var profile = await profileService.GetProfile(user);
			profile.IsAutoRenewal = !profile.IsAutoRenewal;
			await profileService.Update(profile);
		}
		return RedirectToAction("Subscriptions");
	}

	public async Task<ViewResult> SubscriptionHistory()
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return View("EditAccountNoUser");
		var history = await subscriptionHistoryService.GetByUserID(user.UserID);
		return View(history);
	}

	public ViewResult SubscriptionCardUpdate()
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return View("EditAccountNoUser");
		return View();
	}

	[HttpPost]
	public async Task<ActionResult> SubscriptionCardUpdate(string token)
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return View("EditAccountNoUser");
		var result = await buyService.UpdatePaymentMethod(user.UserID, token);
		if (!result.IsSuccessful)
		{
			ViewBag.ErrorMessage = result.Message;
			return View();
		}
		return RedirectToAction("Subscriptions");
	}

	public async Task<ViewResult> EditProfile()
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return View("EditAccountNoUser");
		var profile = await profileService.GetProfileForEdit(user);
		var userEdit = new UserEditProfile(profile);
		return View(userEdit);
	}

	[HttpPost]
	public async Task<ViewResult> EditProfile(UserEditProfile userEdit)
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return View("EditAccountNoUser");
		await profileService.EditUserProfile(user, userEdit);
		ViewBag.Result = Resources.ProfileUpdated;
		var profile = await profileService.GetProfileForEdit(user);
		var newEdit = new UserEditProfile(profile);
		return View(newEdit);
	}

	[TypeFilter(typeof(OAuthOnlyForbidAttribute))]
	public ViewResult Security()
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return View("EditAccountNoUser");
		var isNewUserApproved = settingsManager.Current.IsNewUserApproved;
		var userEdit = new UserEditSecurity(user, isNewUserApproved);
		return View(userEdit);
	}

	[TypeFilter(typeof(OAuthOnlyForbidAttribute))]
	[HttpPost]
	public async Task<ViewResult> ChangePassword(UserEditSecurity userEdit)
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return View("EditAccountNoUser");
		var (isPasswordPassed, _) = await userService.CheckPassword(user.Email, userEdit.OldPassword);
		if (!isPasswordPassed)
			ViewBag.PasswordResult = Resources.OldPasswordIncorrect;
		else if (!userEdit.NewPasswordsMatch())
			ViewBag.PasswordResult = Resources.RetypePasswordMustMatch;
		else if (!userService.IsPasswordValid(userEdit.NewPassword, out var errorMessage))
			ViewBag.PasswordResult = errorMessage;
		else
		{
			await userService.SetPassword(user, userEdit.NewPassword, HttpContext.Connection.RemoteIpAddress?.ToString(), user);
			ViewBag.PasswordResult = Resources.NewPasswordSaved;
		}
		return View("Security", new UserEditSecurity { NewEmail = String.Empty, NewEmailRetype = String.Empty, IsNewUserApproved = settingsManager.Current.IsNewUserApproved });
	}

	[TypeFilter(typeof(OAuthOnlyForbidAttribute))]
	[HttpPost]
	public async Task<ViewResult> ChangeEmail(UserEditSecurity userEdit)
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return View("EditAccountNoUser");
		if (string.IsNullOrWhiteSpace(userEdit.NewEmail) || !userEdit.NewEmail.IsEmailAddress())
			ViewBag.EmailResult = Resources.ValidEmailAddressRequired;
		else if (userEdit.NewEmail != userEdit.NewEmailRetype)
			ViewBag.EmailResult = Resources.EmailsMustMatch;
		else if (await userService.IsEmailInUseByDifferentUser(user, userEdit.NewEmail))
			ViewBag.EmailResult = Resources.EmailInUse;
		else
		{
			await userService.ChangeEmail(user, userEdit.NewEmail, user, HttpContext.Connection.RemoteIpAddress?.ToString());
			if (settingsManager.Current.IsNewUserApproved)
				ViewBag.EmailResult = Resources.EmailChangeSuccess;
			else
			{
				ViewBag.EmailResult = Resources.VerificationEmailSent;
				var verifyUrl = Url.Action("Verify", "Account", null, Request.Scheme);
				var result = newAccountMailer.Send(user, verifyUrl);
				if (result != SmtpStatusCode.Ok)
					ViewBag.EmailResult = Resources.EmailProblemAccount + result;
			}
		}
		return View("Security", new UserEditSecurity { NewEmail = String.Empty, NewEmailRetype = String.Empty, IsNewUserApproved = settingsManager.Current.IsNewUserApproved });
	}

	public async Task<ViewResult> ManagePhotos()
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return View("EditAccountNoUser");
		var profile = await profileService.GetProfile(user);
		var userEdit = new UserEditPhoto(profile);
		if (profile.ImageID.HasValue)
			userEdit.IsImageApproved = await imageService.IsUserImageApproved(profile.ImageID.Value);
		return View(userEdit);
	}
		
	[HttpPost]
	public async Task<ActionResult> ManagePhotos(UserEditPhoto userEdit)
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return View("EditAccountNoUser");
		byte[] avatarFile = null;
		if (userEdit.AvatarFile != null)
			avatarFile = userEdit.AvatarFile.OpenReadStream().ToBytes();
		byte[] photoFile = null;
		if (userEdit.PhotoFile != null)
			photoFile = userEdit.PhotoFile.OpenReadStream().ToBytes();
		await userService.EditUserProfileImages(user, userEdit.DeleteAvatar, userEdit.DeleteImage, avatarFile, photoFile);
		return RedirectToAction("ManagePhotos");
	}

	public async Task<ViewResult> MiniProfile(int id)
	{
		var user = await userService.GetUser(id);
		if (user == null)
			return View("MiniUserNotFound");
		var profile = await profileService.GetProfile(user);
		UserImage userImage = null;
		if (profile.ImageID.HasValue)
			userImage = await imageService.GetUserImage(profile.ImageID.Value);
		var model = new DisplayProfile(user, profile, userImage);
		model.PostCount = await postService.GetPostCount(user);
		var viewingUser = userRetrievalShim.GetUser();
		if (viewingUser == null)
			model.ShowDetails = false;
		return View(model);
	}

	public async Task<ActionResult> ViewProfile(int id)
	{
		var user = await userService.GetUser(id);
		if (user == null)
			return NotFound();
		var profile = await profileService.GetProfile(user);
		UserImage userImage = null;
		if (profile.ImageID.HasValue)
			userImage = await imageService.GetUserImage(profile.ImageID.Value);
		var model = new DisplayProfile(user, profile, userImage);
		model.PostCount = await postService.GetPostCount(user);
		model.Feed = await feedService.GetFeed(user);
		model.UserAwards = await userAwardService.GetAwards(user);
		var viewingUser = userRetrievalShim.GetUser();
		if (viewingUser == null)
			model.ShowDetails = false;
		return View(model);
	}

	public async Task<ActionResult> Posts(int id, int pageNumber = 1)
	{
		var postUser = await userService.GetUser(id);
		if (postUser == null)
			return NotFound();
		var includeDeleted = false;
		var user = userRetrievalShim.GetUser();
		if (user != null && user.IsInRole(PermanentRoles.Moderator))
			includeDeleted = true;
		var titles = forumService.GetAllForumTitles();
		var (topics, pagerContext) = await topicService.GetTopics(user, postUser, includeDeleted, pageNumber);
		var container = new PagedTopicContainer { ForumTitles = titles, PagerContext = pagerContext, Topics = topics };
		await lastReadService.GetTopicReadStatus(user, container);
		ViewBag.PostUserName = postUser.Name;
		return View(container);
	}

	[PopForumsAuthenticationIgnore]
	public ActionResult Login()
	{
		if (config.IsOAuthOnly)
		{
			return Redirect("OAuthLogin");
		}
		
		var referer = Request.Headers.Referer.ToString();
		string link;
		if (Uri.TryCreate(referer, UriKind.Absolute, out var refererUri) && refererUri.Host == Request.Host.Host)
			link = refererUri.PathAndQuery;
		else
			link = Url.Action("Index", HomeController.Name);
		ViewBag.Referrer = link;

		var externalLoginList = externalLoginRoutingService.GetActiveProviderTypeAndNameDictionary();
		
		return View(externalLoginList);
	}

	[PopForumsAuthenticationIgnore]
	public IActionResult OAuthLogin()
	{
		if (config.IsOAuthOnly)
		{
			var identityProviderRedirectUrl = Url.Action(nameof(IdentityController.CallbackHandler), IdentityController.Name, null, Request.Scheme);
			var redirect = oAuthOnlyService.GetLoginUrl(identityProviderRedirectUrl);
			var loginState = new ExternalLoginState {ProviderType = ProviderType.OAuthOnly, ReturnUrl = identityProviderRedirectUrl };
			externalLoginTempService.Persist(loginState);
			return View("OAuthLogin", redirect);
		}

		return RedirectToAction("Login");
	}

	[PopForumsAuthenticationIgnore]
	public async Task<ViewResult> Unsubscribe(int id, string key)
	{
		var user = await userService.GetUser(id);
		if (user == null || (await profileService.Unsubscribe(user, key) == false))
			return View("UnsubscribeFailure");
		return View();
	}

	[TypeFilter(typeof(OAuthOnlyForbidAttribute))]
	public async Task<ViewResult> ExternalLogins()
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return View("EditAccountNoUser");
		var externalAssociations = await externalUserAssociationManager.GetExternalUserAssociations(user);
		ViewBag.Referrer = Url.Action("ExternalLogins");
		return View(externalAssociations);
	}

	[TypeFilter(typeof(OAuthOnlyForbidAttribute))]
	public async Task<ActionResult> RemoveExternalLogin(int id)
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return View("EditAccountNoUser");
		await externalUserAssociationManager.RemoveAssociation(user, id, HttpContext.Connection.RemoteIpAddress?.ToString());
		return RedirectToAction("ExternalLogins");
	}

	public RedirectToActionResult MyProfile()
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return RedirectToAction("Create");
		return RedirectToAction("ViewProfile", new {id = user.UserID});
	}
}