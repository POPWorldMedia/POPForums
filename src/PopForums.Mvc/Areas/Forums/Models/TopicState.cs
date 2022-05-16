namespace PopForums.Mvc.Areas.Forums.Models;

public class TopicState
{
	public int TopicID { get; set; }
	public bool IsPlainText { get; set; }
	public bool IsImageEnabled { get; set; }
	public string NextQuote { get; set; }
}