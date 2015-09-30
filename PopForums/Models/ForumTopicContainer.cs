namespace PopForums.Models
{
	public class ForumTopicContainer : PagedTopicContainer
	{
		public Forum Forum { get; set; }
		public ForumPermissionContext PermissionContext { get; set; }
	}
}
