using System.Collections.Generic;

namespace PopForums.Models
{
	public class VotePostContainer
	{
		public int PostID { get; set; }
		public int Votes { get; set; }
		public Dictionary<int, string> Voters { get; set; }
	}
}