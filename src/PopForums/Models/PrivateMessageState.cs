namespace PopForums.Models;

public class PrivateMessageState
{
	public int PmID { get; set; }
	public JsonElement Users { get; set; }
	public ClientPrivateMessagePost[] Messages { get; set; }
	public int? NewestPostID { get; set; }
	public bool IsUserNotFound { get; set; }
}