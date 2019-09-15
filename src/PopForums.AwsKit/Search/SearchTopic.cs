using System;
using Nest;

namespace PopForums.AwsKit.Search
{
    public class SearchTopic
    {
		public string Id { get; set; }
		public int TopicID { get; set; }
		public int ForumID { get; set; }
		public string Title { get; set; }
		public DateTime LastPostTime { get; set; }
		public string StartedByName { get; set; }
		public int Replies { get; set; }
		public int Views { get; set; }
		public bool IsClosed { get; set; }
		public bool IsPinned { get; set; }
		public string UrlName { get; set; }
		public string LastPostName { get; set; }
		public string FirstPost { get; set; }
		public string[] Posts { get; set; }
		public string TenantID { get; set; }
    }
}
