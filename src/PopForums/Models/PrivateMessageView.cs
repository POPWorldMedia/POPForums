using System.Collections.Generic;

namespace PopForums.Models
{
	public class PrivateMessageView
	{
		public PrivateMessage PrivateMessage { get; set; }
		public List<PrivateMessagePost> Posts { get; set; }
	}
}
