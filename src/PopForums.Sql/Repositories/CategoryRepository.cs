using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class CategoryRepository : ICategoryRepository
	{
		public CategoryRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public async Task<Category> Get(int categoryID)
		{
			Task<Category> category = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				category = connection.QuerySingleOrDefaultAsync<Category>("SELECT CategoryID, Title, SortOrder FROM pf_Category WHERE CategoryID = @CategoryID", new { CategoryID = categoryID }));
			return await category;
		}

		public async Task<List<Category>> GetAll()
		{
			Task<IEnumerable<Category>> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				result = connection.QueryAsync<Category>("SELECT CategoryID, Title, SortOrder FROM pf_Category ORDER BY SortOrder"));
			var list = result.Result.ToList();
			return list;
		}

		public async Task<Category> Create(string newTitle, int sortOrder)
		{
			Task<int> categoryID = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				categoryID = connection.QuerySingleAsync<int>("INSERT INTO pf_Category (Title, SortOrder) VALUES (@Title, @SortOrder);SELECT CAST(SCOPE_IDENTITY() as int)", new { Title = newTitle, SortOrder = sortOrder }));
			var category = new Category { CategoryID = categoryID.Result, Title = newTitle, SortOrder = sortOrder };
			return category;
		}

		public async Task Delete(int categoryID)
		{
			Task<int> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				result = connection.ExecuteAsync("DELETE FROM pf_Category WHERE CategoryID = @CategoryID", new { CategoryID = categoryID }));
			if (result.Result != 1)
				throw new Exception($"Can't delete category with ID {categoryID} because it does not exist.");
		}

		public async Task Update(Category category)
		{
			Task<int> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				result = connection.ExecuteAsync("UPDATE pf_Category SET Title = @Title, SortOrder = @SortOrder WHERE CategoryID = @CategoryID", new { category.Title, category.SortOrder, category.CategoryID }));
			if (result.Result != 1)
				throw new Exception($"Can't update category with ID {category.CategoryID} because it does not exist.");
		}
	}
}