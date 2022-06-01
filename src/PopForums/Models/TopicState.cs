namespace PopForums.Models;

public class TopicState
{
	public int TopicID { get; set; }
	public bool IsImageEnabled { get; set; }
	public bool IsSubscribed { get; set; }
	public bool IsFavorite { get; set; }
	public int? PageIndex { get; set; }
	public int? PageCount { get; set; }
	public int LastVisiblePostID { get; set; }
	public int? AnswerPostID { get; set; }
}