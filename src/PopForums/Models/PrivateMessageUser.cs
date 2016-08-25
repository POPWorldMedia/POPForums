using System;

namespace PopForums.Models
{
	public class PrivateMessageUser
	{
		public int PMID { get; set; }
		public int UserID { get; set; }
		public DateTime LastViewDate { get; set; }
		public bool IsArchived { get; set; }
	}
}
