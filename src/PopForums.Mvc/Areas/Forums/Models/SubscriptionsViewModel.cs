namespace PopForums.Mvc.Areas.Forums.Models;

public class SubscriptionsViewModel
{
	public bool IsSubscriber { get; set; }
	public DateOnly? Expiration { get; set; }
	public string SkuName { get; set; }
	public int Months { get; set; }
	public bool IsAutoRenewal { get; set; }
	public string Last4 { get; set; }
}
