using System;
using System.Web.Mvc;
using PopForums.Extensions;
using PopForums.Services;

namespace PopForums.Models
{
	public class SignupData
	{
		public string Name { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string PasswordRetype { get; set; }
		public int TimeZone { get; set; }
		public bool IsDaylightSaving { get; set; }
		public bool IsSubscribed { get; set; }
		public bool IsCoppa { get; set; }
		public bool IsTos { get; set; }

		public void Validate(ModelStateDictionary modelStateDictionary, IUserService userService, string ip)
		{
			if (!IsCoppa)
				modelStateDictionary.AddModelError("IsCoppa", Resources.MustBe13);
			if (!IsTos)
				modelStateDictionary.AddModelError("IsTos", Resources.MustAcceptTOS);
			userService.IsPasswordValid(Password, modelStateDictionary);
			if (Password != PasswordRetype)
				modelStateDictionary.AddModelError("PasswordRetype", Resources.RetypeYourPassword);
			if (String.IsNullOrWhiteSpace(Name))
				modelStateDictionary.AddModelError("Name", Resources.NameRequired);
			else if (userService.IsNameInUse(Name))
				modelStateDictionary.AddModelError("Name", Resources.NameInUse);
			if (String.IsNullOrWhiteSpace(Email))
				modelStateDictionary.AddModelError("Email", Resources.EmailRequired);
			else
				if (!Email.IsEmailAddress())
					modelStateDictionary.AddModelError("Email", Resources.ValidEmailAddressRequired);
				else if (Email != null && userService.IsEmailInUse(Email))
					modelStateDictionary.AddModelError("Email", Resources.EmailInUse);
			if (Email != null && userService.IsEmailBanned(Email))
				modelStateDictionary.AddModelError("Email", Resources.EmailBanned);
			if (userService.IsIPBanned(ip))
				modelStateDictionary.AddModelError("Email", Resources.IPBanned);
		}

		public static string GetCoppaDate()
		{
			return DateTime.Now.AddYears(-13).ToLongDateString();
		}
	}
}
