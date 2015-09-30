using System;

namespace PopForums.Models
{
	public class ExpiredUserSession
	{
		public int SessionID { get; set; }
		public int? UserID { get; set; }
		public DateTime LastTime { get; set; }
	}
}