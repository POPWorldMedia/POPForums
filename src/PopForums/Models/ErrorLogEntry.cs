using System;
using PopForums.Configuration;

namespace PopForums.Models
{
	public class ErrorLogEntry
	{
		public int ErrorID { get; set; }
		public DateTime TimeStamp { get; set; }
		public string Message { get; set; }
		public string StackTrace { get; set; }
		public string Data { get; set; }
		public ErrorSeverity Severity { get; set; }
	}
}