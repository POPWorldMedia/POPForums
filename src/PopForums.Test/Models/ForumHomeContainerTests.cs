namespace PopForums.Test.Models;

public class ForumHomeContainerTests
{
	[Fact]
	public void UncategorizedForumsShowUpOnProperty()
	{
		var c1 = new Category { CategoryID = 1 };
		var c2 = new Category { CategoryID = 2 };
		var f1 = new Forum {ForumID = 1, CategoryID = null};
		var f2 = new Forum { ForumID = 2, CategoryID = 1};
		var f3 = new Forum { ForumID = 3, CategoryID = 2};
		var f4 = new Forum { ForumID = 4, CategoryID = 0};
		var cats = new List<Category> {c1, c2};
		var forums = new List<Forum> {f1, f2, f3, f4};
		var container = new CategorizedForumContainer(cats, forums);
		Assert.Contains(f1, container.UncategorizedForums);
		Assert.DoesNotContain(f2, container.UncategorizedForums);
		Assert.DoesNotContain(f3, container.UncategorizedForums);
		Assert.Contains(f4, container.UncategorizedForums);
	}

	[Fact]
	public void UncategorizedInCorrectOrder()
	{
		var f1 = new Forum { ForumID = 1, SortOrder = 5 };
		var f2 = new Forum { ForumID = 2, SortOrder = 1 };
		var f3 = new Forum { ForumID = 3, SortOrder = 3 };
		var forums = new List<Forum> { f1, f2, f3 };
		var container = new CategorizedForumContainer(new List<Category>(), forums);
		Assert.True(container.UncategorizedForums[0] == f2);
		Assert.True(container.UncategorizedForums[1] == f3);
		Assert.True(container.UncategorizedForums[2] == f1);
	}

	[Fact]
	public void CategoriesInCorrectOrder()
	{
		var c1 = new Category { CategoryID = 1, SortOrder = 5 };
		var c2 = new Category { CategoryID = 2, SortOrder = 1 };
		var c3 = new Category { CategoryID = 3, SortOrder = 3 };
		var f1 = new Forum { ForumID = 1, CategoryID = 1 };
		var f2 = new Forum { ForumID = 2, CategoryID = 2 };
		var f3 = new Forum { ForumID = 3, CategoryID = 3 };
		var cats = new List<Category> {c1, c2, c3};
		var forums = new List<Forum> {f1, f2, f3};
		var container = new CategorizedForumContainer(cats, forums);
		Assert.True(container.CategoryDictionary.ToArray()[0].Key == c2);
		Assert.True(container.CategoryDictionary.ToArray()[1].Key == c3);
		Assert.True(container.CategoryDictionary.ToArray()[2].Key == c1);
	}

	[Fact]
	public void AllCollectionsPersist()
	{
		var c1 = new Category { CategoryID = 1 };
		var c2 = new Category { CategoryID = 2 };
		var f1 = new Forum { ForumID = 1, CategoryID = null };
		var f2 = new Forum { ForumID = 2, CategoryID = 1 };
		var f3 = new Forum { ForumID = 3, CategoryID = 2 };
		var cats = new List<Category> { c1, c2 };
		var forums = new List<Forum> { f1, f2, f3 };
		var container = new CategorizedForumContainer(cats, forums);
		Assert.Equal(cats, container.AllCategories);
		Assert.Equal(forums, container.AllForums);
	}

	[Fact]
	public void ForumsAppearInCategories()
	{
		var c1 = new Category { CategoryID = 1, Title = "Cat1" };
		var c2 = new Category { CategoryID = 2, Title = "Cat2" };
		var f1 = new Forum { ForumID = 1, CategoryID = null };
		var f2 = new Forum { ForumID = 2, CategoryID = 1 };
		var f3 = new Forum { ForumID = 3, CategoryID = 2 };
		var cats = new List<Category> { c1, c2 };
		var forums = new List<Forum> { f1, f2, f3 };
		var container = new CategorizedForumContainer(cats, forums);
		Assert.Contains(f2, container.CategoryDictionary[c1]);
		Assert.Contains(f3, container.CategoryDictionary[c2]);
		Assert.DoesNotContain(f1, container.CategoryDictionary[c1]);
		Assert.DoesNotContain(f3, container.CategoryDictionary[c1]);
	}
		
	[Fact]
	public void CategoryWithNoForumsDoesNotAppear()
	{
		var c1 = new Category { CategoryID = 1, Title = "Cat1" };
		var c2 = new Category { CategoryID = 2, Title = "Cat2" };
		var c3 = new Category { CategoryID = 3, Title = "Cat3" };
		var f1 = new Forum { ForumID = 1, CategoryID = null };
		var f2 = new Forum { ForumID = 2, CategoryID = 1 };
		var f3 = new Forum { ForumID = 3, CategoryID = 2 };
		var cats = new List<Category> { c1, c2, c3 };
		var forums = new List<Forum> { f1, f2, f3 };
		var container = new CategorizedForumContainer(cats, forums);
		Assert.False(container.CategoryDictionary.ContainsKey(c3));
	}
}