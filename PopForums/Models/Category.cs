namespace PopForums.Models
{
	public class Category
	{
		public Category(int categoryID)
		{
			CategoryID = categoryID;
		}

		public int CategoryID { get; private set; }
		public string Title { get; set; }
		public int SortOrder { get; set; }
	}
}
