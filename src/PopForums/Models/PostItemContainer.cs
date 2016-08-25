using System.Collections.Generic;

namespace PopForums.Models
{
	public class PostItemContainer
	{
		public Post Post { get; set; }
		public List<int> VotedPostIDs { get; set; }
		public Dictionary <int, string> Signatures { get; set; }
		public Dictionary<int, int> Avatars { get; set; }
		public User User { get; set; }
		public Profile Profile { get; set; }
		public Topic Topic { get; set; }
	}
}