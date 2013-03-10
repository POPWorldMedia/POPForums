using System.Collections.Generic;
using System.Linq;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public class CategoryService : ICategoryService
	{
		public CategoryService(ICategoryRepository categoryRepository, IForumRepository forumRepository)
		{
			_categoryRepository = categoryRepository;
			_forumRepository = forumRepository;
		}

		private readonly ICategoryRepository _categoryRepository;
		private readonly IForumRepository _forumRepository;

		public Category Get(int categoryID)
		{
			return _categoryRepository.Get(categoryID);
		}

		public List<Category> GetAll()
		{
			return _categoryRepository.GetAll();
		}

		public Category Create(string title)
		{
			var newCategory = _categoryRepository.Create(title, -2);
			ChangeCategorySortOrder(null, 0);
			newCategory.SortOrder = 0;
			return newCategory;
		}

		public void Delete(Category category)
		{
			var forums = _forumRepository.GetAll().Where(f => f.CategoryID == category.CategoryID);
			foreach (var forum in forums)
				_forumRepository.UpdateCategoryAssociation(forum.ForumID, null);
			_categoryRepository.Delete(category.CategoryID);
		}

		public void UpdateTitle(Category category, string newTitle)
		{
			category.Title = newTitle;
			_categoryRepository.Update(category);
		}

		private void ChangeCategorySortOrder(Category category, int change)
		{
			var categories = GetAll();
			if (category != null)
				categories.Single(c => c.CategoryID == category.CategoryID).SortOrder += change;
			var sorted = categories.OrderBy(c => c.SortOrder).ToList();
			for (var i = 0; i < sorted.Count; i++)
			{
				sorted[i].SortOrder = i * 2;
				_categoryRepository.Update(sorted[i]);
			}
		}

		public void MoveCategoryUp(Category category)
		{
			const int change = -3;
			ChangeCategorySortOrder(category, change);
		}

		public void MoveCategoryDown(Category category)
		{
			const int change = 3;
			ChangeCategorySortOrder(category, change);
		}
	}
}
