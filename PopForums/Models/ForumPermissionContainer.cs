using System.Collections.Generic;

namespace PopForums.Models
{
	public class ForumPermissionContainer
	{
		public int ForumID { get; set; }
		public List<string> AllRoles { get; set; }
		public List<string> PostRoles { get; set; }
		public List<string> ViewRoles { get; set; }
	}
}