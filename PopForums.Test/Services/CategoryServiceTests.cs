using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Test.Services
{
	[TestFixture]
	public class CategoryServiceTests
	{
		private Mock<ICategoryRepository> _mockCategoryRepo;
		private Mock<IForumRepository> _mockForumRepo;

		private ICategoryService GetService()
		{
			_mockCategoryRepo = new Mock<ICategoryRepository>();
			_mockForumRepo = new Mock<IForumRepository>();
			var service = new CategoryService(_mockCategoryRepo.Object, _mockForumRepo.Object);
			return service;
		}

		[Test]
		public void GetAll()
		{
			var service = GetService();
			var allCats = new List<Category>();
			_mockCategoryRepo.Setup(c => c.GetAll()).Returns(allCats);
			var result = service.GetAll();
			Assert.AreSame(allCats, result);
			_mockCategoryRepo.Verify(c => c.GetAll(), Times.Once());
		}

		[Test]
		public void Create()
		{
			const string newTitle = "new category";
			var cat1 = new Category(123) { SortOrder = 0 };
			var cat2 = new Category(456) { SortOrder = 2 };
			var cat3 = new Category(789) { SortOrder = 4 };
			var cat4 = new Category(1000) { SortOrder = 6 };
			var newCat = new Category(999) {Title = newTitle, SortOrder = -2};
			var cats = new List<Category> { cat1, cat2, cat3, cat4, newCat };
			var service = GetService();
			_mockCategoryRepo.Setup(c => c.GetAll()).Returns(cats);
			_mockCategoryRepo.Setup(c => c.Create(newTitle, -2)).Returns(newCat);
			var result = service.Create(newTitle);
			Assert.AreEqual(0, result.SortOrder);
			Assert.AreEqual(999, result.CategoryID);
			Assert.AreEqual(newTitle, result.Title);
			_mockCategoryRepo.Verify(c => c.Create(newTitle, -2), Times.Once());
			Assert.AreEqual(0, newCat.SortOrder);
			Assert.AreEqual(2, cat1.SortOrder);
			Assert.AreEqual(4, cat2.SortOrder);
			Assert.AreEqual(6, cat3.SortOrder);
			Assert.AreEqual(8, cat4.SortOrder);
		}

		[Test]
		public void Delete()
		{
			var service = GetService();
			var cat = new Category(123);
			service.Delete(cat);
			_mockCategoryRepo.Verify(c => c.Delete(cat.CategoryID), Times.Once());
		}

		[Test]
		public void DeleteResetsForumCatIDsToNull()
		{
			var service = GetService();
			var cat = new Category(123);
			var f1 = new Forum(1) { CategoryID = cat.CategoryID };
			var f2 = new Forum(2) { CategoryID = cat.CategoryID };
			var f3 = new Forum(3) { CategoryID = 456 };
			var forums = new List<Forum> {f1, f2, f3};
			_mockForumRepo.Setup(f => f.GetAll()).Returns(forums);
			service.Delete(cat);
			_mockForumRepo.Verify(f => f.UpdateCategoryAssociation(1, null), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateCategoryAssociation(2, null), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateCategoryAssociation(3, null), Times.Never());
		}

		[Test]
		public void UpdateTitle()
		{
			var savedCategory = new Category(789);
			var service = GetService();
			var cat = new Category(123) { Title = "old", SortOrder = 456 };
			_mockCategoryRepo.Setup(c => c.Update(cat)).Callback<Category>(category => savedCategory = category);
			service.UpdateTitle(cat, "new");
			_mockCategoryRepo.Verify(c => c.Update(It.IsAny<Category>()), Times.Once());
			Assert.AreEqual("new", savedCategory.Title);
			Assert.AreEqual(123, savedCategory.CategoryID);
			Assert.AreEqual(456, savedCategory.SortOrder);
		}

		[Test]
		public void MoveUp()
		{
			var cat1 = new Category(123) {SortOrder = 0};
			var cat2 = new Category(456) {SortOrder = 2};
			var cat3 = new Category(789) {SortOrder = 4};
			var cat4 = new Category(1000) {SortOrder = 6};
			var cats = new List<Category> { cat1, cat2, cat3, cat4 };
			var service = GetService();
			_mockCategoryRepo.Setup(c => c.GetAll()).Returns(cats);
			service.MoveCategoryUp(cat3);
			_mockCategoryRepo.Verify(c => c.GetAll(), Times.Once());
			_mockCategoryRepo.Verify(c => c.Update(It.IsAny<Category>()), Times.Exactly(4));
			_mockCategoryRepo.Verify(c => c.Update(cat1), Times.Once());
			_mockCategoryRepo.Verify(c => c.Update(cat2), Times.Once());
			_mockCategoryRepo.Verify(c => c.Update(cat3), Times.Once());
			_mockCategoryRepo.Verify(c => c.Update(cat4), Times.Once());
			Assert.AreEqual(0, cat1.SortOrder);
			Assert.AreEqual(2, cat3.SortOrder);
			Assert.AreEqual(4, cat2.SortOrder);
			Assert.AreEqual(6, cat4.SortOrder);
		}

		[Test]
		public void MoveDown()
		{
			var cat1 = new Category(123) { SortOrder = 0 };
			var cat2 = new Category(456) { SortOrder = 2 };
			var cat3 = new Category(789) { SortOrder = 4 };
			var cat4 = new Category(1000) { SortOrder = 6 };
			var cats = new List<Category> { cat1, cat2, cat3, cat4 };
			var service = GetService();
			_mockCategoryRepo.Setup(c => c.GetAll()).Returns(cats);
			service.MoveCategoryDown(cat3);
			_mockCategoryRepo.Verify(c => c.GetAll(), Times.Once());
			_mockCategoryRepo.Verify(c => c.Update(It.IsAny<Category>()), Times.Exactly(4));
			_mockCategoryRepo.Verify(c => c.Update(cat1), Times.Once());
			_mockCategoryRepo.Verify(c => c.Update(cat2), Times.Once());
			_mockCategoryRepo.Verify(c => c.Update(cat3), Times.Once());
			_mockCategoryRepo.Verify(c => c.Update(cat4), Times.Once());
			Assert.AreEqual(0, cat1.SortOrder);
			Assert.AreEqual(2, cat2.SortOrder);
			Assert.AreEqual(4, cat4.SortOrder);
			Assert.AreEqual(6, cat3.SortOrder);
		}
	}
}
