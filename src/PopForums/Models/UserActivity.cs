using System;

namespace PopForums.Models
{
	public class UserActivity
	{
		public int UserID { get; set; }
		public DateTime LastActivityDate { get; set; }
		public DateTime LastLoginDate { get; set; }
	}
}