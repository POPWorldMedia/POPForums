namespace PopForums.Models;

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
		Instagram = profile.Instagram;
		Facebook = profile.Facebook;
		HideVanity = profile.HideVanity;
		IsAutoFollowOnReply = profile.IsAutoFollowOnReply;
	}

	public bool IsSubscribed { get; set; }
	public string Signature { get; set; }
	public bool ShowDetails { get; set; }
	public string Location { get; set; }
	public bool IsPlainText { get; set; }
	public DateTime? Dob { get; set; }
	public string Web { get; set; }
	public string Instagram { get; set; }
	public string Facebook { get; set; }
	public bool HideVanity { get; set; }
	public bool IsAutoFollowOnReply { get; set; }
}