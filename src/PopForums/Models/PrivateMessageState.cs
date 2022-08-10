namespace PopForums.Models;

public class PrivateMessageState
{
	public int PmID { get; set; }
	public dynamic[] Users { get; set; }
	public dynamic[] Messages { get; set; }
}