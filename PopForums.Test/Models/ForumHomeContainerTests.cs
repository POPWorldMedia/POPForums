using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PopForums.Models;

namespace PopForums.Test.Models
{
	[TestFixture]
	public class ForumHomeContainerTests
	{
		[Test]
		public void UncategorizedForumsShowUpOnProperty()
		{
			var c1 = new Category(1);
			var c2 = new Category(2);
			var f1 = new Forum(1) {CategoryID = null};
			var f2 = new Forum(2) {CategoryID = 1};
			var f3 = new Forum(3) {CategoryID = 2};
			var f4 = new Forum(4) {CategoryID = 0};
			var cats = new List<Category> {c1, c2};
			var forums = new List<Forum> {f1, f2, f3, f4};
			var container = new CategorizedForumContainer(cats, forums);
			Assert.True(container.UncategorizedForums.Contains(f1));
			Assert.False(container.UncategorizedForums.Contains(f2));
			Assert.False(container.UncategorizedForums.Contains(f3));
			Assert.True(container.UncategorizedForums.Contains(f4));
		}

		[Test]
		public void UncategorizedInCorrectOrder()
		{
			var f1 = new Forum(1) { SortOrder = 5 };
			var f2 = new Forum(2) { SortOrder = 1 };
			var f3 = new Forum(3) { SortOrder = 3 };
			var forums = new List<Forum> { f1, f2, f3 };
			var container = new CategorizedForumContainer(new List<Category>(), forums);
			Assert.True(container.UncategorizedForums[0] == f2);
			Assert.True(container.UncategorizedForums[1] == f3);
			Assert.True(container.UncategorizedForums[2] == f1);
		}

		[Test]
		public void CategoriesInCorrectOrder()
		{
			var c1 = new Category(1) { SortOrder = 5 };
			var c2 = new Category(2) { SortOrder = 1 };
			var c3 = new Category(3) { SortOrder = 3 };
			var f1 = new Forum(1) { CategoryID = 1 };
			var f2 = new Forum(2) { CategoryID = 2 };
			var f3 = new Forum(3) { CategoryID = 3 };
			var cats = new List<Category> {c1, c2, c3};
			var forums = new List<Forum> {f1, f2, f3};
			var container = new CategorizedForumContainer(cats, forums);
			Assert.True(container.CategoryDictionary.ToArray()[0].Key == c2);
			Assert.True(container.CategoryDictionary.ToArray()[1].Key == c3);
			Assert.True(container.CategoryDictionary.ToArray()[2].Key == c1);
		}

		[Test]
		public void AllCollectionsPersist()
		{
			var c1 = new Category(1);
			var c2 = new Category(2);
			var f1 = new Forum(1) { CategoryID = null };
			var f2 = new Forum(2) { CategoryID = 1 };
			var f3 = new Forum(3) { CategoryID = 2 };
			var cats = new List<Category> { c1, c2 };
			var forums = new List<Forum> { f1, f2, f3 };
			var container = new CategorizedForumContainer(cats, forums);
			Assert.AreEqual(cats, container.AllCategories);
			Assert.AreEqual(forums, container.AllForums);
		}

		[Test]
		public void ForumsAppearInCategories()
		{
			var c1 = new Category(1) { Title = "Cat1" };
			var c2 = new Category(2) { Title = "Cat2" };
			var f1 = new Forum(1) { CategoryID = null };
			var f2 = new Forum(2) { CategoryID = 1 };
			var f3 = new Forum(3) { CategoryID = 2 };
			var cats = new List<Category> { c1, c2 };
			var forums = new List<Forum> { f1, f2, f3 };
			var container = new CategorizedForumContainer(cats, forums);
			Assert.True(container.CategoryDictionary[c1].Contains(f2));
			Assert.True(container.CategoryDictionary[c2].Contains(f3));
			Assert.False(container.CategoryDictionary[c1].Contains(f1));
			Assert.False(container.CategoryDictionary[c1].Contains(f3));
		}
		
		[Test]
		public void CategoryWithNoForumsDoesNotAppear()
		{
			var c1 = new Category(1) { Title = "Cat1" };
			var c2 = new Category(2) { Title = "Cat2" };
			var c3 = new Category(3) { Title = "Cat3" };
			var f1 = new Forum(1) { CategoryID = null };
			var f2 = new Forum(2) { CategoryID = 1 };
			var f3 = new Forum(3) { CategoryID = 2 };
			var cats = new List<Category> { c1, c2, c3 };
			var forums = new List<Forum> { f1, f2, f3 };
			var container = new CategorizedForumContainer(cats, forums);
			Assert.False(container.CategoryDictionary.ContainsKey(c3));
		}
	}
}
