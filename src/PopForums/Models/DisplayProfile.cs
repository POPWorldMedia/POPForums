namespace PopForums.Models;

public class DisplayProfile
{
	public DisplayProfile(User user, Profile profile, UserImage userImage)
	{
		UserID = user.UserID;
		Name = user.Name;
		Joined = user.CreationDate;
		Dob = profile.Dob;
		Location = profile.Location;
		Web = profile.Web;
		Instagram = profile.Instagram;
		Facebook = profile.Facebook;
		AvatarID = profile.AvatarID;
		ImageID = profile.ImageID;
		ShowDetails = profile.ShowDetails;
		if (userImage != null && userImage.IsApproved)
			IsImageApproved = true;
		Points = profile.Points;
		IsApproved = user.IsApproved;
	}

	public int UserID { get; set; }
	public string Name { get; set; }
	public DateTime Joined { get; set; }
	public DateTime? Dob { get; set; }
	public string Location { get; set; }
	public int PostCount { get; set; }
	public string Web { get; set; }
	public string Instagram { get; set; }
	public string Facebook { get; set; }
	public int? AvatarID { get; set; }
	public int? ImageID { get; set; }
	public bool ShowDetails { get; set; }
	public bool IsImageApproved { get; set; }
	public int Points { get; set; }
	public List<FeedEvent> Feed { get; set; }
	public List<UserAward> UserAwards { get; set; }
	public bool IsApproved { get; set; }
}