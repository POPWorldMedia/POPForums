namespace PopForums.Mvc.Areas.Forums.Models;

public class TopicState
{
	public int TopicID { get; set; }
	public bool IsImageEnabled { get; set; }
	public bool IsSubscribed { get; set; }
	public bool IsFavorite { get; set; }
}