using System;

namespace PopForums.Models
{
	public class Forum
	{
		public Forum(int forumID)
		{
			ForumID = forumID;
		}

		public int ForumID { get; private set; }
		public int? CategoryID { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public bool IsVisible { get; set; }
		public bool IsArchived { get; set; }
		public int SortOrder { get; set; }
		public int TopicCount { get; set; }
		public int PostCount { get; set; }
		public DateTime LastPostTime { get; set; }
		public string LastPostName { get; set; }
		public string UrlName { get; set; }
		public string ForumAdapterName { get; set; }
	}
}
