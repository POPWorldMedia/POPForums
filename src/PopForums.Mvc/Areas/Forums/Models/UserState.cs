namespace PopForums.Mvc.Areas.Forums.Models;

public class UserState
{
	public bool IsImageEnabled { get; set; }
	public bool IsPlainText { get; set; }
	public int NewPmCount { get; set; }
	public int NotificationCount { get; set; }
	public int UserID { get; set; }
}