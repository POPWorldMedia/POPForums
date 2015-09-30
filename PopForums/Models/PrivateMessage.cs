using System;

namespace PopForums.Models
{
	public class PrivateMessage
	{
		public int PMID { get; set; }
		public string Subject { get; set; }
		public DateTime LastPostTime { get; set; }
		public string UserNames { get; set; }
		public DateTime LastViewDate { get; set; }
	}
}
