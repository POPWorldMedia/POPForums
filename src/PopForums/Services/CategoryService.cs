using PopForums.Services.Interfaces;

namespace PopForums.Services;

public class CategoryService : ICategoryService
{
	private readonly ICategoryRepository _categoryRepository;
	private readonly IForumRepository _forumRepository;

	public CategoryService(ICategoryRepository categoryRepository, IForumRepository forumRepository)
	{
		_categoryRepository = categoryRepository;
		_forumRepository = forumRepository;
	}

	public async Task<Category> Get(int categoryID)
	{
		return await _categoryRepository.Get(categoryID);
	}

	public async Task<List<Category>> GetAll()
	{
		return await _categoryRepository.GetAll();
	}

	public async Task<Category> Create(string title)
	{
		var newCategory = await _categoryRepository.Create(title, -2);

		await ChangeCategorySortOrder(null, 0);

		newCategory.SortOrder = 0;

		return newCategory;
	}

	public async Task Delete(int categoryID)
	{
		var category = await _categoryRepository.Get(categoryID);

		if (category == null)
		{
			throw new Exception($"Category with ID {categoryID} does not exist.");
		}
			
		await Delete(category);
	}

	public async Task Delete(Category category)
	{
		var forumResult = await _forumRepository.GetAll();
		var forums = forumResult.Where(f => f.CategoryID == category.CategoryID);

		foreach (var forum in forums)
		{
			await _forumRepository.UpdateCategoryAssociation(forum.ForumID, null);
		}

		await _categoryRepository.Delete(category.CategoryID);
	}

	public async Task UpdateTitle(int categoryID, string newTitle)
	{
		var category = await _categoryRepository.Get(categoryID);

		if (category == null)
		{
			throw new Exception($"Category with ID {categoryID} does not exist.");
		}

		await UpdateTitle(category, newTitle);
	}

	public async Task UpdateTitle(Category category, string newTitle)
	{
		category.Title = newTitle;
		await _categoryRepository.Update(category);
	}

	public async Task MoveCategoryUp(int categoryID)
	{
		var category = await _categoryRepository.Get(categoryID);

		if (category == null)
		{
			throw new Exception($"Can't move CategoryID {categoryID} up because it does not exist.");
		}

		await MoveCategoryUp(category);
	}

	public async Task MoveCategoryDown(int categoryID)
	{
		var category = await _categoryRepository.Get(categoryID);

		if (category == null)
		{
			throw new Exception($"Can't move CategoryID {categoryID} down because it does not exist.");
		}

		await MoveCategoryDown(category);
	}

	public async Task MoveCategoryUp(Category category)
	{
		const int change = -3;
		await ChangeCategorySortOrder(category, change);
	}

	public async Task MoveCategoryDown(Category category)
	{
		const int change = 3;
		await ChangeCategorySortOrder(category, change);
	}

	private async Task ChangeCategorySortOrder(Category category, int change)
	{
		var categories = await GetAll();

		if (category != null)
		{
			categories.Single(c => c.CategoryID == category.CategoryID).SortOrder += change;
		}

		var sorted = categories.OrderBy(c => c.SortOrder).ToList();

		for (var i = 0; i < sorted.Count; i++)
		{
			sorted[i].SortOrder = i * 2;
			await _categoryRepository.Update(sorted[i]);
		}
	}
}