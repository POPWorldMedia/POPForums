namespace PopForums.Models;

public class PostImage
{
	public string ID { get; set; }
	public DateTime TimeStamp { get; set; }
	public string TenantID { get; set; }
	public string ContentType { get; set; }
	public byte[] ImageData { get; set; }

}