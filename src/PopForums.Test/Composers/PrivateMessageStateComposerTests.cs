using System.Text.Json;
using System.Text.Json.Serialization;
using PopForums.Composers;

namespace PopForums.Test.Composers;

public class PrivateMessageStateComposerTests
{
	protected PrivateMessageStateComposer GetComposer()
	{
		_privateMessageService = Substitute.For<IPrivateMessageService>();
		return new PrivateMessageStateComposer(_privateMessageService);
	}

	private IPrivateMessageService _privateMessageService;

	public class GetState : PrivateMessageStateComposerTests
	{
		[Fact]
		public async Task MessagesMappedWithBuffer()
		{
			var composer = GetComposer();
			var jsonUsers = JsonSerializer.SerializeToElement(new[] {new {UserID = 2, Name = "Jeff"}, new {UserID = 3, Name = "Diana"}, new {UserID = 4, Name = "Simon"}});
			var pm = new PrivateMessage { LastViewDate = DateTime.UtcNow, PMID = 123, Users = jsonUsers};
			var posts = new List<PrivateMessagePost>();
			var post1 = new PrivateMessagePost{ PMID = pm.PMID, UserID = 2, Name = "Jeff", PostTime = new DateTime(2020,1,1), FullText = "post1", PMPostID = 1};
			var post2 = new PrivateMessagePost{ PMID = pm.PMID, UserID = 3, Name = "Diana", PostTime = new DateTime(2021,1,1), FullText = "post2", PMPostID = 2};
			var post3 = new PrivateMessagePost{ PMID = pm.PMID, UserID = 4, Name = "Simon", PostTime = new DateTime(2022,1,1), FullText = "post3", PMPostID = 3};
			posts.Add(post1);
			posts.Add(post2);
			_privateMessageService.GetMostRecentPosts(pm.PMID, pm.LastViewDate).Returns(posts);
			_privateMessageService.GetPosts(pm.PMID, pm.LastViewDate).Returns(new List<PrivateMessagePost> { post3 });
			_privateMessageService.GetUsers(pm.PMID).Returns([
				new PrivateMessageUser { PMID = pm.PMID, UserID = 2 },
				new PrivateMessageUser { PMID = pm.PMID, UserID = 3 },
				new PrivateMessageUser { PMID = pm.PMID, UserID = 4 }
			]);

			var state = await composer.GetState(pm);
			
			Assert.Equal(post3.UserID, state.Messages[0].UserID);
			Assert.Equal(post3.Name, state.Messages[0].Name);
			Assert.Equal(post3.PostTime.ToString("o"), state.Messages[0].PostTime);
			Assert.Equal(post3.FullText, state.Messages[0].FullText);
			Assert.Equal(post3.PMPostID, state.Messages[0].PMPostID);
			
			Assert.Equal(post1.UserID, state.Messages[1].UserID);
			Assert.Equal(post1.Name, state.Messages[1].Name);
			Assert.Equal(post1.PostTime.ToString("o"), state.Messages[1].PostTime);
			Assert.Equal(post1.FullText, state.Messages[1].FullText);
			Assert.Equal(post1.PMPostID, state.Messages[1].PMPostID);
			
			Assert.Equal(post2.UserID, state.Messages[2].UserID);
			Assert.Equal(post2.Name, state.Messages[2].Name);
			Assert.Equal(post2.PostTime.ToString("o"), state.Messages[2].PostTime);
			Assert.Equal(post2.FullText, state.Messages[2].FullText);
			Assert.Equal(post2.PMPostID, state.Messages[2].PMPostID);
		}
		
		[Fact]
		public async Task NewestPostIDSet()
		{
			var composer = GetComposer();
			var jsonUsers = JsonSerializer.SerializeToElement(Array.Empty<object>());
			var pm = new PrivateMessage { LastViewDate = DateTime.UtcNow, PMID = 123, Users = jsonUsers};
			var posts = new List<PrivateMessagePost>();
			var post1 = new PrivateMessagePost{ PMID = pm.PMID };
			var post2 = new PrivateMessagePost{ PMID = pm.PMID };
			var post3 = new PrivateMessagePost{ PMID = pm.PMID };
			posts.Add(post1);
			posts.Add(post2);
			_privateMessageService.GetMostRecentPosts(pm.PMID, pm.LastViewDate).Returns(posts);
			_privateMessageService.GetPosts(pm.PMID, pm.LastViewDate).Returns(new List<PrivateMessagePost> { post3 });
			_privateMessageService.GetUsers(pm.PMID).Returns(new List<PrivateMessageUser>());

			var state = await composer.GetState(pm);
			
			Assert.Equal(post1.PMPostID, state.NewestPostID);
		}
		
		[Fact]
		public async Task PMIDSet()
		{
			var composer = GetComposer();
			var jsonUsers = JsonSerializer.SerializeToElement(Array.Empty<object>());
			var pm = new PrivateMessage { LastViewDate = DateTime.UtcNow, PMID = 123, Users = jsonUsers};
			_privateMessageService.GetMostRecentPosts(pm.PMID, pm.LastViewDate).Returns(new List<PrivateMessagePost>());
			_privateMessageService.GetPosts(pm.PMID, pm.LastViewDate).Returns(new List<PrivateMessagePost>());
			_privateMessageService.GetUsers(pm.PMID).Returns(new List<PrivateMessageUser>());

			var state = await composer.GetState(pm);
			
			Assert.Equal(pm.PMID, state.PmID);
		}
		
		[Fact]
		public async Task PMUsersJsonSet()
		{
			var composer = GetComposer();
			var jsonUsers = JsonSerializer.SerializeToElement(Array.Empty<object>());
			var pm = new PrivateMessage { LastViewDate = DateTime.UtcNow, PMID = 123, Users = jsonUsers};
			_privateMessageService.GetMostRecentPosts(pm.PMID, pm.LastViewDate).Returns(new List<PrivateMessagePost>());
			_privateMessageService.GetPosts(pm.PMID, pm.LastViewDate).Returns(new List<PrivateMessagePost>());
			_privateMessageService.GetUsers(pm.PMID).Returns(new List<PrivateMessageUser>());

			var state = await composer.GetState(pm);
			
			Assert.Equal(pm.Users, state.Users);
		}
		
		[Fact]
		public async Task IsUserNotFoundSetToFalse()
		{
			var composer = GetComposer();
			var jsonUsers = JsonSerializer.SerializeToElement(new[] {new {UserID = 2, Name = "Jeff"}, new {UserID = 3, Name = "Diana"}, new {UserID = 4, Name = "Simon"}});
			var pm = new PrivateMessage { LastViewDate = DateTime.UtcNow, PMID = 123, Users = jsonUsers};
			_privateMessageService.GetMostRecentPosts(pm.PMID, pm.LastViewDate).Returns(new List<PrivateMessagePost>());
			_privateMessageService.GetPosts(pm.PMID, pm.LastViewDate).Returns(new List<PrivateMessagePost>());
			_privateMessageService.GetUsers(pm.PMID).Returns([
				new PrivateMessageUser { PMID = pm.PMID, UserID = 2 },
				new PrivateMessageUser { PMID = pm.PMID, UserID = 3 },
				new PrivateMessageUser { PMID = pm.PMID, UserID = 4 }
			]);

			var state = await composer.GetState(pm);
			
			Assert.False(state.IsUserNotFound);
		}
		
		[Fact]
		public async Task IsUserNotFoundSetToTrue()
		{
			var composer = GetComposer();
			var jsonUsers = JsonSerializer.SerializeToElement(new[] {new {UserID = 2, Name = "Jeff"}, new {UserID = 3, Name = "Diana"}, new {UserID = 4, Name = "Simon"}});
			var pm = new PrivateMessage { LastViewDate = DateTime.UtcNow, PMID = 123, Users = jsonUsers};
			_privateMessageService.GetMostRecentPosts(pm.PMID, pm.LastViewDate).Returns(new List<PrivateMessagePost>());
			_privateMessageService.GetPosts(pm.PMID, pm.LastViewDate).Returns(new List<PrivateMessagePost>());
			_privateMessageService.GetUsers(pm.PMID).Returns([
				new PrivateMessageUser { PMID = pm.PMID, UserID = 2 },
				new PrivateMessageUser { PMID = pm.PMID, UserID = 4 }
			]);

			var state = await composer.GetState(pm);
			
			Assert.True(state.IsUserNotFound);
		}
	}
}