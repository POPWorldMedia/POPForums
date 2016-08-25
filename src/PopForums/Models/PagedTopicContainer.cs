using System.Collections.Generic;

namespace PopForums.Models
{
	public class PagedTopicContainer
	{
		public PagedTopicContainer()
		{
			ReadStatusLookup = new Dictionary<int, ReadStatus>();
		}

		public List<Topic> Topics { get; set; }
		public PagerContext PagerContext { get; set; }
		public Dictionary<int, ReadStatus> ReadStatusLookup { get; private set; }
		public Dictionary<int, string> ForumTitles { get; set; }
	}
}
