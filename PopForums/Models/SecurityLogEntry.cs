using System;

namespace PopForums.Models
{
	public class SecurityLogEntry
	{
		public int SecurityLogID { get; set; }
		public SecurityLogType SecurityLogType { get; set; }
		public int? UserID { get; set; }
		public int? TargetUserID { get; set; }
		public string IP { get; set; }
		public string Message { get; set; }
		public DateTime ActivityDate { get; set; }
	}
}