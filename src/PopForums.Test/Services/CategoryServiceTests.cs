using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;
using Xunit;

namespace PopForums.Test.Services
{
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

		[Fact]
		public async Task GetAll()
		{
			var service = GetService();
			var allCats = new List<Category>();
			_mockCategoryRepo.Setup(c => c.GetAll()).ReturnsAsync(allCats);
			var result = await service.GetAll();
			Assert.Same(allCats, result);
			_mockCategoryRepo.Verify(c => c.GetAll(), Times.Once());
		}

		[Fact]
		public async Task Create()
		{
			const string newTitle = "new category";
			var cat1 = new Category { CategoryID = 123, SortOrder = 0 };
			var cat2 = new Category { CategoryID = 456, SortOrder = 2 };
			var cat3 = new Category { CategoryID = 789, SortOrder = 4 };
			var cat4 = new Category { CategoryID = 1000, SortOrder = 6 };
			var newCat = new Category { CategoryID = 999, Title = newTitle, SortOrder = -2};
			var cats = new List<Category> { cat1, cat2, cat3, cat4, newCat };
			var service = GetService();
			_mockCategoryRepo.Setup(c => c.GetAll()).ReturnsAsync(cats);
			_mockCategoryRepo.Setup(c => c.Create(newTitle, -2)).ReturnsAsync(newCat);
			var result = await service.Create(newTitle);
			Assert.Equal(0, result.SortOrder);
			Assert.Equal(999, result.CategoryID);
			Assert.Equal(newTitle, result.Title);
			_mockCategoryRepo.Verify(c => c.Create(newTitle, -2), Times.Once());
			Assert.Equal(0, newCat.SortOrder);
			Assert.Equal(2, cat1.SortOrder);
			Assert.Equal(4, cat2.SortOrder);
			Assert.Equal(6, cat3.SortOrder);
			Assert.Equal(8, cat4.SortOrder);
		}

		[Fact]
		public void Delete()
		{
			var service = GetService();
			var cat = new Category { CategoryID = 123 };
			service.Delete(cat);
			_mockCategoryRepo.Verify(c => c.Delete(cat.CategoryID), Times.Once());
		}

		[Fact]
		public async Task DeleteByIdThrowsIfNotFound()
		{
			var service = GetService();
			_mockCategoryRepo.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync((Category) null);

			await Assert.ThrowsAsync<Exception>(async () => await service.Delete(1));
		}

		[Fact]
		public void DeleteResetsForumCatIDsToNull()
		{
			var service = GetService();
			var cat = new Category { CategoryID = 123 };
			var f1 = new Forum { ForumID = 1, CategoryID = cat.CategoryID };
			var f2 = new Forum { ForumID = 2, CategoryID = cat.CategoryID };
			var f3 = new Forum { ForumID = 3, CategoryID = 456 };
			var forums = new List<Forum> {f1, f2, f3};
			_mockForumRepo.Setup(f => f.GetAll()).ReturnsAsync(forums);
			service.Delete(cat);
			_mockForumRepo.Verify(f => f.UpdateCategoryAssociation(1, null), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateCategoryAssociation(2, null), Times.Once());
			_mockForumRepo.Verify(f => f.UpdateCategoryAssociation(3, null), Times.Never());
		}

		[Fact]
		public void UpdateTitle()
		{
			var savedCategory = new Category { CategoryID = 789 };
			var service = GetService();
			var cat = new Category { CategoryID = 123, Title = "old", SortOrder = 456 };
			_mockCategoryRepo.Setup(c => c.Update(cat)).Callback<Category>(category => savedCategory = category);
			service.UpdateTitle(cat, "new");
			_mockCategoryRepo.Verify(c => c.Update(It.IsAny<Category>()), Times.Once());
			Assert.Equal("new", savedCategory.Title);
			Assert.Equal(123, savedCategory.CategoryID);
			Assert.Equal(456, savedCategory.SortOrder);
		}

		[Fact]
		public async Task UpdateTitleByIdThrowsIfNotFound()
		{
			var service = GetService();
			_mockCategoryRepo.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync((Category)null);

			await Assert.ThrowsAsync<Exception>(async () => await service.UpdateTitle(1, ""));
		}

		[Fact]
		public async Task MoveUp()
		{
			var cat1 = new Category { CategoryID = 123, SortOrder = 0};
			var cat2 = new Category { CategoryID = 456, SortOrder = 2};
			var cat3 = new Category { CategoryID = 789, SortOrder = 4};
			var cat4 = new Category { CategoryID = 1000, SortOrder = 6};
			var cats = new List<Category> { cat1, cat2, cat3, cat4 };
			var service = GetService();
			_mockCategoryRepo.Setup(c => c.GetAll()).ReturnsAsync(cats);
			await service.MoveCategoryUp(cat3);
			_mockCategoryRepo.Verify(c => c.GetAll(), Times.Once());
			_mockCategoryRepo.Verify(c => c.Update(It.IsAny<Category>()), Times.Exactly(4));
			_mockCategoryRepo.Verify(c => c.Update(cat1), Times.Once());
			_mockCategoryRepo.Verify(c => c.Update(cat2), Times.Once());
			_mockCategoryRepo.Verify(c => c.Update(cat3), Times.Once());
			_mockCategoryRepo.Verify(c => c.Update(cat4), Times.Once());
			Assert.Equal(0, cat1.SortOrder);
			Assert.Equal(2, cat3.SortOrder);
			Assert.Equal(4, cat2.SortOrder);
			Assert.Equal(6, cat4.SortOrder);
		}

		[Fact]
		public async Task MoveDown()
		{
			var cat1 = new Category { CategoryID = 123, SortOrder = 0 };
			var cat2 = new Category { CategoryID = 456, SortOrder = 2 };
			var cat3 = new Category { CategoryID = 789, SortOrder = 4 };
			var cat4 = new Category { CategoryID = 1000, SortOrder = 6 };
			var cats = new List<Category> { cat1, cat2, cat3, cat4 };
			var service = GetService();
			_mockCategoryRepo.Setup(c => c.GetAll()).ReturnsAsync(cats);
			await service.MoveCategoryDown(cat3);
			_mockCategoryRepo.Verify(c => c.GetAll(), Times.Once());
			_mockCategoryRepo.Verify(c => c.Update(It.IsAny<Category>()), Times.Exactly(4));
			_mockCategoryRepo.Verify(c => c.Update(cat1), Times.Once());
			_mockCategoryRepo.Verify(c => c.Update(cat2), Times.Once());
			_mockCategoryRepo.Verify(c => c.Update(cat3), Times.Once());
			_mockCategoryRepo.Verify(c => c.Update(cat4), Times.Once());
			Assert.Equal(0, cat1.SortOrder);
			Assert.Equal(2, cat2.SortOrder);
			Assert.Equal(4, cat4.SortOrder);
			Assert.Equal(6, cat3.SortOrder);
		}

		[Fact]
		public async Task MoveUpByIdThrowsIfNotFound()
		{
			var service = GetService();
			_mockCategoryRepo.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync((Category)null);

			await Assert.ThrowsAsync<Exception>(async () => await service.MoveCategoryUp(1));
		}

		[Fact]
		public async Task MoveDownByIdThrowsIfNotFound()
		{
			var service = GetService();
			_mockCategoryRepo.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync((Category)null);

			await Assert.ThrowsAsync<Exception>(async () => await service.MoveCategoryDown(1));
		}
	}
}
