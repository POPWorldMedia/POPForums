namespace PopForums.Models.Subscriptions;

public class SubscriptionHistory
{
	public int SubscriptionHistoryID { get; set; }
	public int UserID { get; set; }
	public DateTime TimeStamp { get; set; }
	public string SkuID { get; set; }
	public string Message { get; set; }
}