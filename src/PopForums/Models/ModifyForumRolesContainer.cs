namespace PopForums.Models
{
	public class ModifyForumRolesContainer
	{
		public int ForumID { get; set; }
		public ModifyForumRolesType ModifyType { get; set; }
		public string Role { get; set; }
	}
}