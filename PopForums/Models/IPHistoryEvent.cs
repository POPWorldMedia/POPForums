using System;

namespace PopForums.Models
{
	public class IPHistoryEvent
	{
		public DateTime EventTime { get; set; }
		public Type Type { get; set; }
		public string Description { get; set; }
		public int? UserID { get; set; }
		public string Name { get; set; }
		public object ID { get; set; }
	}
}
