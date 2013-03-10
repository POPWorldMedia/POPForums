using System;
using System.Web.Mvc;
using NUnit.Framework;
using PopForums.Extensions;
using PopForums.Models;

namespace PopForums.Test.Extensions
{
	[TestFixture]
	public class ViewDataDictionaryTests
	{
		[Test]
		public void SetAndGetUserValue()
		{
			var user = new User(123, DateTime.MinValue) {Name = "Name", Email = "Email", IsApproved = true, LastActivityDate = DateTime.MaxValue, LastLoginDate = DateTime.MaxValue, AuthorizationKey = Guid.NewGuid()};
			var viewData = new ViewDataDictionary();
			viewData.SetUserInViewData(user);
			var retrievedUser = viewData[ViewDataDictionaries.ViewDataUserKey];
			Assert.AreSame(user, retrievedUser);
		}
	}
}