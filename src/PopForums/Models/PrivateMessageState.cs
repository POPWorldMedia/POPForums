namespace PopForums.Models;

public class PrivateMessageState
{
	public int PmID { get; set; }
	public JsonElement Users { get; set; }
	public dynamic[] Messages { get; set; }
	public int? NewestPostID { get; set; }
}