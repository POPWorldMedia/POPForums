using System.Collections.Generic;

namespace PopForums.Models
{
	public class AnswerWithComments
	{
		public Post Answer { get; set; }
		public List<Post> Comments { get; set; } 
	}
}
