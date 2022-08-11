namespace PopForums.Models;

public class PrivateMessage
{
	public int PMID { get; set; }
	public DateTime LastPostTime { get; set; }
	public JsonElement Users { get; set; }
	public DateTime LastViewDate { get; set; }
}