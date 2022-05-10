namespace PopForums.Mvc.Models;

public class UIState
{
	public bool IsLoggedIn { get; set; }
	public int NewPmCount { get; set; }
	public string Name { get; set; } = string.Empty;
	public int UserID { get; set; }
	public bool IsAdmin { get; set; }
}