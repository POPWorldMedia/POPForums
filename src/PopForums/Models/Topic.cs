using System;

namespace PopForums.Models
{
	public class Topic
	{
		public int TopicID { get; set; }
		public int ForumID { get; set; }
		public string Title { get; set; }
		public int ReplyCount { get; set; }
		public int ViewCount { get; set; }
		public int StartedByUserID { get; set; }
		public string StartedByName { get; set; }
		public int LastPostUserID { get; set; }
		public string LastPostName { get; set; }
		public DateTime LastPostTime { get; set; }
		public bool IsClosed { get; set; }
		public bool IsPinned { get; set; }
		public bool IsDeleted { get; set; }
		public string UrlName { get; set; }
		public int? AnswerPostID { get; set; }
	}
}
