namespace PopForums.Repositories;

public interface ICategoryRepository
{
	Task<Category> Get(int categoryID);
	Task<List<Category>> GetAll();
	Task<Category> Create(string newTitle, int sortOrder);
	Task Delete(int categoryID);
	Task Update(Category category);
}