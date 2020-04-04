using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Services;
using PopForums.Extensions;
using PopForums.Mvc.Areas.Forums.Services;
using PopForums.Mvc.Areas.Forums.Extensions;

namespace PopForums.Mvc.Areas.Forums.Controllers
{
	[Area("Forums")]
	public class ForumController : Controller
	{
		public ForumController(ISettingsManager settingsManager, IForumService forumService, ITopicService topicService, IPostService postService, ITopicViewCountService topicViewCountService, ISubscribedTopicsService subService, ILastReadService lastReadService, IFavoriteTopicService favoriteTopicService, IProfileService profileService, IUserRetrievalShim userRetrievalShim, ITopicViewLogService topicViewLogService, IPostMasterService postMasterService, IForumPermissionService forumPermissionService)
		{
			_settingsManager = settingsManager;
			_forumService = forumService;
			_topicService = topicService;
			_postService = postService;
			_topicViewCountService = topicViewCountService;
			_subService = subService;
			_lastReadService = lastReadService;
			_favoriteTopicService = favoriteTopicService;
			_profileService = profileService;
			_userRetrievalShim = userRetrievalShim;
			_topicViewLogService = topicViewLogService;
			_postMasterService = postMasterService;
			_forumPermissionService = forumPermissionService;
		}

		public static string Name = "Forum";

		private readonly ISettingsManager _settingsManager;
		private readonly IForumService _forumService;
		private readonly ITopicService _topicService;
		private readonly IPostService _postService;
		private readonly ITopicViewCountService _topicViewCountService;
		private readonly ISubscribedTopicsService _subService;
		private readonly ILastReadService _lastReadService;
		private readonly IFavoriteTopicService _favoriteTopicService;
		private readonly IProfileService _profileService;
		private readonly IUserRetrievalShim _userRetrievalShim;
		private readonly ITopicViewLogService _topicViewLogService;
		private readonly IPostMasterService _postMasterService;
		private readonly IForumPermissionService _forumPermissionService;

		public async Task<ActionResult> Index(string urlName, int pageNumber = 1)
		{
			if (string.IsNullOrWhiteSpace(urlName))
				return NotFound();
			var forum = await _forumService.Get(urlName);
			if (forum == null)
				return NotFound();
			var user = _userRetrievalShim.GetUser();
			var permissionContext = await _forumPermissionService.GetPermissionContext(forum, user);
			if (!permissionContext.UserCanView)
			{
				return StatusCode(403);
			}

			var (topics, pagerContext) = await _topicService.GetTopics(forum, permissionContext.UserCanModerate, pageNumber);
			var container = new ForumTopicContainer { Forum = forum, Topics = topics, PagerContext = pagerContext, PermissionContext = permissionContext };
			await _lastReadService.GetTopicReadStatus(user, container);
			var adapter = new ForumAdapterFactory(forum);
			if (adapter.IsAdapterEnabled)
			{
				adapter.ForumAdapter.AdaptForum(this, container);
				if (string.IsNullOrWhiteSpace(adapter.ForumAdapter.ViewName))
					return View(adapter.ForumAdapter.Model);
				return View(adapter.ForumAdapter.ViewName, adapter.ForumAdapter.Model);
			}
			if (forum.IsQAForum)
				return View("IndexQA", container);
			return View(container);
		}

		public async Task<ActionResult> PostTopic(int id)
		{
			var user = _userRetrievalShim.GetUser();
			if (user == null)
				return Content(Resources.LoginToPost);
			var (forum, permissionContext) = await GetForumByIdWithPermissionContext(id, user);
			if (!permissionContext.UserCanView)
				return Content(Resources.ForumNoView);
			if (!permissionContext.UserCanPost)
				return Content(Resources.ForumNoPost);

			var profile = await _profileService.GetProfile(user);
			var newPost = new NewPost { ItemID = forum.ForumID, IncludeSignature = profile.Signature.Length > 0, IsPlainText = profile.IsPlainText, IsImageEnabled = _settingsManager.Current.AllowImages };
			return View("NewTopic", newPost);
		}

		[HttpPost]
		public async Task<IActionResult> PostTopic(NewPost newPost)
		{
			var user = _userRetrievalShim.GetUser();
			if (user == null)
				return Forbid();
			var userProfileUrl = Url.Action("ViewProfile", "Account", new { id = user.UserID });
			string TopicLinkGenerator(Topic t) => Url.Action("Topic", "Forum", new {id = t.UrlName});
			string RedirectLinkGenerator(Topic t) => Url.RouteUrl(new {controller = "Forum", action = "Topic", id = t.UrlName});
			var ip = HttpContext.Connection.RemoteIpAddress.ToString();

			var result = await _postMasterService.PostNewTopic(user, newPost, ip, userProfileUrl, TopicLinkGenerator, RedirectLinkGenerator);

			if (result.IsSuccessful)
				return Json(new BasicJsonMessage {Result = true, Redirect = result.Redirect});
			return Json(new BasicJsonMessage {Result = false, Message = result.Message});
		}

		private async Task<Tuple<Forum, ForumPermissionContext>> GetForumByIdWithPermissionContext(int forumID, User user)
		{
			var forum = await _forumService.Get(forumID);
			if (forum == null)
				throw new Exception($"Forum {forumID} not found");
			var permissionContext = await _forumPermissionService.GetPermissionContext(forum, user);
			return Tuple.Create(forum, permissionContext);
		}

		private async Task<Tuple<ForumPermissionContext, Topic>> GetPermissionContextByTopicID(int topicID)
		{
			var topic = await _topicService.Get(topicID);
			if (topic == null)
				throw new Exception($"Topic {topicID} not found");
			var forum = await _forumService.Get(topic.ForumID);
			if (forum == null)
				throw new Exception($"Forum {topic.ForumID} not found");
			var user = _userRetrievalShim.GetUser();
			var permissionContext = await _forumPermissionService.GetPermissionContext(forum, user);
			return Tuple.Create(permissionContext, topic);
		}

		public async Task<ActionResult> TopicID(int id)
		{
			var topic = await _topicService.Get(id);
			if (topic == null)
				return NotFound();
			return RedirectToActionPermanent("Topic", new { id = topic.UrlName });
		}

		public async Task<ActionResult> Topic(string id, int pageNumber = 1)
		{
			var topic = await _topicService.Get(id);
			if (topic == null)
				return NotFound();
			var forum = await _forumService.Get(topic.ForumID);
			if (forum == null)
				throw new Exception($"TopicID {topic.TopicID} references ForumID {topic.ForumID}, which does not exist.");

			var user = _userRetrievalShim.GetUser();
			var adapter = new ForumAdapterFactory(forum);
			var permissionContext = await _forumPermissionService.GetPermissionContext(forum, user, topic);
			if (!permissionContext.UserCanView)
			{
				return NotFound();
			}

			PagerContext pagerContext = null;
			var isSubscribed = false;
			var isFavorite = false;
			DateTime? lastReadTime = DateTime.UtcNow;
			if (user != null)
			{
				lastReadTime = await _lastReadService.GetLastReadTime(user, topic);
				isFavorite = await _favoriteTopicService.IsTopicFavorite(user, topic);
				isSubscribed = await _subService.IsTopicSubscribed(user, topic);
				if (isSubscribed)
					await _subService.MarkSubscribedTopicViewed(user, topic);
				if (!adapter.IsAdapterEnabled || (adapter.IsAdapterEnabled && adapter.ForumAdapter.MarkViewedTopicRead))
					await _lastReadService.MarkTopicRead(user, topic);
				if (user.IsInRole(PermanentRoles.Moderator))
				{
					var categorizedForums = await _forumService.GetCategorizedForumContainer();
					var categorizedForumSelectList = new List<SelectListItem>();
					foreach (var uncategorizedForum in categorizedForums.UncategorizedForums)
						categorizedForumSelectList.Add(new SelectListItem { Value = uncategorizedForum.ForumID.ToString(), Text = uncategorizedForum.Title, Selected = forum.ForumID == uncategorizedForum.ForumID});
					foreach (var categoryPair in categorizedForums.CategoryDictionary)
					{
						var group = new SelectListGroup {Name = categoryPair.Key.Title};
						foreach (var categorizedForum in categoryPair.Value)
							categorizedForumSelectList.Add(new SelectListItem { Value = categorizedForum.ForumID.ToString(), Text = categorizedForum.Title, Selected = forum.ForumID == categorizedForum.ForumID, Group = group});
					}
					ViewBag.CategorizedForums = categorizedForumSelectList;
				}
			}
			List<Post> posts;
			if (forum.IsQAForum)
				posts = await _postService.GetPosts(topic, permissionContext.UserCanModerate);
			else
				(posts, pagerContext) = await _postService.GetPosts(topic, permissionContext.UserCanModerate, pageNumber);
			if (posts.Count == 0)
				return NotFound();
			var signatures = await _profileService.GetSignatures(posts);
			var avatars = await _profileService.GetAvatars(posts);
			var votedIDs = await _postService.GetVotedPostIDs(user, posts);
			var container = ComposeTopicContainer(topic, forum, permissionContext, isSubscribed, posts, pagerContext, isFavorite, signatures, avatars, votedIDs, lastReadTime);
			await _topicViewCountService.ProcessView(topic);
			await _topicViewLogService.LogView(user?.UserID, topic.TopicID);
			if (adapter.IsAdapterEnabled)
			{
				adapter.ForumAdapter.AdaptTopic(this, container);
				if (string.IsNullOrWhiteSpace(adapter.ForumAdapter.ViewName))
					return View(adapter.ForumAdapter.Model);
				return View(adapter.ForumAdapter.ViewName, adapter.ForumAdapter.Model);
			}
			if (forum.IsQAForum)
			{
				var containerForQA = _forumService.MapTopicContainerForQA(container);
				return View("TopicQA", containerForQA);
			}
			return View(container);
		}

		public async Task<ActionResult> TopicPage(int id, int pageNumber, int low, int high)
		{
			var topic = await _topicService.Get(id);
			if (topic == null)
				return NotFound();
			var forum = await _forumService.Get(topic.ForumID);
			if (forum == null)
				throw new Exception($"TopicID {topic.TopicID} references ForumID {topic.ForumID}, which does not exist.");
			var user = _userRetrievalShim.GetUser();

			var permissionContext = await _forumPermissionService.GetPermissionContext(forum, user, topic);
			if (!permissionContext.UserCanView)
			{
				return StatusCode(403);
			}

			DateTime? lastReadTime = DateTime.UtcNow;
			if (user != null)
			{
				lastReadTime = await _lastReadService.GetLastReadTime(user, topic);
			}

			var (posts, pagerContext) = await _postService.GetPosts(topic, permissionContext.UserCanModerate, pageNumber);
			if (posts.Count == 0)
				return NotFound();
			var signatures = await _profileService.GetSignatures(posts);
			var avatars = await _profileService.GetAvatars(posts);
			var votedIDs = await _postService.GetVotedPostIDs(user, posts);
			var container = ComposeTopicContainer(topic, forum, permissionContext, false, posts, pagerContext, false, signatures, avatars, votedIDs, lastReadTime);
			await _topicViewCountService.ProcessView(topic);
			ViewBag.Low = low;
			ViewBag.High = high;
			return View(container);
		}

		public async Task<ActionResult> PostReply(int id, int quotePostID = 0, int replyID = 0)
		{
			var user = _userRetrievalShim.GetUser();
			if (user == null)
				return Content(Resources.LoginToPost);
			var topic = await _topicService.Get(id);
			if (topic == null)
				return Content(Resources.TopicNotExist);
			var forum = await _forumService.Get(topic.ForumID);
			if (forum == null)
				throw new Exception(String.Format("TopicID {0} references ForumID {1}, which does not exist.", topic.TopicID, topic.ForumID));
			if (topic.IsClosed)
				return Content(Resources.Closed);
			var permissionContext = await _forumPermissionService.GetPermissionContext(forum, user, topic);
			if (!permissionContext.UserCanView)
				return Content(Resources.ForumNoView);
			if (!permissionContext.UserCanPost)
				return Content(Resources.ForumNoPost);

			var title = topic.Title;
			if (!title.ToLower().StartsWith("re:"))
				title = "Re: " + title;
			var profile = await _profileService.GetProfile(user);
			var newPost = new NewPost { ItemID = topic.TopicID, Title = title, IncludeSignature = profile.Signature.Length > 0, IsPlainText = profile.IsPlainText, IsImageEnabled = _settingsManager.Current.AllowImages, ParentPostID = replyID };

			if (quotePostID != 0)
			{
				var post = await _postService.Get(quotePostID);
				newPost.FullText = await _postService.GetPostForQuote(post, user, profile.IsPlainText);
			}

			if (forum.IsQAForum)
			{
				newPost.IncludeSignature = false;
				if (newPost.ParentPostID == 0)
				{
					ViewBag.IsQA = true;
					return View("NewReply", newPost);
				}
				return View("NewComment", newPost);
			}
			return View("NewReply", newPost);
		}

		[HttpPost]
		public async Task<JsonResult> PostReply(NewPost newPost)
		{
			var user = _userRetrievalShim.GetUser();
			var userProfileUrl = Url.Action("ViewProfile", "Account", new { id = user.UserID });
			string TopicLinkGenerator(Topic t) => this.FullUrlHelper("GoToNewestPost", Name, new { id = t.TopicID });
			string UnsubscribeLinkGenerator(User u, Topic t) => this.FullUrlHelper("Unsubscribe", SubscriptionController.Name, new {topicID = t.TopicID, userID = user.UserID, hash = _profileService.GetUnsubscribeHash(user)});
			string PostLinkGenerator(Post p) => Url.Action("PostLink", "Forum", new {id = p.PostID});
			string RedirectLinkGenerator(Post p) => Url.RouteUrl(new {controller = "Forum", action = "PostLink", id = p.PostID});
			var ip = HttpContext.Connection.RemoteIpAddress.ToString();

			var result = await _postMasterService.PostReply(user, newPost.ParentPostID, ip, false, newPost, DateTime.UtcNow, TopicLinkGenerator, UnsubscribeLinkGenerator, userProfileUrl, PostLinkGenerator, RedirectLinkGenerator);

			return Json(new BasicJsonMessage { Result = result.IsSuccessful, Redirect = result.Redirect, Message = result.Message });
		}

		public async Task<ActionResult> Post(int id)
		{
			var post = await _postService.Get(id);
			if (post == null)
				return NotFound();
			var (permissionContext, topic) = await GetPermissionContextByTopicID(post.TopicID);
			if (!permissionContext.UserCanView)
				return StatusCode(403);
			var user = _userRetrievalShim.GetUser();
			var postList = new List<Post> { post };
			var signatures = await _profileService.GetSignatures(postList);
			var avatars = await _profileService.GetAvatars(postList);
			var votedPostIDs = await _postService.GetVotedPostIDs(user, postList);
			ViewData["PopForums.Identity.CurrentUser"] = user; // TODO: what is this used for?
			if (user != null)
				await _lastReadService.MarkTopicRead(user, topic);
			return View("PostItem", new PostItemContainer { Post = post, Avatars = avatars, Signatures = signatures, VotedPostIDs = votedPostIDs, Topic = topic, User = user });
		}
		
		public async Task<ViewResult> Recent(int pageNumber = 1)
		{
			var includeDeleted = false;
			var user = _userRetrievalShim.GetUser();
			if (user != null && user.IsInRole(PermanentRoles.Moderator))
				includeDeleted = true;
			var titles = _forumService.GetAllForumTitles();
			var (topics, pagerContext) = await _forumService.GetRecentTopics(user, includeDeleted, pageNumber);
			var container = new PagedTopicContainer { ForumTitles = titles, PagerContext = pagerContext, Topics = topics };
			await _lastReadService.GetTopicReadStatus(user, container);
			return View(container);
		}

		[HttpPost]
		public async Task<RedirectToActionResult> MarkForumRead(int id)
		{
			var user = _userRetrievalShim.GetUser();
			if (user == null)
				throw new Exception("There is no logged in user. Can't mark forum read.");
			var forum = await _forumService.Get(id);
			if (forum == null)
				throw new Exception($"There is no ForumID {id} to mark as read.");
			await _lastReadService.MarkForumRead(user, forum);
			return RedirectToAction("Index", HomeController.Name);
		}

		[HttpPost]
		public async Task<RedirectToActionResult> MarkAllForumsRead()
		{
			var user = _userRetrievalShim.GetUser();
			if (user == null)
				throw new Exception("There is no logged in user. Can't mark forum read.");
			await _lastReadService.MarkAllForumsRead(user);
			return RedirectToAction("Index", HomeController.Name);
		}

		public async Task<ActionResult> PostLink(int id)
		{
			var includeDeleted = false;
			var user = _userRetrievalShim.GetUser();
			if (user != null && user.IsInRole(PermanentRoles.Moderator))
				includeDeleted = true;
			var post = await _postService.Get(id);
			if (post == null || (post.IsDeleted && (user == null || !user.IsInRole(PermanentRoles.Moderator))))
				return NotFound();
			var (pageNumber, topic) = await _postService.GetTopicPageForPost(post, includeDeleted);
			var forum = await _forumService.Get(topic.ForumID);
			var adapter = new ForumAdapterFactory(forum);
			if (adapter.IsAdapterEnabled)
			{
				var result = adapter.ForumAdapter.AdaptPostLink(this, post, topic, forum);
				if (result != null)
					return result;
			}
			var url = Url.Action("Topic", new { id = topic.UrlName, pageNumber }) + "#" + post.PostID;
			return Redirect(url);
		}

		public async Task<ActionResult> GoToNewestPost(int id)
		{
			var topic = await _topicService.Get(id);
			if (topic == null)
				return NotFound();
			var includeDeleted = false;
			var user = _userRetrievalShim.GetUser();
			if (user != null && user.IsInRole(PermanentRoles.Moderator))
				includeDeleted = true;
			if (user == null)
				return RedirectToAction("Topic", new { id = topic.UrlName });
			var post = await _lastReadService.GetFirstUnreadPost(user, topic);
			var (pageNumber, t) = await _postService.GetTopicPageForPost(post, includeDeleted);
			var url = Url.Action("Topic", new { id = topic.UrlName, pageNumber }) + "#" + post.PostID;
			return Redirect(url);
		}

		public async Task<ActionResult> Edit(int id)
		{
			var post = await _postService.Get(id);
			if (post == null)
				return NotFound();
			var user = _userRetrievalShim.GetUser();
			if (!user.IsPostEditable(post))
				return StatusCode(403);
			var postEdit = await _postService.GetPostForEdit(post, user);
			return View(postEdit);
		}

		[HttpPost]
		public async Task<ActionResult> Edit(int id, PostEdit postEdit)
		{
			var user = _userRetrievalShim.GetUser();
			string RedirectLinkGenerator(Post p) => Url.RouteUrl(new { controller = "Forum", action = "PostLink", id = p.PostID });
			var result = await _postMasterService.EditPost(id, postEdit, user, RedirectLinkGenerator);
			if (result.IsSuccessful)
				return Redirect(result.Redirect);
			ViewBag.Message = result.Message;
			return View(postEdit);
		}

		[HttpPost]
		public async Task<ActionResult> DeletePost(int id)
		{
			var post = await _postService.Get(id);
			var user = _userRetrievalShim.GetUser();
			if (!user.IsPostEditable(post))
				return StatusCode(403);
			await _postService.Delete(post, user);
			if (post.IsFirstInTopic || !user.IsInRole(PermanentRoles.Moderator))
			{
				var topic = await _topicService.Get(post.TopicID);
				var forum = await _forumService.Get(topic.ForumID);
				return RedirectToAction("Index", "Forum", new { urlName = forum.UrlName });
			}
			return RedirectToAction("PostLink", "Forum", new { id = post.PostID });
		}

		public async Task<ContentResult> IsLastPostInTopic(int id, int lastPostID)
		{
			var last = await _postService.GetLastPostID(id);
			var result = last == lastPostID;
			return Content(result.ToString());
		}

		public async Task<ActionResult> TopicPartial(int id, int lastPost, int lowPage)
		{
			var topic = await _topicService.Get(id);
			if (topic == null)
				return NotFound();
			var forum = await _forumService.Get(topic.ForumID);
			if (forum == null)
				throw new Exception($"TopicID {topic.TopicID} references ForumID {topic.ForumID}, which does not exist.");
			var user = _userRetrievalShim.GetUser();

			var permissionContext = await _forumPermissionService.GetPermissionContext(forum, user, topic);
			if (!permissionContext.UserCanView)
			{
				return StatusCode(403);
			}

			DateTime? lastReadTime = DateTime.UtcNow;
			if (user != null)
			{
				lastReadTime = await _lastReadService.GetLastReadTime(user, topic);
			}

			var (posts, pagerContext) = await _postService.GetPosts(topic, lastPost, permissionContext.UserCanModerate);
			var signatures = await _profileService.GetSignatures(posts);
			var avatars = await _profileService.GetAvatars(posts);
			var votedIDs = await _postService.GetVotedPostIDs(user, posts);
			var container = ComposeTopicContainer(topic, forum, permissionContext, false, posts, pagerContext, false, signatures, avatars, votedIDs, lastReadTime);
			ViewBag.Low = lowPage;
			ViewBag.High = pagerContext.PageCount;
			return View("TopicPage", container);
		}

		public async Task<ActionResult> Voters(int id)
		{
			var post = await _postService.Get(id);
			if (post == null)
				return NotFound();
			var voters = await _postService.GetVoters(post);
			return View(voters);
		}

		[HttpPost]
		public async Task<ActionResult> VotePost(int id)
		{
			var post = await _postService.Get(id);
			if (post == null)
				return NotFound();
			var topic = await _topicService.Get(post.TopicID);
			if (topic == null)
				throw new Exception($"Post {post.PostID} appears to be orphaned from a topic.");
			var user = _userRetrievalShim.GetUser();
			if (user == null)
				return StatusCode(403);
			var helper = Url;
			var userProfileUrl = helper.Action("ViewProfile", "Account", new { id = user.UserID });
			var topicUrl = helper.Action("PostLink", "Forum", new { id = post.PostID });
			await _postService.VotePost(post, user, userProfileUrl, topicUrl, topic.Title);
			var count = await _postService.GetVoteCount(post);
			return View("Votes", count);
		}

		[HttpPost]
		// TODO: test validate [ValidateInput(false)]
		public ContentResult PreviewText(string fullText, bool isPlainText)
		{
			var result = _postService.GenerateParsedTextPreview(fullText, isPlainText);
			return Content(result, "text/html");
		}

		private static TopicContainer ComposeTopicContainer(Topic topic, Forum forum, ForumPermissionContext permissionContext, bool isSubscribed, List<Post> posts, PagerContext pagerContext, bool isFavorite, Dictionary<int, string> signatures, Dictionary<int, int> avatars, List<int> votedPostIDs, DateTime? lastreadTime)
		{
			return new TopicContainer { Forum = forum, Topic = topic, Posts = posts, PagerContext = pagerContext, PermissionContext = permissionContext, IsSubscribed = isSubscribed, IsFavorite = isFavorite, Signatures = signatures, Avatars = avatars, VotedPostIDs = votedPostIDs, LastReadTime = lastreadTime };
		}

		[HttpPost]
		public async Task<ActionResult> SetAnswer(int topicID, int postID)
		{
			var post = await _postService.Get(postID);
			if (post == null)
				return NotFound();
			var topic = await _topicService.Get(topicID);
			if (topic == null)
				return NotFound();
			var user = _userRetrievalShim.GetUser();
			if (user == null)
				return StatusCode(403);
			try
			{
				var helper = Url;
				var userProfileUrl = helper.Action("ViewProfile", "Account", new { id = user.UserID });
				var topicUrl = helper.Action("PostLink", "Forum", new { id = post.PostID });
				await _topicService.SetAnswer(user, topic, post, userProfileUrl, topicUrl);
			}
			catch (SecurityException) // TODO: what is this?
			{
				return StatusCode(403);
			}
			return new EmptyResult();
		}
	}
}
