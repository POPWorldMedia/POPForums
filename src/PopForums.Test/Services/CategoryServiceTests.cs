namespace PopForums.Test.Services;

public class CategoryServiceTests
{
	private ICategoryRepository _mockCategoryRepo;
	private IForumRepository _mockForumRepo;

	private ICategoryService GetService()
	{
		_mockCategoryRepo = Substitute.For<ICategoryRepository>();
		_mockForumRepo = Substitute.For<IForumRepository>();
		var service = new CategoryService(_mockCategoryRepo, _mockForumRepo);
		return service;
	}

	[Fact]
	public async Task GetAll()
	{
		var service = GetService();
		var allCats = new List<Category>();
		_mockCategoryRepo.GetAll().Returns(Task.FromResult(allCats));
		var result = await service.GetAll();
		Assert.Same(allCats, result);
		await _mockCategoryRepo.Received().GetAll();
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
		_mockCategoryRepo.GetAll().Returns(Task.FromResult(cats));
		_mockCategoryRepo.Create(newTitle, -2).Returns(Task.FromResult(newCat));
		var result = await service.Create(newTitle);
		Assert.Equal(0, result.SortOrder);
		Assert.Equal(999, result.CategoryID);
		Assert.Equal(newTitle, result.Title);
		await _mockCategoryRepo.Received().Create(newTitle, -2);
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
		_mockCategoryRepo.Received().Delete(cat.CategoryID);
	}

	[Fact]
	public async Task DeleteByIdThrowsIfNotFound()
	{
		var service = GetService();
		_mockCategoryRepo.Get(Arg.Any<int>()).Returns(Task.FromResult((Category)null));

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
		_mockForumRepo.GetAll().Returns(forums);
		service.Delete(cat);
		_mockForumRepo.Received().UpdateCategoryAssociation(1, null);
		_mockForumRepo.Received().UpdateCategoryAssociation(2, null);
		_mockForumRepo.DidNotReceive().UpdateCategoryAssociation(3, null);
	}

	[Fact]
	public void UpdateTitle()
	{
		var savedCategory = new Category { CategoryID = 789 };
		var service = GetService();
		var cat = new Category { CategoryID = 123, Title = "old", SortOrder = 456 };
		_mockCategoryRepo.Update(Arg.Do<Category>(x => savedCategory = x));
		service.UpdateTitle(cat, "new");
		_mockCategoryRepo.Received().Update(Arg.Any<Category>());
		Assert.Equal("new", savedCategory.Title);
		Assert.Equal(123, savedCategory.CategoryID);
		Assert.Equal(456, savedCategory.SortOrder);
	}

	[Fact]
	public async Task UpdateTitleByIdThrowsIfNotFound()
	{
		var service = GetService();
		_mockCategoryRepo.Get(Arg.Any<int>()).Returns((Category)null);

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
		_mockCategoryRepo.GetAll().Returns(Task.FromResult(cats));
		await service.MoveCategoryUp(cat3);
		await _mockCategoryRepo.Received().GetAll();
		await _mockCategoryRepo.Received(4).Update(Arg.Any<Category>());
		await _mockCategoryRepo.Received().Update(cat1);
		await _mockCategoryRepo.Received().Update(cat2);
		await _mockCategoryRepo.Received().Update(cat3);
		await _mockCategoryRepo.Received().Update(cat4);
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
		_mockCategoryRepo.GetAll().Returns(Task.FromResult(cats));
		await service.MoveCategoryDown(cat3);
		await _mockCategoryRepo.Received().GetAll();
		await _mockCategoryRepo.Received(4).Update(Arg.Any<Category>());
		await _mockCategoryRepo.Received().Update(cat1);
		await _mockCategoryRepo.Received().Update(cat2);
		await _mockCategoryRepo.Received().Update(cat3);
		await _mockCategoryRepo.Received().Update(cat4);
		Assert.Equal(0, cat1.SortOrder);
		Assert.Equal(2, cat2.SortOrder);
		Assert.Equal(4, cat4.SortOrder);
		Assert.Equal(6, cat3.SortOrder);
	}

	[Fact]
	public async Task MoveUpByIdThrowsIfNotFound()
	{
		var service = GetService();
		_mockCategoryRepo.Get(Arg.Any<int>()).Returns((Category)null);

		await Assert.ThrowsAsync<Exception>(async () => await service.MoveCategoryUp(1));
	}

	[Fact]
	public async Task MoveDownByIdThrowsIfNotFound()
	{
		var service = GetService();
		_mockCategoryRepo.Get(Arg.Any<int>()).Returns((Category)null);

		await Assert.ThrowsAsync<Exception>(async () => await service.MoveCategoryDown(1));
	}
}