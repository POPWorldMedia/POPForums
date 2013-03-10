using System.Web.Routing;
using Moq;
using NUnit.Framework;
using PopForums.Repositories;
using PopForums.Web;

namespace PopForums.Test.Configuration
{
	[TestFixture]
	public class ForumRouteConstraintTest
	{
		[Test]
		public void TrueMatch()
		{
			var constraint = GetConstraint();
			var routeValues = new RouteValueDictionary(new { urlName = "forum-2" });
			var result = constraint.Match(null, null, null, routeValues, RouteDirection.IncomingRequest);
			Assert.IsTrue(result);
		}

		[Test]
		public void FalseMatch()
		{
			var constraint = GetConstraint();
			var routeValues = new RouteValueDictionary(new { urlName = "forum" });
			var result = constraint.Match(null, null, null, routeValues, RouteDirection.IncomingRequest);
			Assert.IsFalse(result);
		}

		private ForumRouteConstraint GetConstraint()
		{
			var mockForumRepo = new Mock<IForumRepository>();
			mockForumRepo.Setup(f => f.GetAllForumUrlNames()).Returns(new[] {"forum-1", "forum-2", "forum-3"});
			return new ForumRouteConstraint(mockForumRepo.Object);
		}
	}
}
