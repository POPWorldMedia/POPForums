using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Services
{
	public interface IPostService
	{
		Task<Tuple<List<Post>, PagerContext>> GetPosts(Topic topic, bool includeDeleted, int pageIndex);
		Task<Tuple<List<Post>, PagerContext>> GetPosts(Topic topic, int lastLoadedPostID, bool includeDeleted);
		Task<List<Post>> GetPosts(Topic topic, bool includeDeleted);
		Task<Post> Get(int postID);
		Task<Tuple<int, Topic>> GetTopicPageForPost(Post post, bool includeDeleted);
		Task<int> GetPostCount(User user);
		Task<PostEdit> GetPostForEdit(Post post, User user);
		Task Delete(Post post, User user);
		Task Undelete(Post post, User user);
		Task<string> GetPostForQuote(Post post, User user, bool forcePlainText);
		Task<List<IPHistoryEvent>> GetIPHistory(string ip, DateTime start, DateTime end);
		Task<int> GetLastPostID(int topicID);
		Task VotePost(Post post, User user, string userUrl, string topicUrl, string topicTitle);
		Task<VotePostContainer> GetVoters(Post post);
		Task<int> GetVoteCount(Post post);
		Task<List<int>> GetVotedPostIDs(User user, List<Post> posts);
		string GenerateParsedTextPreview(string text, bool isPlainText);
	}

	public class PostService : IPostService
	{
		public PostService(IPostRepository postRepository, IProfileRepository profileRepository, ISettingsManager settingsManager, ITopicService topicService, ITextParsingService textParsingService, IModerationLogService moderationLogService, IForumService forumService, IEventPublisher eventPublisher, IUserService userService, ISearchIndexQueueRepository searchIndexQueueRepository, ITenantService tenantService)
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
		private readonly ISearchIndexQueueRepository _searchIndexQueueRepository;
		private readonly ITenantService _tenantService;

		public async Task<Tuple<List<Post>, PagerContext>> GetPosts(Topic topic, bool includeDeleted, int pageIndex)
		{
			var pageSize = _settingsManager.Current.PostsPerPage;
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var posts = await _postRepository.Get(topic.TopicID, includeDeleted, startRow, pageSize);
			int postCount;
			if (includeDeleted)
				postCount = await _postRepository.GetReplyCount(topic.TopicID, true);
			else
				postCount = topic.ReplyCount + 1;
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(postCount) / Convert.ToDouble(pageSize)));
			var pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			return Tuple.Create(posts, pagerContext);
		}

		public async Task<Tuple<List<Post>, PagerContext>> GetPosts(Topic topic, int lastLoadedPostID, bool includeDeleted)
		{
			var allPosts = await _postRepository.Get(topic.TopicID, includeDeleted);
			var lastIndex = allPosts.FindIndex(p => p.PostID == lastLoadedPostID);
			if (lastIndex < 0)
				throw new Exception($"PostID {lastLoadedPostID} is not a part of TopicID {topic.TopicID}.");
			var posts = allPosts.Skip(lastIndex + 1).ToList();
			var pageSize = _settingsManager.Current.PostsPerPage;
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(allPosts.Count) / Convert.ToDouble(pageSize)));
			var pagerContext = new PagerContext { PageCount = totalPages, PageIndex = totalPages, PageSize = pageSize };
			return Tuple.Create(posts, pagerContext);
		}

		public async Task<List<Post>> GetPosts(Topic topic, bool includeDeleted)
		{
			return await _postRepository.Get(topic.TopicID, includeDeleted);
		}

		public async Task<Post> Get(int postID)
		{
			return await _postRepository.Get(postID);
		}

		public async Task<Tuple<int, Topic>> GetTopicPageForPost(Post post, bool includeDeleted)
		{
			var topic = await _topicService.Get(post.TopicID);
			var ids = await _postRepository.GetPostIDsWithTimes(post.TopicID, includeDeleted);
			var postIDs = ids.Select(p => p.Key).ToList();
			var index = postIDs.IndexOf(post.PostID);
			var pageSize = _settingsManager.Current.PostsPerPage;
			var page = Convert.ToInt32(Math.Floor((double)index/pageSize)) + 1;
			return Tuple.Create(page, topic);
		}

		public async Task<int> GetPostCount(User user)
		{
			return await _postRepository.GetPostCount(user.UserID);
		}

		public async Task<PostEdit> GetPostForEdit(Post post, User user)
		{
			if (post == null)
				throw new ArgumentNullException("post");
			if (user == null)
				throw new ArgumentNullException("user");
			var profile = await _profileRepository.GetProfile(user.UserID);
			var postEdit = new PostEdit(post) { IsPlainText = profile.IsPlainText, IsFirstInTopic = post.IsFirstInTopic };
			if (profile.IsPlainText)
			{
				postEdit.FullText = _textParsingService.HtmlToForumCode(post.FullText);
				postEdit.IsPlainText = true;
			}
			else
				postEdit.FullText = _textParsingService.HtmlToClientHtml(post.FullText);
			return postEdit;
		}

		public async Task<string> GetPostForQuote(Post post, User user, bool forcePlainText)
		{
			if (post == null)
				throw new ArgumentNullException("post");
			if (post.IsDeleted)
				return "Post not found";
			if (user == null)
				throw new ArgumentNullException("user");
			var profile = await _profileRepository.GetProfile(user.UserID);
			string quote;
			if (profile.IsPlainText || forcePlainText)
				quote = String.Format("[quote]\r\n[i]{0} said:[/i]\r\n{1}[/quote]", post.Name, _textParsingService.HtmlToForumCode(post.FullText));
			else
				quote = String.Format("[quote]<i>{0} said:</i><br />{1}[/quote]", post.Name, _textParsingService.HtmlToClientHtml(post.FullText));
			return quote;
		}

		public async Task Delete(Post post, User user)
		{
			if (user.UserID == post.UserID || user.IsInRole(PermanentRoles.Moderator))
			{
				var topic = await _topicService.Get(post.TopicID);
				var forum = await _forumService.Get(topic.ForumID);
				if (post.IsFirstInTopic)
					await _topicService.DeleteTopic(topic, user);
				else
				{
					await _moderationLogService.LogPost(user, ModerationType.PostDelete, post, String.Empty, String.Empty);
					post.IsDeleted = true;
					post.LastEditTime = DateTime.UtcNow;
					post.LastEditName = user.Name;
					post.IsEdited = true;
					await _postRepository.Update(post);
					await _topicService.RecalculateReplyCount(topic);
					await _topicService.UpdateLast(topic);
					_forumService.UpdateCounts(forum);
					await _forumService.UpdateLast(forum);
					await _searchIndexQueueRepository.Enqueue(new SearchIndexPayload { TenantID = _tenantService.GetTenant(), TopicID = topic.TopicID });
				}
			}
			else
				throw new InvalidOperationException("User must be Moderator or author to delete post.");
		}

		public async Task Undelete(Post post, User user)
		{
			if (user.IsInRole(PermanentRoles.Moderator))
			{
				await _moderationLogService.LogPost(user, ModerationType.PostUndelete, post, String.Empty, String.Empty);
				post.IsDeleted = false;
				post.LastEditTime = DateTime.UtcNow;
				post.LastEditName = user.Name;
				post.IsEdited = true;
				await _postRepository.Update(post);
				var topic = await _topicService.Get(post.TopicID);
				await _topicService.RecalculateReplyCount(topic);
				await _topicService.UpdateLast(topic);
				var forum = await _forumService.Get(topic.ForumID);
				_forumService.UpdateCounts(forum);
				await _forumService.UpdateLast(forum);
				await _searchIndexQueueRepository.Enqueue(new SearchIndexPayload {TenantID = _tenantService.GetTenant(), TopicID = topic.TopicID});
			}
			else
				throw new InvalidOperationException("User must be Moderator to undelete post.");
		}

		public async Task<List<IPHistoryEvent>> GetIPHistory(string ip, DateTime start, DateTime end)
		{
			return await _postRepository.GetIPHistory(ip, start, end);
		}

		public async Task<int> GetLastPostID(int topicID)
		{
			return await _postRepository.GetLastPostID(topicID);
		}

		public async Task VotePost(Post post, User user, string userUrl, string topicUrl, string topicTitle)
		{
			if (post.UserID == user.UserID)
				return;
			var voters = await _postRepository.GetVotes(post.PostID);
			if (voters.ContainsKey(user.UserID))
				return;
			await _postRepository.VotePost(post.PostID, user.UserID);
			var votes = await _postRepository.CalculateVoteCount(post.PostID);
			await _postRepository.SetVoteCount(post.PostID, votes);
			var votedUpUser = await _userService.GetUser(post.UserID);
			if (votedUpUser != null)
			{
				// <a href="{0}">{1}</a> voted for a post in the topic: <a href="{2}">{3}</a>
				var message = String.Format(Resources.VoteUpPublishMessage, userUrl, user.Name, topicUrl, topicTitle);
				await _eventPublisher.ProcessEvent(message, votedUpUser, EventDefinitionService.StaticEventIDs.PostVote, false);
			}
		}

		public async Task<VotePostContainer> GetVoters(Post post)
		{
			var results = await _postRepository.GetVotes(post.PostID);
			var filtered = results.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
			var container = new VotePostContainer
			                	{
			                		PostID = post.PostID,
									Votes = results.Count,
									Voters = filtered
			                	};
			return container;
		}

		public async Task<int> GetVoteCount(Post post)
		{
			return await _postRepository.GetVoteCount(post.PostID);
		}

		public async Task<List<int>> GetVotedPostIDs(User user, List<Post> posts)
		{
			if (user == null)
				return new List<int>();
			var ids = posts.Select(x => x.PostID).ToList();
			return await _postRepository.GetVotedPostIDs(user.UserID, ids);
		}

		public string GenerateParsedTextPreview(string text, bool isPlainText)
		{
			var result = isPlainText ? _textParsingService.ForumCodeToHtml(text) : _textParsingService.ClientHtmlToHtml(text);
			return result;
		}
	}
}
