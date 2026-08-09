namespace PopForums.Mvc.Areas.Forums.Models;

public class UserSubscriptionEdit
{
	public int UserID { get; set; }
	public string Name { get; set; }
	public string SkuID { get; set; }
	public DateOnly? Expiration { get; set; }
}
