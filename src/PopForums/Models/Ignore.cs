namespace PopForums.Models;

public class Ignore
{
	public int UserID { get; set; }
	public int IgnoreUserID { get; set; }
}

public class IgnoreWithName : Ignore
{
	public string Name { get; set; }
}