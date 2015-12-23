using System;

namespace PopForums.Models
{
	public class ModerationLogEntry
	{
		public int ModerationID { get; set; }
		public DateTime TimeStamp { get; set; }
		public int UserID { get; set; }
		public string UserName { get; set; }
		public ModerationType ModerationType { get; set; }
		public int? ForumID { get; set; }
		public int TopicID { get; set; }
		public int? PostID { get; set; }
		public string Comment { get; set; }
		public string OldText { get; set; }
	}
}