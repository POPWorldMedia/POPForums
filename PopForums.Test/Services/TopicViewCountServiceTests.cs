using System.Web;
using Moq;
using NUnit.Framework;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;
using PopForums.Test.Controllers;

namespace PopForums.Test.Services
{
	[TestFixture]
	public class TopicViewCountServiceTests
	{
		private HttpContextHelper GetHelper()
		{
			var helper = new HttpContextHelper();
			helper.MockRequest.Setup(r => r.Cookies).Returns(new HttpCookieCollection());
			helper.MockResponse.Setup(r => r.Cookies).Returns(new HttpCookieCollection());
			return helper;
		}

		[Test]
		public void CookieIsSet()
		{
			var helper = GetHelper();
			var mockTopicRepo = new Mock<ITopicRepository>();
			var topicViewCountService = new TopicViewCountService(mockTopicRepo.Object);
			topicViewCountService.ProcessView(new Topic(123), helper.MockContext.Object);
			Assert.AreEqual(1, helper.MockResponse.Object.Cookies.Count);
			var cookie = helper.MockResponse.Object.Cookies[0];
			Assert.AreEqual("123", cookie.Value);
			Assert.AreEqual("PopForums.LastTopicID", cookie.Name);
		}

		[Test]
		public void NoCookieCallRepoForView()
		{
			var helper = GetHelper();
			var mockTopicRepo = new Mock<ITopicRepository>();
			var topicViewCountService = new TopicViewCountService(mockTopicRepo.Object);
			topicViewCountService.ProcessView(new Topic(123), helper.MockContext.Object);
			mockTopicRepo.Verify(t => t.IncrementViewCount(123), Times.Once());
		}

		[Test]
		public void CookiePresentAndMatchDontCallRepoForView()
		{
			var helper = GetHelper();
			helper.MockRequest.Object.Cookies.Set(new HttpCookie("PopForums.LastTopicID") { Value = "123" });
			var mockTopicRepo = new Mock<ITopicRepository>();
			var topicViewCountService = new TopicViewCountService(mockTopicRepo.Object);
			topicViewCountService.ProcessView(new Topic(123), helper.MockContext.Object);
			mockTopicRepo.Verify(t => t.IncrementViewCount(123), Times.Never());
		}

		[Test]
		public void CookiePresentNoMatchCallRepoForView()
		{
			var helper = GetHelper();
			helper.MockRequest.Object.Cookies.Set(new HttpCookie("PopForums.LastTopicID") { Value = "456" });
			var mockTopicRepo = new Mock<ITopicRepository>();
			var topicViewCountService = new TopicViewCountService(mockTopicRepo.Object);
			topicViewCountService.ProcessView(new Topic(123), helper.MockContext.Object);
			mockTopicRepo.Verify(t => t.IncrementViewCount(123), Times.Once());
		}
	}
}
