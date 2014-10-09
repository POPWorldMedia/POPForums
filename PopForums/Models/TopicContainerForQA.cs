using System.Collections.Generic;

namespace PopForums.Models
{
	public class TopicContainerForQA : TopicContainer
	{
		public Post QuestionPost { get; set; }
		public List<Post> QuestionComments { get; set; } 
		public List<AnswerWithComments> AnswersWithComments { get; set; } 
	}
}
