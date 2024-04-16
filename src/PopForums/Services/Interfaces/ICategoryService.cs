namespace PopForums.Services.Interfaces;

public interface ICategoryService
{
    Task<Category> Get(int categoryID);

    Task<List<Category>> GetAll();

    Task<Category> Create(string title);

    Task Delete(int categoryID);

    Task Delete(Category category);

    Task UpdateTitle(int categoryID, string newTitle);

    Task UpdateTitle(Category category, string newTitle);

    Task MoveCategoryUp(int categoryID);

    Task MoveCategoryDown(int categoryID);

    Task MoveCategoryUp(Category category);

    Task MoveCategoryDown(Category category);
}
