namespace PopForums.Models.Subscriptions;

public class Sku
{
	public string SkuID { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public decimal Price { get; set; }
	public bool IsActive { get; set; }
	public ushort Months { get; set; }
	public int SortOrder { get; set; }
}