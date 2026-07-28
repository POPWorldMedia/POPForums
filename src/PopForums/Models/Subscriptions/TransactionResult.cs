namespace PopForums.Models.Subscriptions;

public class TransactionResult
{
	public bool IsSuccess { get; set; }
	public Transaction Transaction { get; set; }
	public string Message { get; set; }
}