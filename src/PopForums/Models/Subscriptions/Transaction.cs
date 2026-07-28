namespace PopForums.Models.Subscriptions;

public class Transaction
{
	public int TransactionID { get; set; }
	public string ProcessorID { get; set; }
	public string CustomerID { get; set; }
	public string Status { get; set; }
	public string Raw { get; set; }
	public string Last4 { get; set; }
	public int UserID { get; set; }
	public DateTime TimeStamp { get; set; }
	public string SkuID { get; set; }
	public decimal Amount { get; set; }
}