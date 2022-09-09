namespace PopForums.Models;

public class SubscribeNotificationPayload
{
	public int TopicID { get; set; }
	public string TopicTitle { get; set; }
	public int PostingUserID { get; set; }
	public string PostingUserName { get; set; }
}