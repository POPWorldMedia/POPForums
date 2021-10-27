namespace PopForums.Models;

public class CategoryContainerWithForums
{
	public Category Category { get; set; }
	public IEnumerable<Forum> Forums { get; set; }
}