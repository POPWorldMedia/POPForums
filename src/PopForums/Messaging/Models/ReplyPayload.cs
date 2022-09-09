namespace PopForums.Messaging.Models;

public class ReplyPayload
{
	public string PostName { get; set; }
	public string Title { get; set; }
	public int TopicID { get; set; }
	public int UserID { get; set; }
	public string TenantID { get; set; }
}