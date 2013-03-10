using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Services
{
	public interface ICategoryService
	{
		Category Get(int categoryID);
		List<Category> GetAll();
		Category Create(string title);
		void Delete(Category category);
		void UpdateTitle(Category category, string newTitle);
		void MoveCategoryUp(Category category);
		void MoveCategoryDown(Category category);
	}
}