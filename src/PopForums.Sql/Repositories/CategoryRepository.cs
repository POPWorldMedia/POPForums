using System;
using System.Collections.Generic;
using System.Linq;
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

		public Category Get(int categoryID)
		{
			Category category = null;
			_sqlObjectFactory.GetConnection().Using(connection => 
				category = connection.QuerySingleOrDefault<Category>("SELECT CategoryID, Title, SortOrder FROM pf_Category WHERE CategoryID = @CategoryID", new { CategoryID = categoryID }));
			return category;
		}

		public List<Category> GetAll()
		{
			var categories = new List<Category>();
			_sqlObjectFactory.GetConnection().Using(connection => 
				categories = connection.Query<Category>("SELECT CategoryID, Title, SortOrder FROM pf_Category ORDER BY SortOrder").ToList());
			return categories;
		}

		public Category Create(string newTitle, int sortOrder)
		{
			var categoryID = 0;
			_sqlObjectFactory.GetConnection().Using(connection => 
				categoryID = connection.QuerySingle<int>("INSERT INTO pf_Category (Title, SortOrder) VALUES (@Title, @SortOrder);SELECT CAST(SCOPE_IDENTITY() as int)", new { Title = newTitle, SortOrder = sortOrder }));
			var category = new Category { CategoryID = categoryID, Title = newTitle, SortOrder = sortOrder };
			return category;
		}

		public void Delete(int categoryID)
		{
			var result = 0;
			_sqlObjectFactory.GetConnection().Using(connection => 
				result = connection.Execute("DELETE FROM pf_Category WHERE CategoryID = @CategoryID", new { CategoryID = categoryID }));
			if (result != 1)
				throw new Exception($"Can't delete category with ID {categoryID} because it does not exist.");
		}

		public void Update(Category category)
		{
			var result = 0;
			_sqlObjectFactory.GetConnection().Using(connection => 
				result = connection.Execute("UPDATE pf_Category SET Title = @Title, SortOrder = @SortOrder WHERE CategoryID = @CategoryID", new { category.Title, category.SortOrder, category.CategoryID }));
			if (result != 1)
				throw new Exception(String.Format("Can't update category with ID {0} because it does not exist.", category.CategoryID));
		}
	}
}