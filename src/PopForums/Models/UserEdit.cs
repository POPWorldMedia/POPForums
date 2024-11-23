namespace PopForums.Models;

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
		Instagram = profile.Instagram;
		Facebook = profile.Facebook;
		HideVanity = profile.HideVanity;
		Roles = user.Roles.ToArray();
		AvatarID = profile.AvatarID;
		ImageID = profile.ImageID;
		IsAutoFollowOnReply = profile.IsAutoFollowOnReply;
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
	public string Instagram { get; set; }
	public string Facebook { get; set; }
	public bool HideVanity { get; set; }
	public string[] Roles { get; set; }
	public int? AvatarID { get; set; }
	public int? ImageID { get; set; }
	public bool DeleteAvatar { get; set; }
	public bool DeleteImage { get; set; }
	public bool IsAutoFollowOnReply { get; set; }
}