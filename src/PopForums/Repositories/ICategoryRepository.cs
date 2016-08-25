using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface ICategoryRepository
	{
		Category Get(int categoryID);
		List<Category> GetAll();
		Category Create(string newTitle, int sortOrder);
		void Delete(int categoryID);
		void Update(Category category);
	}
}