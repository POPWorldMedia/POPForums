using System.Collections.Generic;

namespace PopForums.Models
{
	public class PostWithChildren
	{
		public Post Post { get; set; }
		public List<Post> Children { get; set; } 
	}
}
