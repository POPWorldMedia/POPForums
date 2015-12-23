using System.Collections.Generic;

namespace PopForums.Models
{
	public class TopicContainerForQA : TopicContainer
	{
		public PostWithChildren QuestionPostWithComments { get; set; }
		public List<PostWithChildren> AnswersWithComments { get; set; } 
	}
}
