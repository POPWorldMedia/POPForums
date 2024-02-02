namespace PopForums.Models;

public class PostImage
{
	public string ID { get; init; }
	public DateTime TimeStamp { get; init; }
	public string TenantID { get; init; }
	public string ContentType { get; init; }
	public byte[] ImageData { get; init; }

}