using System;

namespace PopForums.Models
{
	public class UserEdit
	{
		public UserEdit() {}

		public UserEdit(User user, Profile profile)
		{
			UserID = user.UserID;
			Name = user.Name;
			Email = user.Email;
			IsApproved = user.IsApproved;
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
			Roles = user.Roles.ToArray();
			AvatarID = profile.AvatarID;
			ImageID = profile.ImageID;
		}

		public int UserID { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string NewEmail { get; set; }
		public string NewPassword { get; set; }
		public bool IsApproved { get; set; }
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
		public string[] Roles { get; set; }
		public int? AvatarID { get; set; }
		public int? ImageID { get; set; }
		public bool DeleteAvatar { get; set; }
		public bool DeleteImage { get; set; }
	}
}