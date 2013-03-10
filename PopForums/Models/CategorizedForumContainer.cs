using System.Collections.Generic;
using System.Linq;

namespace PopForums.Models
{
	public class CategorizedForumContainer
	{
		public CategorizedForumContainer(IEnumerable<Category> categories, IEnumerable<Forum> forums)
		{
			ReadStatusLookup = new Dictionary<int, ReadStatus>();
			AllCategories = categories;
			AllForums = forums;
			UncategorizedForums = forums.Where(f => !f.CategoryID.HasValue || f.CategoryID == 0).OrderBy(f => f.SortOrder).ToList();
			CategoryDictionary = new Dictionary<Category, List<Forum>>();
			foreach (var category in AllCategories.OrderBy(c => c.SortOrder))
			{
				var forumSet = AllForums.Where(f => f.CategoryID == category.CategoryID).OrderBy(f => f.SortOrder).ToList();
				if (forumSet.Count > 0)
					CategoryDictionary.Add(category, forumSet);
			}
		}

		public IEnumerable<Category> AllCategories { get; private set; }
		public IEnumerable<Forum> AllForums { get; private set; }
		public List<Forum> UncategorizedForums { get; private set; }
		public Dictionary<Category, List<Forum>> CategoryDictionary { get; private set; }
		public string ForumTitle { get; set; }
		public Dictionary<int, ReadStatus> ReadStatusLookup { get; private set; }
	}
}