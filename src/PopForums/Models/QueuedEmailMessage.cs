using System;

namespace PopForums.Models
{
	public class QueuedEmailMessage : EmailMessage
	{
		public int MessageID { get; set; }
		public DateTime QueueTime { get; set; }
	}
}