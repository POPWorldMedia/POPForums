using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using PopForums.Models;
using PopForums.Web;

namespace PopForums.Test.Web
{
	[TestFixture]
	public class AdminAttributeTest
	{
		private class TestableAdmin : AdminAttribute
		{
			public bool GetAuthorizeCoreResult(HttpContextBase context)
			{
				return AuthorizeCore(context);
			}

			public void DoHandleUnauthorizedRequest(AuthorizationContext filterContext)
			{
				HandleUnauthorizedRequest(filterContext);
			}
		}

		[Test]
		public void AdminRoleReturnsTrue()
		{
			var admin = new TestableAdmin();
			var contextMock = new Mock<HttpContextBase>();
			var principalMock = new Mock<IPrincipal>();
			principalMock.Setup(p => p.IsInRole(PermanentRoles.Admin)).Returns(true);
			contextMock.Setup(c => c.User).Returns(principalMock.Object);
			Assert.IsTrue(admin.GetAuthorizeCoreResult(contextMock.Object));
		}

		[Test]
		public void NonAdminRoleReturnsFalse()
		{
			var admin = new TestableAdmin();
			var contextMock = new Mock<HttpContextBase>();
			var principalMock = new Mock<IPrincipal>();
			principalMock.Setup(p => p.IsInRole(PermanentRoles.Admin)).Returns(false);
			contextMock.Setup(c => c.User).Returns(principalMock.Object);
			Assert.IsFalse(admin.GetAuthorizeCoreResult(contextMock.Object));
		}

		[Test]
		public void NoUsereReturnsFalse()
		{
			var admin = new TestableAdmin();
			var contextMock = new Mock<HttpContextBase>();
			contextMock.Setup(c => c.User).Returns((IPrincipal)null);
			Assert.IsFalse(admin.GetAuthorizeCoreResult(contextMock.Object));
		}

		[Test]
		public void UnauthHandlerReturnsForbiddenViewResult()
		{
			var admin = new TestableAdmin();
			var context = new AuthorizationContext();
			admin.DoHandleUnauthorizedRequest(context);
			Assert.IsInstanceOf<ViewResult>(context.Result);
			Assert.AreEqual("Forbidden", ((ViewResult) context.Result).ViewName);
		}
	}
}
