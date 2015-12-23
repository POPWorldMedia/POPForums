namespace PopForums.Models
{
	public class UserImage
	{
		public int UserImageID { get; set; }
		public int UserID { get; set; }
		public int SortOrder { get; set; }
		public bool IsApproved { get; set; }
	}
}
