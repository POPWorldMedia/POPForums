using System;

namespace PopForums.Models
{
	public class FeedEvent
	{
		public int UserID { get; set; }
		public string Message { get; set; }
		public int Points { get; set; }
		public DateTime TimeStamp { get; set; }
	}
}