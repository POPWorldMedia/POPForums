using PopForums.Models;
using Xunit;

namespace PopForums.Test.Models
{
	public class CategoryTest
	{
		[Fact]
		public void NewUp()
		{
			const int catID = 123;
			var cat = new Category(catID);
			Assert.Equal(catID, cat.CategoryID);
		}
	}
}
