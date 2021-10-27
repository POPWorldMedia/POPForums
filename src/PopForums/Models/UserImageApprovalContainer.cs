namespace PopForums.Models;

public class UserImageApprovalContainer
{
	public bool IsNewUserImageApproved { get; set; }
	public List<UserImagePair> Unapproved { get; set; }
}

public class UserImagePair
{
	public User User { get; set; }
	public UserImage UserImage { get; set; }
}