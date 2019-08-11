using System;

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
		public string Token { get; set; }

		public static string GetCoppaDate()
		{
			return DateTime.Now.AddYears(-13).ToString("D");
		}
	}
}
