using System;
using System.Collections.Generic;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Data.Sql.Repositories
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
			_sqlObjectFactory.GetConnection().Using(c => c.Command(_sqlObjectFactory, "SELECT CategoryID, Title, SortOrder FROM pf_Category WHERE CategoryID = @CategoryID")
				.AddParameter(_sqlObjectFactory, "@CategoryID", categoryID)
				.ExecuteReader()
				.ReadOne(r => category = new Category(r.GetInt32(0)) { Title = r.GetString(1), SortOrder = r.GetInt32(2) }));
			return category;
		}

		public List<Category> GetAll()
		{
			var categories = new List<Category>();
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(_sqlObjectFactory, "SELECT CategoryID, Title, SortOrder FROM pf_Category ORDER BY SortOrder")
					.ExecuteReader()
					.ReadAll(r => categories.Add(new Category(r.GetInt32(0)) {Title = r.GetString(1), SortOrder = r.GetInt32(2)})));
			return categories;
		}

		public Category Create(string newTitle, int sortOrder)
		{
			var categoryID = 0;
			_sqlObjectFactory.GetConnection().Using(c => categoryID = Convert.ToInt32(c.Command(_sqlObjectFactory, "INSERT INTO pf_Category (Title, SortOrder) VALUES (@Title, @SortOrder)")
				.AddParameter(_sqlObjectFactory, "@Title", newTitle)
				.AddParameter(_sqlObjectFactory, "@SortOrder", sortOrder)
				.ExecuteAndReturnIdentity()));
			var category = new Category(categoryID) { Title = newTitle, SortOrder = sortOrder };
			return category;
		}

		public void Delete(int categoryID)
		{
			var result = 0;
			_sqlObjectFactory.GetConnection().Using(c => result = c.Command(_sqlObjectFactory, "DELETE FROM pf_Category WHERE CategoryID = @CategoryID")
				.AddParameter(_sqlObjectFactory, "@CategoryID", categoryID)
				.ExecuteNonQuery());
			if (result != 1)
				throw new Exception(String.Format("Can't delete category with ID {0} because it does not exist.", categoryID));
		}

		public void Update(Category category)
		{
			var result = 0;
			_sqlObjectFactory.GetConnection().Using(c => result = c.Command(_sqlObjectFactory, "UPDATE pf_Category SET Title = @Title, SortOrder = @SortOrder WHERE CategoryID = @CategoryID")
				.AddParameter(_sqlObjectFactory, "@Title", category.Title)
				.AddParameter(_sqlObjectFactory, "@SortOrder", category.SortOrder)
				.AddParameter(_sqlObjectFactory, "@CategoryID", category.CategoryID)
				.ExecuteNonQuery());
			if (result != 1)
				throw new Exception(String.Format("Can't update category with ID {0} because it does not exist.", category.CategoryID));
		}
	}
}