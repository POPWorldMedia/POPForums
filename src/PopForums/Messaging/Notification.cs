namespace PopForums.Messaging;

public class Notification
{
	public int UserID { get; set; }
	public DateTime TimeStamp { get; set; }
	public bool IsRead { get; set; }
	public NotificationType NotificationType { get; set; }
	public long ContextID { get; set; }
	public JsonElement Data { get; set; }
	public int UnreadCount { get; set; }
}