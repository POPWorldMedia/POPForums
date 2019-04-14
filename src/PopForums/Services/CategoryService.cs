using System;
using System.Collections.Generic;
using System.Linq;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface ICategoryService
	{
		Category Get(int categoryID);
		List<Category> GetAll();
		Category Create(string title);
		void Delete(int categoryID);
		void Delete(Category category);
		void UpdateTitle(int categoryID, string newTitle);
		void UpdateTitle(Category category, string newTitle);
		void MoveCategoryUp(int categoryID);
		void MoveCategoryDown(int categoryID);
		void MoveCategoryUp(Category category);
		void MoveCategoryDown(Category category);
	}

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

		public void Delete(int categoryID)
		{
			var category = _categoryRepository.Get(categoryID);
			if (category == null)
				throw new Exception($"Category with ID {categoryID} does not exist.");
			Delete(category);
		}

		public void Delete(Category category)
		{
			var forums = _forumRepository.GetAll().Where(f => f.CategoryID == category.CategoryID);
			foreach (var forum in forums)
				_forumRepository.UpdateCategoryAssociation(forum.ForumID, null);
			_categoryRepository.Delete(category.CategoryID);
		}

		public void UpdateTitle(int categoryID, string newTitle)
		{
			var category = _categoryRepository.Get(categoryID);
			if (category == null)
				throw new Exception($"Category with ID {categoryID} does not exist.");
			UpdateTitle(category, newTitle);
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

		public void MoveCategoryUp(int categoryID)
		{
			var category = _categoryRepository.Get(categoryID);
			if (category == null)
				throw new Exception($"Can't move CategoryID {categoryID} up because it does not exist.");
			MoveCategoryUp(category);
		}

		public void MoveCategoryDown(int categoryID)
		{
			var category = _categoryRepository.Get(categoryID);
			if (category == null)
				throw new Exception($"Can't move CategoryID {categoryID} down because it does not exist.");
			MoveCategoryDown(category);
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
