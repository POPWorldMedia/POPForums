using System;

namespace PopForums.Models
{
	public class Profile
	{
		public Profile()
		{
		}

		public Profile(int userID)
		{
			UserID = userID;
		}

		public int UserID { get; private set; }
		public bool IsSubscribed { get; set; }
		public string Signature { get; set; }
		public bool ShowDetails { get; set; }
		public string Location { get; set; }
		public bool IsPlainText { get; set; }
		public DateTime? Dob { get; set; }
		public string Web { get; set; }
		public string Aim { get; set; }
		public string Icq { get; set; }
		public string YahooMessenger { get; set; }
		public string Facebook { get; set; }
		public string Twitter { get; set; }
		public bool IsTos { get; set; }
		public int TimeZone { get; set; }
		public bool IsDaylightSaving { get; set; }
		public int? AvatarID { get; set; }
		public int? ImageID { get; set; }
		public bool HideVanity { get; set; }
		public int? LastPostID { get; set; }
		public int Points { get; set; }
	}
}