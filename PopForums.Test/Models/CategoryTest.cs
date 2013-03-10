using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PopForums.Models;

namespace PopForums.Test.Models
{
	[TestFixture]
	public class CategoryTest
	{
		[Test]
		public void NewUp()
		{
			const int catID = 123;
			var cat = new Category(catID);
			Assert.AreEqual(catID, cat.CategoryID);
		}
	}
}
