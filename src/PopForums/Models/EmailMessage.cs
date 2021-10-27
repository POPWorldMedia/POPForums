namespace PopForums.Models;

public class EmailMessage
{
	public string FromEmail { get; set; }
	public string FromName { get; set; }
	public string ToEmail { get; set; }
	public string ToName { get; set; }
	public string Subject { get; set; }
	public string Body { get; set; }
	public string HtmlBody { get; set; }
}