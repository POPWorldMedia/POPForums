using System;

namespace PopForums.Models
{
	public class UserEditProfile
	{
		public UserEditProfile() {}

		public UserEditProfile(Profile profile)
		{
			IsSubscribed = profile.IsSubscribed;
			Signature = profile.Signature;
			ShowDetails = profile.ShowDetails;
			Location = profile.Location;
			IsPlainText = profile.IsPlainText;
			Dob = profile.Dob;
			Web = profile.Web;
			Aim = profile.Aim;
			Icq = profile.Icq;
			YahooMessenger = profile.YahooMessenger;
			Facebook = profile.Facebook;
			Twitter = profile.Twitter;
			TimeZone = profile.TimeZone;
			IsDaylightSaving = profile.IsDaylightSaving;
			HideVanity = profile.HideVanity;
		}

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
		public int TimeZone { get; set; }
		public bool IsDaylightSaving { get; set; }
		public bool HideVanity { get; set; }
	}
}