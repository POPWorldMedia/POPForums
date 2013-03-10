using System;

namespace PopForums.Models
{
	public class UserEditSecurity
	{
		public UserEditSecurity() {}

		public UserEditSecurity(User user)
		{
			OldEmail = user.Email;
		}

		public string OldPassword { get; set; }
		public string NewPassword { get; set; }
		public string NewPasswordRetype { get; set; }
		public string OldEmail { get; set; }
		public string NewEmail { get; set; }
		public string NewEmailRetype { get; set; }

		public bool NewPasswordsMatch()
		{
			if (String.IsNullOrWhiteSpace(NewPassword) || String.IsNullOrWhiteSpace(NewPasswordRetype))
				return false;
			return NewPassword == NewPasswordRetype;
		}

		public bool NewEmailsMatch()
		{
			if (String.IsNullOrWhiteSpace(NewEmail) || String.IsNullOrWhiteSpace(NewEmailRetype))
				return false;
			return NewEmail == NewEmailRetype;
		}
	}
}