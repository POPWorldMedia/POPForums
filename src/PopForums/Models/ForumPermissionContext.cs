namespace PopForums.Models
{
	public class ForumPermissionContext
	{
		public bool UserCanView { get; set; }
		public bool UserCanPost { get; set; }
		public bool UserCanModerate { get; set; }
		public string DenialReason { get; set; }
	}
}