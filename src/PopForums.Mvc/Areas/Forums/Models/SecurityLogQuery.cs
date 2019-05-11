using System;

namespace PopForums.Mvc.Areas.Forums.Models
{
	public class SecurityLogQuery
	{
		public string SearchTerm { get; set; }
		public string Type { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
	}
}