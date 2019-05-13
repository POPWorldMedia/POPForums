using System.Collections.Generic;

namespace PopForums.Models
{
	public class PagedList<T> : PagerContext where T : class
	{
		public IEnumerable<T> List { get; set; }
	}
}
