using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PopForums.Configuration;
using PopForums.Feeds;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;
using PopForums.Services;

namespace PopForums.Test.Services
{
	[TestFixture]
	public class PostServiceTests
	{
		private Mock<IPostRepository> _postRepo;
		private Mock<IProfileRepository> _profileRepo;
		private Mock<ISettingsManager> _settingsManager;
		private Mock<Settings> _settings;
		private Mock<ITopicService> _topicService;
		private Mock<ITextParsingService> _textParsingService;
		private Mock<IModerationLogService> _modLogService;
		private Mock<IForumService> _forumService;
		private Mock<IEventPublisher> _eventPub;
		private Mock<IUserService> _userService;
		private Mock<IFeedService> _feedService;

		private PostService GetService()
		{
			_postRepo = new Mock<IPostRepository>();
			_profileRepo = new Mock<IProfileRepository>();
			_settingsManager = new Mock<ISettingsManager>();
			_settings = new Mock<Settings>();
			_topicService = new Mock<ITopicService>();
			_textParsingService = new Mock<ITextParsingService>();
			_modLogService = new Mock<IModerationLogService>();
			_forumService = new Mock<IForumService>();
			_eventPub = new Mock<IEventPublisher>();
			_userService = new Mock<IUserService>();
			_feedService = new Mock<IFeedService>();
			_settingsManager.Setup(s => s.Current).Returns(_settings.Object);
			return new PostService(_postRepo.Object, _profileRepo.Object, _settingsManager.Object, _topicService.Object, _textParsingService.Object, _modLogService.Object, _forumService.Object, _eventPub.Object, _userService.Object, _feedService.Object);
		}

		[Test]
		public void GetPostsPageSizeAndStartRowCalcdCorrectly()
		{
			var topic = new Topic(1) {ReplyCount = 20};
			var postService = GetService();
			_settings.Setup(s => s.PostsPerPage).Returns(2);
			PagerContext pagerContext;
			postService.GetPosts(topic, false, 4, out pagerContext);
			_postRepo.Verify(p => p.Get(1, false, 7, 2), Times.Once());
			_postRepo.Verify(p => p.GetReplyCount(It.IsAny<int>(), It.IsAny<bool>()), Times.Never());
			Assert.AreEqual(11, pagerContext.PageCount);
			Assert.AreEqual(2, pagerContext.PageSize);
		}

		[Test]
		public void GetPostsReplyCountCalledOnIncludeDeleted()
		{
			var topic = new Topic(1) { ReplyCount = 20 };
			var postService = GetService();
			_settings.Setup(s => s.PostsPerPage).Returns(2);
			_postRepo.Setup(p => p.GetReplyCount(topic.TopicID, true)).Returns(21);
			PagerContext pagerContext;
			postService.GetPosts(topic, true, 4, out pagerContext);
			_postRepo.Verify(p => p.GetReplyCount(topic.TopicID, true), Times.Once());
			Assert.AreEqual(11, pagerContext.PageCount);
		}

		[Test]
		public void GetPostsPagerContextConstructed()
		{
			var topic = new Topic(1) { ReplyCount = 20 };
			var postService = GetService();
			_settings.Setup(s => s.PostsPerPage).Returns(3);
			PagerContext pagerContext;
			postService.GetPosts(topic, false, 4, out pagerContext);
			Assert.AreEqual(7, pagerContext.PageCount);
			Assert.AreEqual(4, pagerContext.PageIndex);
		}

		[Test]
		public void GetPostsHitsRepo()
		{
			var topic = new Topic(1) { ReplyCount = 20 };
			var posts = new List<Post>();
			var postService = GetService();
			_settings.Setup(s => s.PostsPerPage).Returns(3);
			_postRepo.Setup(p => p.Get(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(posts);
			PagerContext pagerContext;
			var result = postService.GetPosts(topic, false, 4, out pagerContext);
			Assert.AreSame(posts, result);
		}

		[Test]
		public void GetCallsRepoAndReturns()
		{
			var postService = GetService();
			var postID = 123;
			var post = new Post(postID);
			_postRepo.Setup(p => p.Get(postID)).Returns(post);
			var postResult = postService.Get(postID);
			_postRepo.Verify(p => p.Get(postID), Times.Once());
			Assert.AreSame(postResult, post);
		}

		[Test]
		public void DupeAndTimeCheckNoPreviousPost()
		{
			var postService = GetService();
			var user = UserServiceTests.GetDummyUser("Jeff", "jeff@popw.com");
			_profileRepo.Setup(p => p.GetLastPostID(user.UserID)).Returns((int?)null);
			Assert.IsFalse(postService.IsNewPostDupeOrInTimeLimit(new NewPost(), user));
		}

		[Test]
		public void DupeAndTimeCheckIsInTimeLimit()
		{
			var postService = GetService();
			var user = UserServiceTests.GetDummyUser("Jeff", "jeff@popw.com");
			_profileRepo.Setup(p => p.GetLastPostID(user.UserID)).Returns(123);
			_postRepo.Setup(p => p.Get(123)).Returns(new Post(456) {PostTime = DateTime.UtcNow.AddSeconds(-15)});
			_settings.Setup(s => s.MinimumSecondsBetweenPosts).Returns(20);
			Assert.IsTrue(postService.IsNewPostDupeOrInTimeLimit(new NewPost(), user));
		}

		[Test]
		public void DupeAndTimeCheckIsDupeTextPlainText()
		{
			var postService = GetService();
			var user = UserServiceTests.GetDummyUser("Jeff", "jeff@popw.com");
			const string dupeText = "whatever";
			_profileRepo.Setup(p => p.GetLastPostID(user.UserID)).Returns(123);
			_postRepo.Setup(p => p.Get(123)).Returns(new Post(456) { FullText = dupeText });
			_textParsingService.Setup(x => x.ForumCodeToHtml(dupeText)).Returns(dupeText);
			Assert.IsTrue(postService.IsNewPostDupeOrInTimeLimit(new NewPost { FullText = dupeText, IsPlainText = true}, user));
		}

		[Test]
		public void DupeAndTimeCheckIsDupeTextRichText()
		{
			var postService = GetService();
			var user = UserServiceTests.GetDummyUser("Jeff", "jeff@popw.com");
			const string dupeText = "whatever";
			_profileRepo.Setup(p => p.GetLastPostID(user.UserID)).Returns(123);
			_postRepo.Setup(p => p.Get(123)).Returns(new Post(456) { FullText = dupeText });
			_textParsingService.Setup(x => x.ClientHtmlToHtml(dupeText)).Returns(dupeText);
			Assert.IsTrue(postService.IsNewPostDupeOrInTimeLimit(new NewPost { FullText = dupeText, IsPlainText = false }, user));
		}

		[Test]
		public void DupeAndTimeCheckIsNotInTimeLimitOrDupeText()
		{
			var postService = GetService();
			var user = UserServiceTests.GetDummyUser("Jeff", "jeff@popw.com");
			_profileRepo.Setup(p => p.GetLastPostID(user.UserID)).Returns(123);
			_postRepo.Setup(p => p.Get(123)).Returns(new Post(456) { FullText = "one thing", PostTime = DateTime.UtcNow.AddSeconds(-30) });
			_settings.Setup(s => s.MinimumSecondsBetweenPosts).Returns(20);
			Assert.IsFalse(postService.IsNewPostDupeOrInTimeLimit(new NewPost { FullText = "another" }, user));
		}

		[Test]
		public void GetPostCountCallsRepo()
		{
			var postService = GetService();
			_postRepo.Setup(p => p.GetPostCount(123)).Returns(456);
			var user = new User(123, DateTime.MinValue);
			var result = postService.GetPostCount(user);
			_postRepo.Verify(p => p.GetPostCount(123), Times.Exactly(1));
			Assert.AreEqual(456, result);
		}

		[Test]
		public void GetPostForEditPlainText()
		{
			var service = GetService();
			var post = new Post(123) { Title = "mah title", FullText = "not", ShowSig = true };
			var user = new User(456, DateTime.MinValue);
			_profileRepo.Setup(p => p.GetProfile(user.UserID)).Returns(new Profile {IsPlainText = true});
			_textParsingService.Setup(p => p.HtmlToForumCode("not")).Returns("new text");
			var postEdit = service.GetPostForEdit(post, user, false);
			Assert.AreEqual("mah title", postEdit.Title);
			Assert.AreEqual("new text", postEdit.FullText);
			Assert.True(postEdit.ShowSig);
			Assert.True(postEdit.IsPlainText);
			_textParsingService.Verify(t => t.HtmlToForumCode("not"), Times.Exactly(1));
		}

		[Test]
		public void GetPostForEditPlainTextMobile()
		{
			var service = GetService();
			var post = new Post(123) { Title = "mah title", FullText = "not", ShowSig = true };
			var user = new User(456, DateTime.MinValue);
			_profileRepo.Setup(p => p.GetProfile(user.UserID)).Returns(new Profile { IsPlainText = false });
			_textParsingService.Setup(p => p.HtmlToForumCode("not")).Returns("new text");
			var postEdit = service.GetPostForEdit(post, user, true);
			Assert.AreEqual("mah title", postEdit.Title);
			Assert.AreEqual("new text", postEdit.FullText);
			Assert.True(postEdit.ShowSig);
			Assert.True(postEdit.IsPlainText);
			_textParsingService.Verify(t => t.HtmlToForumCode("not"), Times.Exactly(1));
		}

		[Test]
		public void GetPostForEditNotPlainText()
		{
			var service = GetService();
			var post = new Post(123) { Title = "mah title", FullText = "not", ShowSig = true };
			var user = new User(456, DateTime.MinValue);
			_profileRepo.Setup(p => p.GetProfile(user.UserID)).Returns(new Profile { IsPlainText = false });
			_textParsingService.Setup(p => p.HtmlToClientHtml("not")).Returns("new text");
			var postEdit = service.GetPostForEdit(post, user, false);
			Assert.AreEqual("mah title", postEdit.Title);
			Assert.AreEqual("new text", postEdit.FullText);
			Assert.True(postEdit.ShowSig);
			Assert.False(postEdit.IsPlainText);
			_textParsingService.Verify(t => t.HtmlToClientHtml("not"), Times.Exactly(1));
		}

		[Test]
		public void EditPostCensorsTitle()
		{
			var service = GetService();
			service.EditPost(new Post(456), new PostEdit{ Title = "blah" }, new User(123, DateTime.MinValue));
			_textParsingService.Verify(t => t.EscapeHtmlAndCensor("blah"), Times.Exactly(1));
		}

		[Test]
		public void EditPostPlainTextParsed()
		{
			var service = GetService();
			service.EditPost(new Post(456), new PostEdit { FullText = "blah", IsPlainText = true }, new User(123, DateTime.MinValue));
			_textParsingService.Verify(t => t.ForumCodeToHtml("blah"), Times.Exactly(1));
		}

		[Test]
		public void EditPostRichTextParsed()
		{
			var service = GetService();
			service.EditPost(new Post(456), new PostEdit { FullText = "blah", IsPlainText = false }, new User(123, DateTime.MinValue));
			_textParsingService.Verify(t => t.ClientHtmlToHtml("blah"), Times.Exactly(1));
		}

		[Test]
		public void EditPostSavesMappedValues()
		{
			var service = GetService();
			var post = new Post(67);
			_postRepo.Setup(p => p.Update(It.IsAny<Post>())).Callback<Post>(p => post = p);
			_textParsingService.Setup(t => t.ClientHtmlToHtml("blah")).Returns("new");
			_textParsingService.Setup(t => t.EscapeHtmlAndCensor("unparsed title")).Returns("new title");
			service.EditPost(new Post(456) { ShowSig = false }, new PostEdit { FullText = "blah", Title = "unparsed title", IsPlainText = false, ShowSig = true }, new User(123, DateTime.MinValue) { Name = "dude" });
			Assert.AreNotEqual(post.LastEditTime, new DateTime(2009, 1, 1));
			Assert.AreEqual(post.PostID, 456);
			Assert.AreEqual("new", post.FullText);
			Assert.AreEqual("new title", post.Title);
			Assert.True(post.ShowSig);
			Assert.True(post.IsEdited);
			Assert.AreEqual("dude", post.LastEditName);
		}

		[Test]
		public void EditPostModeratorLogged()
		{
			var service = GetService();
			var user = new User(123, DateTime.MinValue) {Name = "dude"};
			_textParsingService.Setup(t => t.ClientHtmlToHtml("blah")).Returns("new");
			_textParsingService.Setup(t => t.EscapeHtmlAndCensor("unparsed title")).Returns("new title");
			service.EditPost(new Post(456) { ShowSig = false, FullText = "old text" }, new PostEdit { FullText = "blah", Title = "unparsed title", IsPlainText = false, ShowSig = true, Comment = "mah comment" }, user);
			_modLogService.Verify(m => m.LogPost(user, ModerationType.PostEdit, It.IsAny<Post>(), "mah comment", "old text"), Times.Exactly(1));
		}

		[Test]
		public void DeleteThrowsForNonAuthorAndNonMod()
		{
			var service = GetService();
			var user = new User(123, DateTime.MinValue);
			var post = new Post(67);
			Assert.Throws<Exception>(() => service.Delete(post, user));
		}

		[Test]
		public void DeleteCallDeleteTopicIfFirstInTopic()
		{
			var forum = new Forum(5);
			var topic = new Topic(4) { ForumID = forum.ForumID };
			var service = GetService();
			var user = new User(123, DateTime.MinValue);
			var post = new Post(67) { UserID = user.UserID, IsFirstInTopic = true, TopicID = topic.TopicID };
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).Returns(forum);
			service.Delete(post, user);
			_topicService.Verify(t => t.DeleteTopic(topic, user), Times.Exactly(1));
		}

		[Test]
		public void DeleteCallLogs()
		{
			var forum = new Forum(5);
			var topic = new Topic(4) { ForumID = forum.ForumID };
			var service = GetService();
			var user = new User(123, DateTime.MinValue);
			var post = new Post(67) { UserID = user.UserID, IsFirstInTopic = false, TopicID = topic.TopicID };
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).Returns(forum);
			service.Delete(post, user);
			_modLogService.Verify(m => m.LogPost(user, ModerationType.PostDelete, post, String.Empty, String.Empty), Times.Exactly(1));
		}

		[Test]
		public void DeleteSetsEditFields()
		{
			var forum = new Forum(5);
			var topic = new Topic(4) { ForumID = forum.ForumID };
			var service = GetService();
			var user = new User(123, DateTime.MinValue);
			var post = new Post(67) { UserID = user.UserID, IsFirstInTopic = false, TopicID = topic.TopicID, IsEdited = false };
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).Returns(forum);
			var editedPost = new Post(0);
			_postRepo.Setup(p => p.Update(It.IsAny<Post>())).Callback<Post>(x => editedPost = x);
			service.Delete(post, user);
			Assert.IsTrue(editedPost.IsEdited);
			Assert.AreEqual(user.Name, editedPost.LastEditName);
			Assert.IsTrue(editedPost.LastEditTime.HasValue);
		}

		[Test]
		public void DeleteCallSetsIsDeletedAndUpdates()
		{
			var forum = new Forum(5);
			var topic = new Topic(4) { ForumID = forum.ForumID };
			var service = GetService();
			var user = new User(123, DateTime.MinValue);
			var post = new Post(67) { UserID = user.UserID, IsFirstInTopic = false, TopicID = topic.TopicID, IsDeleted = false };
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).Returns(forum);
			var persistedPost = new Post(0);
			_postRepo.Setup(p => p.Update(It.IsAny<Post>())).Callback<Post>(p => persistedPost = p);
			service.Delete(post, user);
			Assert.AreEqual(post.PostID, persistedPost.PostID);
			Assert.True(persistedPost.IsDeleted);
		}

		[Test]
		public void DeleteCallFiresRecalcs()
		{
			var forum = new Forum(5);
			var topic = new Topic(4) { ForumID = forum.ForumID };
			var service = GetService();
			var user = new User(123, DateTime.MinValue);
			var post = new Post(67) { UserID = user.UserID, IsFirstInTopic = false, TopicID = topic.TopicID };
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).Returns(forum);
			service.Delete(post, user);
			_topicService.Verify(t => t.RecalculateReplyCount(topic), Times.Exactly(1));
			_topicService.Verify(t => t.UpdateLast(topic), Times.Once());
			_forumService.Verify(f => f.UpdateCounts(forum), Times.Exactly(1));
			_forumService.Verify(f => f.UpdateLast(forum), Times.Exactly(1));
		}

		[Test]
		public void UndeleteThrowsForNonMod()
		{
			var service = GetService();
			var user = new User(123, DateTime.MinValue);
			var post = new Post(67) { UserID = user.UserID };
			Assert.Throws<Exception>(() => service.Undelete(post, user));
		}

		[Test]
		public void UndeleteCallLogs()
		{
			var forum = new Forum(5);
			var topic = new Topic(4) { ForumID = forum.ForumID };
			var service = GetService();
			var user = new User(123, DateTime.MinValue) { Roles = new List<string> { PermanentRoles.Moderator }};
			var post = new Post(67) { TopicID = topic.TopicID };
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).Returns(forum);
			service.Undelete(post, user);
			_modLogService.Verify(m => m.LogPost(user, ModerationType.PostUndelete, post, String.Empty, String.Empty), Times.Exactly(1));
		}

		[Test]
		public void UndeleteSetsEditFields()
		{
			var forum = new Forum(5);
			var topic = new Topic(4) { ForumID = forum.ForumID };
			var service = GetService();
			var user = new User(123, DateTime.MinValue) { Roles = new List<string> { PermanentRoles.Moderator } };
			var post = new Post(67) { TopicID = topic.TopicID, IsEdited = false, UserID = user.UserID };
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).Returns(forum);
			var editedPost = new Post(0);
			_postRepo.Setup(p => p.Update(It.IsAny<Post>())).Callback<Post>(x => editedPost = x);
			service.Undelete(post, user);
			Assert.IsTrue(editedPost.IsEdited);
			Assert.AreEqual(user.Name, editedPost.LastEditName);
			Assert.IsTrue(editedPost.LastEditTime.HasValue);
		}

		[Test]
		public void UndeleteCallSetsIsDeletedAndUpdates()
		{
			var forum = new Forum(5);
			var topic = new Topic(4) { ForumID = forum.ForumID };
			var service = GetService();
			var user = new User(123, DateTime.MinValue) { Roles = new List<string> { PermanentRoles.Moderator } };
			var post = new Post(67) { TopicID = topic.TopicID, IsDeleted = true };
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).Returns(forum);
			var persistedPost = new Post(0);
			_postRepo.Setup(p => p.Update(It.IsAny<Post>())).Callback<Post>(p => persistedPost = p);
			service.Undelete(post, user);
			Assert.AreEqual(post.PostID, persistedPost.PostID);
			Assert.False(persistedPost.IsDeleted);
		}

		[Test]
		public void UndeleteCallFiresRecalcs()
		{
			var forum = new Forum(5);
			var topic = new Topic(4) { ForumID = forum.ForumID };
			var service = GetService();
			var user = new User(123, DateTime.MinValue) { Roles = new List<string> { PermanentRoles.Moderator } };
			var post = new Post(67) { TopicID = topic.TopicID };
			_topicService.Setup(t => t.Get(topic.TopicID)).Returns(topic);
			_forumService.Setup(f => f.Get(forum.ForumID)).Returns(forum);
			service.Undelete(post, user);
			_topicService.Verify(t => t.RecalculateReplyCount(topic), Times.Exactly(1));
			_topicService.Verify(t => t.UpdateLast(topic), Times.Once());
			_forumService.Verify(f => f.UpdateCounts(forum), Times.Exactly(1));
			_forumService.Verify(f => f.UpdateLast(forum), Times.Exactly(1));
		}

		[Test]
		public void GetFirstInTopicThrowsWithNull()
		{
			var service = GetService();
			_postRepo.Setup(p => p.GetFirstInTopic(It.IsAny<int>())).Returns((Post) null);
			Assert.Throws<Exception>(() => service.GetFirstInTopic(new Topic(1)));
		}

		[Test]
		public void GetFirstInTopicReturnsFromRepo()
		{
			var service = GetService();
			var post = new Post(123);
			_postRepo.Setup(p => p.GetFirstInTopic(It.IsAny<int>())).Returns(post);
			var result = service.GetFirstInTopic(new Topic(2));
			Assert.AreSame(post, result);
			_postRepo.Verify(p => p.GetFirstInTopic(It.IsAny<int>()), Times.Exactly(1));
		}

		[Test]
		public void GetPostsToEndSkipsLoadedPosts()
		{
			var service = GetService();
			var posts = new List<Post> {new Post(1), new Post(2), new Post(3), new Post(4)};
			_postRepo.Setup(p => p.Get(It.IsAny<int>(), It.IsAny<bool>())).Returns(posts);
			_settingsManager.Setup(s => s.Current.PostsPerPage).Returns(5);
			PagerContext pagerContext;
			var result = service.GetPosts(new Topic(123), 2, true, out pagerContext);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(3, result[0].PostID);
			Assert.AreEqual(4, result[1].PostID);
		}

		[Test]
		public void GetPostsToEndCalcsCorrectPagerContext()
		{
			var service = GetService();
			var posts = new List<Post> { new Post(1), new Post(2), new Post(3), new Post(4), new Post(5), new Post(6), new Post(7), new Post(8) };
			_postRepo.Setup(p => p.Get(It.IsAny<int>(), It.IsAny<bool>())).Returns(posts);
			_settingsManager.Setup(s => s.Current.PostsPerPage).Returns(3);
			PagerContext pagerContext;
			service.GetPosts(new Topic(123), 2, true, out pagerContext);
			Assert.AreEqual(3, pagerContext.PageCount);
			Assert.AreEqual(3, pagerContext.PageIndex);
		}

		[Test]
		public void VotePostCallsRepoWithPostIDAndUserID()
		{
			var service = GetService();
			var post = new Post(1);
			var user = new User(2, new DateTime());
			_postRepo.Setup(x => x.GetVotes(post.PostID)).Returns(new Dictionary<int, string>());
			service.VotePost(post, user, "abc", "def", "ghi");
			_postRepo.Verify(x => x.VotePost(post.PostID, user.UserID), Times.Once());
		}

		[Test]
		public void VotePostCalcsAndSetsCount()
		{
			var service = GetService();
			var post = new Post(1);
			var user = new User(2, new DateTime());
			const int votes = 32;
			_postRepo.Setup(x => x.GetVotes(post.PostID)).Returns(new Dictionary<int, string>());
			_postRepo.Setup(x => x.CalculateVoteCount(post.PostID)).Returns(votes);
			service.VotePost(post, user, "abc", "def", "ghi");
			_postRepo.Verify(x => x.SetVoteCount(post.PostID, votes), Times.Once());
		}

		[Test]
		public void VotePostDoesntAllowDupeVote()
		{
			var service = GetService();
			var post = new Post(1);
			var user = new User(2, new DateTime());
			var voters = new Dictionary<int, string> { { 1, "Foo" }, { user.UserID, "Dude" }, { 3, null }, { 4, "Chica" } };
			_postRepo.Setup(x => x.GetVotes(post.PostID)).Returns(voters);
			service.VotePost(post, user, "abc", "def", "ghi");
			_postRepo.Verify(x => x.VotePost(post.PostID, user.UserID), Times.Never());
		}

		[Test]
		public void GetVoteCountCallsRepoAndReturns()
		{
			var service = GetService();
			var post = new Post(1);
			const int votes = 32;
			_postRepo.Setup(x => x.GetVoteCount(post.PostID)).Returns(votes);
			var result = service.GetVoteCount(post);
			Assert.AreEqual(votes, result);
		}

		[Test]
		public void GetVotersReturnsContainerWithPostID()
		{
			var service = GetService();
			var post = new Post(1);
			_postRepo.Setup(x => x.GetVotes(post.PostID)).Returns(new Dictionary<int, string>());
			var result = service.GetVoters(post);
			Assert.AreEqual(post.PostID, result.PostID);
		}

		[Test]
		public void GetVotersReturnsContainerWithTotalVotes()
		{
			var service = GetService();
			var post = new Post(1);
			var voters = new Dictionary<int, string> {{1, "Foo"}, {2, "Dude"}, {3, null}, {4, "Chica"}};
			_postRepo.Setup(x => x.GetVotes(post.PostID)).Returns(voters);
			var result = service.GetVoters(post);
			Assert.AreEqual(4, result.Votes);
		}

		[Test]
		public void GetVotersFiltersNullNames()
		{
			var service = GetService();
			var post = new Post(1);
			var voters = new Dictionary<int, string> { { 1, "Foo" }, { 2, "Dude" }, { 3, null }, { 4, "Chica" } };
			_postRepo.Setup(x => x.GetVotes(post.PostID)).Returns(voters);
			var result = service.GetVoters(post);
			Assert.AreEqual(3, result.Voters.Count);
			Assert.False(result.Voters.ContainsValue(null));
		}

		[Test]
		public void GetVotedIDsPassesUserID()
		{
			var service = GetService();
			var user = new User(123, DateTime.MinValue);
			service.GetVotedPostIDs(user, new List<Post>());
			_postRepo.Verify(x => x.GetVotedPostIDs(user.UserID, It.IsAny<List<int>>()), Times.Once());
		}

		[Test]
		public void GetVotedIDsPassesPostIDList()
		{
			var service = GetService();
			var user = new User(123, DateTime.MinValue);
			var list = new List<Post> {new Post(4), new Post(5), new Post(8)};
			List<int> returnedList = null;
			_postRepo.Setup(x => x.GetVotedPostIDs(user.UserID, It.IsAny<List<int>>())).Callback<int, List<int>>((u, l) => returnedList = l);
			service.GetVotedPostIDs(user, list);
			Assert.AreEqual(3, returnedList.Count);
			Assert.AreEqual(4, returnedList[0]);
			Assert.AreEqual(5, returnedList[1]);
			Assert.AreEqual(8, returnedList[2]);
		}

		[Test]
		public void GetVotedIDsReturnsRepoObject()
		{
			var service = GetService();
			var list = new List<int>();
			_postRepo.Setup(x => x.GetVotedPostIDs(It.IsAny<int>(), It.IsAny<List<int>>())).Returns(list);
			var result = service.GetVotedPostIDs(new User(123, DateTime.MinValue), new List<Post>());
			Assert.AreSame(list, result);
		}

		[Test]
		public void GetVotedIDsReturnsEmptyListWithNullUser()
		{
			var service = GetService();
			var result = service.GetVotedPostIDs(null, new List<Post>());
			Assert.AreEqual(0, result.Count);
		}

		[Test]
		public void VoteUpDoesntCallPublisherWhenUserFromPostDoesNotExist()
		{
			var service = GetService();
			_userService.Setup(x => x.GetUser(It.IsAny<int>())).Returns((User) null);
			_postRepo.Setup(x => x.GetVotes(It.IsAny<int>())).Returns(new Dictionary<int, string>());
			service.VotePost(new Post(123), new User(456, DateTime.MinValue), "", "", "");
			_eventPub.Verify(x => x.ProcessEvent(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<string>(), false), Times.Never());
		}

		[Test]
		public void VoteUpCallsEventPub()
		{
			var service = GetService();
			var voteUpUser = new User(777, DateTime.MinValue);
			_userService.Setup(x => x.GetUser(voteUpUser.UserID)).Returns(voteUpUser);
			_postRepo.Setup(x => x.GetVotes(It.IsAny<int>())).Returns(new Dictionary<int, string>());
			service.VotePost(new Post(123) { UserID = voteUpUser.UserID }, new User(456, DateTime.MinValue), "", "", "");
			_eventPub.Verify(x => x.ProcessEvent(It.IsAny<string>(), voteUpUser, EventDefinitionService.StaticEventIDs.PostVote, false), Times.Once());
		}

		[Test]
		public void VoteUpDoesNotCallWhenUserIsPoster()
		{
			var service = GetService();
			var voteUpUser = new User(456, DateTime.MinValue);
			_userService.Setup(x => x.GetUser(voteUpUser.UserID)).Returns(voteUpUser);
			_postRepo.Setup(x => x.GetVotes(It.IsAny<int>())).Returns(new Dictionary<int, string>());
			service.VotePost(new Post(123) { UserID = voteUpUser.UserID }, new User(456, DateTime.MinValue), "", "", "");
			_eventPub.Verify(x => x.ProcessEvent(It.IsAny<string>(), It.IsAny<User>(), EventDefinitionService.StaticEventIDs.PostVote, false), Times.Never());
		}

		[Test]
		public void VoteUpPassesPubStringWithFormattedStuff()
		{
			var service = GetService();
			var voteUpUser = new User(777, DateTime.MinValue);
			const string userUrl = "http://abc";
			const string topicUrl = "http://def";
			const string title = "blah blah blah";
			_userService.Setup(x => x.GetUser(voteUpUser.UserID)).Returns(voteUpUser);
			_postRepo.Setup(x => x.GetVotes(It.IsAny<int>())).Returns(new Dictionary<int, string>());
			var message = String.Empty;
			_eventPub.Setup(x => x.ProcessEvent(It.IsAny<string>(), It.IsAny<User>(), EventDefinitionService.StaticEventIDs.PostVote, false)).Callback<string, User, string, bool>((x, y, z, a) => message = x);
			service.VotePost(new Post(123) { UserID = voteUpUser.UserID }, new User(456, DateTime.MinValue), userUrl, topicUrl, title);
			Assert.True(message.Contains(userUrl));
			Assert.True(message.Contains(topicUrl));
			Assert.True(message.Contains(title));
		}
	}
}
