using System;
using System.Collections.Generic;
using System.Linq;
using PopForums.Configuration;
using PopForums.Feeds;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Services
{
	public interface IPostService
	{
		List<Post> GetPosts(Topic topic, bool includeDeleted, int pageIndex, out PagerContext pagerContext);
		List<Post> GetPosts(Topic topic, int lastLoadedPostID, bool includeDeleted, out PagerContext pagerContext);
		List<Post> GetPosts(Topic topic, bool includeDeleted);
		Post Get(int postID);
		List<Post> GetPostWithReplies(int id, bool includeDeleted);
		Post GetFirstInTopic(Topic topic);
		bool IsNewPostDupeOrInTimeLimit(NewPost newPost, User user);
		int GetTopicPageForPost(Post post, bool includeDeleted, out Topic topic);
		int GetPostCount(User user);
		PostEdit GetPostForEdit(Post post, User user);
		void EditPost(Post post, PostEdit postEdit, User editingUser);
		void Delete(Post post, User user);
		void Undelete(Post post, User user);
		string GetPostForQuote(Post post, User user, bool forcePlainText);
		List<IPHistoryEvent> GetIPHistory(string ip, DateTime start, DateTime end);
		int GetLastPostID(int topicID);
		void VotePost(Post post, User user, string userUrl, string topicUrl, string topicTitle);
		VotePostContainer GetVoters(Post post);
		int GetVoteCount(Post post);
		List<int> GetVotedPostIDs(User user, List<Post> posts);
		string GenerateParsedTextPreview(string text, bool isPlainText);
	}

	public class PostService : IPostService
	{
		public PostService(IPostRepository postRepository, IProfileRepository profileRepository, ISettingsManager settingsManager, ITopicService topicService, ITextParsingService textParsingService, IModerationLogService moderationLogService, IForumService forumService, IEventPublisher eventPublisher, IUserService userService, IFeedService feedService, ITopicRepository topicRepository, ISearchIndexQueueRepository searchIndexQueueRepository, ITenantService tenantService)
		{
			_postRepository = postRepository;
			_profileRepository = profileRepository;
			_settingsManager = settingsManager;
			_topicService = topicService;
			_textParsingService = textParsingService;
			_moderationLogService = moderationLogService;
			_forumService = forumService;
			_eventPublisher = eventPublisher;
			_userService = userService;
			_feedService = feedService;
			_topicRepository = topicRepository;
			_searchIndexQueueRepository = searchIndexQueueRepository;
			_tenantService = tenantService;
		}

		private readonly IPostRepository _postRepository;
		private readonly IProfileRepository _profileRepository;
		private readonly ISettingsManager _settingsManager;
		private readonly ITopicService _topicService;
		private readonly ITextParsingService _textParsingService;
		private readonly IModerationLogService _moderationLogService;
		private readonly IForumService _forumService;
		private readonly IEventPublisher _eventPublisher;
		private readonly IUserService _userService;
		private readonly IFeedService _feedService;
		private readonly ITopicRepository _topicRepository;
		private readonly ISearchIndexQueueRepository _searchIndexQueueRepository;
		private readonly ITenantService _tenantService;

		public List<Post> GetPosts(Topic topic, bool includeDeleted, int pageIndex, out PagerContext pagerContext)
		{
			var pageSize = _settingsManager.Current.PostsPerPage;
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var posts = _postRepository.Get(topic.TopicID, includeDeleted, startRow, pageSize);
			int postCount;
			if (includeDeleted)
				postCount = _postRepository.GetReplyCount(topic.TopicID, true);
			else
				postCount = topic.ReplyCount + 1;
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(postCount) / Convert.ToDouble(pageSize)));
			pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			return posts;
		}

		public List<Post> GetPosts(Topic topic, int lastLoadedPostID, bool includeDeleted, out PagerContext pagerContext)
		{
			var allPosts = _postRepository.Get(topic.TopicID, includeDeleted);
			var lastIndex = allPosts.FindIndex(p => p.PostID == lastLoadedPostID);
			if (lastIndex < 0)
				throw new Exception(String.Format("PostID {0} is not a part of TopicID {1}.", lastLoadedPostID, topic.TopicID));
			var posts = allPosts.Skip(lastIndex + 1).ToList();
			var pageSize = _settingsManager.Current.PostsPerPage;
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(allPosts.Count) / Convert.ToDouble(pageSize)));
			pagerContext = new PagerContext { PageCount = totalPages, PageIndex = totalPages, PageSize = pageSize };
			return posts;
		}

		public List<Post> GetPosts(Topic topic, bool includeDeleted)
		{
			return _postRepository.Get(topic.TopicID, includeDeleted);
		}

		public List<Post> GetPostWithReplies(int id, bool includeDeleted)
		{
			return _postRepository.GetPostWithReplies(id, includeDeleted);
		}

		public Post Get(int postID)
		{
			return _postRepository.Get(postID);
		}

		public Post GetFirstInTopic(Topic topic)
		{
			var post = _postRepository.GetFirstInTopic(topic.TopicID);
			if (post == null)
				throw new Exception("No first post found for TopicID " + topic.TopicID);
			return post;
		}

		public bool IsNewPostDupeOrInTimeLimit(NewPost newPost, User user)
		{
			var postID = _profileRepository.GetLastPostID(user.UserID);
			if (postID == null)
				return false;
			var lastPost = _postRepository.Get(postID.Value);
			if (lastPost == null)
				return false;
			var minimumSeconds = _settingsManager.Current.MinimumSecondsBetweenPosts;
			if (DateTime.UtcNow.Subtract(lastPost.PostTime).TotalSeconds < minimumSeconds)
				return true;
			var parsedText = newPost.IsPlainText ? _textParsingService.ForumCodeToHtml(newPost.FullText) : _textParsingService.ClientHtmlToHtml(newPost.FullText);
			if (parsedText == lastPost.FullText)
				return true;
			return false;
		}

		public int GetTopicPageForPost(Post post, bool includeDeleted, out Topic topic)
		{
			topic = _topicService.Get(post.TopicID);
			var postIDs = _postRepository.GetPostIDsWithTimes(post.TopicID, includeDeleted).Select(p => p.Key).ToList();
			var index = postIDs.IndexOf(post.PostID);
			var pageSize = _settingsManager.Current.PostsPerPage;
			var page = Convert.ToInt32(Math.Floor((double)index/pageSize)) + 1;
			return page;
		}

		public int GetPostCount(User user)
		{
			return _postRepository.GetPostCount(user.UserID);
		}

		public PostEdit GetPostForEdit(Post post, User user)
		{
			if (post == null)
				throw new ArgumentNullException("post");
			if (user == null)
				throw new ArgumentNullException("user");
			var profile = _profileRepository.GetProfile(user.UserID);
			var postEdit = new PostEdit(post) { IsPlainText = profile.IsPlainText };
			if (profile.IsPlainText)
			{
				postEdit.FullText = _textParsingService.HtmlToForumCode(post.FullText);
				postEdit.IsPlainText = true;
			}
			else
				postEdit.FullText = _textParsingService.HtmlToClientHtml(post.FullText);
			return postEdit;
		}

		public string GetPostForQuote(Post post, User user, bool forcePlainText)
		{
			if (post == null)
				throw new ArgumentNullException("post");
			if (post.IsDeleted)
				return "Post not found";
			if (user == null)
				throw new ArgumentNullException("user");
			var profile = _profileRepository.GetProfile(user.UserID);
			string quote;
			if (profile.IsPlainText || forcePlainText)
				quote = String.Format("[quote]\r\n[i]{0} said:[/i]\r\n{1}[/quote]", post.Name, _textParsingService.HtmlToForumCode(post.FullText));
			else
				quote = String.Format("[quote]<i>{0} said:</i><br />{1}[/quote]", post.Name, _textParsingService.HtmlToClientHtml(post.FullText));
			return quote;
		}

		public void EditPost(Post post, PostEdit postEdit, User editingUser)
		{
			// TODO: text parsing is controller for new topic and replies, see issue #121 https://github.com/POPWorldMedia/POPForums/issues/121
			// TODO: also not checking for empty posts
			var oldText = post.FullText;
			post.Title = _textParsingService.Censor(postEdit.Title);
			if (postEdit.IsPlainText)
				post.FullText = _textParsingService.ForumCodeToHtml(postEdit.FullText);
			else
				post.FullText = _textParsingService.ClientHtmlToHtml(postEdit.FullText);
			post.ShowSig = postEdit.ShowSig;
			post.LastEditTime = DateTime.UtcNow;
			post.LastEditName = editingUser.Name;
			post.IsEdited = true;
			_postRepository.Update(post);
			_moderationLogService.LogPost(editingUser, ModerationType.PostEdit, post, postEdit.Comment, oldText);
			_searchIndexQueueRepository.Enqueue(new SearchIndexPayload {TenantID = _tenantService.GetTenant(), TopicID = post.TopicID});
		}

		public void Delete(Post post, User user)
		{
			if (user.UserID == post.UserID || user.IsInRole(PermanentRoles.Moderator))
			{
				var topic = _topicService.Get(post.TopicID);
				var forum = _forumService.Get(topic.ForumID);
				if (post.IsFirstInTopic)
					_topicService.DeleteTopic(topic, user);
				else
				{
					_moderationLogService.LogPost(user, ModerationType.PostDelete, post, String.Empty, String.Empty);
					post.IsDeleted = true;
					post.LastEditTime = DateTime.UtcNow;
					post.LastEditName = user.Name;
					post.IsEdited = true;
					_postRepository.Update(post);
					_topicService.RecalculateReplyCount(topic);
					_topicService.UpdateLast(topic);
					_forumService.UpdateCounts(forum);
					_forumService.UpdateLast(forum);
				}
			}
			else
				throw new InvalidOperationException("User must be Moderator or author to delete post.");
		}

		public void Undelete(Post post, User user)
		{
			if (user.IsInRole(PermanentRoles.Moderator))
			{
				_moderationLogService.LogPost(user, ModerationType.PostUndelete, post, String.Empty, String.Empty);
				post.IsDeleted = false;
				post.LastEditTime = DateTime.UtcNow;
				post.LastEditName = user.Name;
				post.IsEdited = true;
				_postRepository.Update(post);
				var topic = _topicService.Get(post.TopicID);
				_topicService.RecalculateReplyCount(topic);
				_topicService.UpdateLast(topic);
				var forum = _forumService.Get(topic.ForumID);
				_forumService.UpdateCounts(forum);
				_forumService.UpdateLast(forum);
			}
			else
				throw new InvalidOperationException("User must be Moderator to undelete post.");
		}

		public List<IPHistoryEvent> GetIPHistory(string ip, DateTime start, DateTime end)
		{
			return _postRepository.GetIPHistory(ip, start, end);
		}

		public int GetLastPostID(int topicID)
		{
			return _postRepository.GetLastPostID(topicID);
		}

		public void VotePost(Post post, User user, string userUrl, string topicUrl, string topicTitle)
		{
			if (post.UserID == user.UserID)
				return;
			var voters = _postRepository.GetVotes(post.PostID);
			if (voters.ContainsKey(user.UserID))
				return;
			_postRepository.VotePost(post.PostID, user.UserID);
			var votes = _postRepository.CalculateVoteCount(post.PostID);
			_postRepository.SetVoteCount(post.PostID, votes);
			var votedUpUser = _userService.GetUser(post.UserID);
			if (votedUpUser != null)
			{
				// <a href="{0}">{1}</a> voted for a post in the topic: <a href="{2}">{3}</a>
				var message = String.Format(Resources.VoteUpPublishMessage, userUrl, user.Name, topicUrl, topicTitle);
				_eventPublisher.ProcessEvent(message, votedUpUser, EventDefinitionService.StaticEventIDs.PostVote, false);
			}
		}

		public VotePostContainer GetVoters(Post post)
		{
			var results = _postRepository.GetVotes(post.PostID);
			var filtered = results.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
			var container = new VotePostContainer
			                	{
			                		PostID = post.PostID,
									Votes = results.Count,
									Voters = filtered
			                	};
			return container;
		}

		public int GetVoteCount(Post post)
		{
			return _postRepository.GetVoteCount(post.PostID);
		}

		public List<int> GetVotedPostIDs(User user, List<Post> posts)
		{
			if (user == null)
				return new List<int>();
			var ids = posts.Select(x => x.PostID).ToList();
			return _postRepository.GetVotedPostIDs(user.UserID, ids);
		}

		public string GenerateParsedTextPreview(string text, bool isPlainText)
		{
			var result = isPlainText ? _textParsingService.ForumCodeToHtml(text) : _textParsingService.ClientHtmlToHtml(text);
			return result;
		}
	}
}
