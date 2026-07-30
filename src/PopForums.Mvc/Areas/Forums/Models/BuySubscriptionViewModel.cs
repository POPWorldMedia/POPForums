using PopForums.Models.Subscriptions;

namespace PopForums.Mvc.Areas.Forums.Models;

public class BuySubscriptionViewModel
{
	public List<Sku> Skus { get; set; }
	public DateOnly? Expiration { get; set; }
}
