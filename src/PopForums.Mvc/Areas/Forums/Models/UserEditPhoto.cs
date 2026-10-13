namespace PopForums.Mvc.Areas.Forums.Models;

public class UserEditPhoto
{
	public UserEditPhoto() { }

	public UserEditPhoto(Profile profile)
	{
		AvatarID = profile.AvatarID;
		ImageID = profile.ImageID;
	}

	public int? AvatarID { get; set; }
	public int? ImageID { get; set; }
	public bool? IsImageApproved { get; set; }
}