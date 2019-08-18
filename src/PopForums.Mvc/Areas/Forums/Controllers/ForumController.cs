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
		public ForumController(ISettingsManager settingsManager, IForumService forumService, ITopicService topicService, IPostService postService, ITopicViewCountService topicViewCountService, ISubscribedTopicsService subService, ILastReadService lastReadService, IFavoriteTopicService favoriteTopicService, IProfileService profileService, IUserRetrievalShim userRetrievalShim, ITopicViewLogService topicViewLogService, ITextParsingService textParsingService, IPostMasterService postMasterService, IForumPermissionService forumPermissionService)
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
			_textParsingService = textParsingService;
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
		private readonly ITextParsingService _textParsingService;
		private readonly IPostMasterService _postMasterService;
		private readonly IForumPermissionService _forumPermissionService;

		public ActionResult Index(string urlName, int page = 1)
		{
			if (String.IsNullOrWhiteSpace(urlName))
				return NotFound();
			var forum = _forumService.Get(urlName);
			if (forum == null)
				return NotFound();
			var user = _userRetrievalShim.GetUser(HttpContext);
			var permissionContext = _forumPermissionService.GetPermissionContext(forum, user);
			if (!permissionContext.UserCanView)
			{
				return StatusCode(403);
			}

			PagerContext pagerContext;
			var topics = _topicService.GetTopics(forum, permissionContext.UserCanModerate, page, out pagerContext);
			var container = new ForumTopicContainer { Forum = forum, Topics = topics, PagerContext = pagerContext, PermissionContext = permissionContext };
			_lastReadService.GetTopicReadStatus(user, container);
			var adapter = new ForumAdapterFactory(forum);
			if (adapter.IsAdapterEnabled)
			{
				adapter.ForumAdapter.AdaptForum(this, container);
				if (String.IsNullOrWhiteSpace(adapter.ForumAdapter.ViewName))
					return View(adapter.ForumAdapter.Model);
				return View(adapter.ForumAdapter.ViewName, adapter.ForumAdapter.Model);
			}
			if (forum.IsQAForum)
				return View("IndexQA", container);
			return View(container);
		}

		public ActionResult PostTopic(int id)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return Content(Resources.LoginToPost);
			ForumPermissionContext permissionContext;
			var forum = GetForumByIdWithPermissionContext(id, user, out permissionContext);
			if (!permissionContext.UserCanView)
				return Content(Resources.ForumNoView);
			if (!permissionContext.UserCanPost)
				return Content(Resources.ForumNoPost);

			var profile = _profileService.GetProfile(user);
			var newPost = new NewPost { ItemID = forum.ForumID, IncludeSignature = profile.Signature.Length > 0, IsPlainText = profile.IsPlainText, IsImageEnabled = _settingsManager.Current.AllowImages };
			return View("NewTopic", newPost);
		}

		[HttpPost]
		public async Task<IActionResult> PostTopic(NewPost newPost)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
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

		private Forum GetForumByIdWithPermissionContext(int forumID, User user, out ForumPermissionContext permissionContext)
		{
			var forum = _forumService.Get(forumID);
			if (forum == null)
				throw new Exception($"Forum {forumID} not found");
			permissionContext = _forumPermissionService.GetPermissionContext(forum, user);
			return forum;
		}

		private ForumPermissionContext GetPermissionContextByTopicID(int topicID, out Topic topic)
		{
			topic = _topicService.Get(topicID);
			if (topic == null)
				throw new Exception(String.Format("Topic {0} not found", topicID));
			var forum = _forumService.Get(topic.ForumID);
			if (forum == null)
				throw new Exception(String.Format("Forum {0} not found", topic.ForumID));
			var user = _userRetrievalShim.GetUser(HttpContext);
			return _forumPermissionService.GetPermissionContext(forum, user);
		}

		public ActionResult TopicID(int id)
		{
			var topic = _topicService.Get(id);
			if (topic == null)
				return NotFound();
			return RedirectToActionPermanent("Topic", new { id = topic.UrlName });
		}

		public async Task<ActionResult> Topic(string id, int page = 1)
		{
			var topic = _topicService.Get(id);
			if (topic == null)
				return NotFound();
			var forum = _forumService.Get(topic.ForumID);
			if (forum == null)
				throw new Exception(String.Format("TopicID {0} references ForumID {1}, which does not exist.", topic.TopicID, topic.ForumID));

			var user = _userRetrievalShim.GetUser(HttpContext);
			var adapter = new ForumAdapterFactory(forum);
			var permissionContext = _forumPermissionService.GetPermissionContext(forum, user, topic);
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
				lastReadTime = _lastReadService.GetLastReadTime(user, topic);
				isFavorite = _favoriteTopicService.IsTopicFavorite(user, topic);
				isSubscribed = _subService.IsTopicSubscribed(user, topic);
				if (isSubscribed)
					_subService.MarkSubscribedTopicViewed(user, topic);
				if (!adapter.IsAdapterEnabled || (adapter.IsAdapterEnabled && adapter.ForumAdapter.MarkViewedTopicRead))
					_lastReadService.MarkTopicRead(user, topic);
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
				posts = _postService.GetPosts(topic, permissionContext.UserCanModerate);
			else
				posts = _postService.GetPosts(topic, permissionContext.UserCanModerate, page, out pagerContext);
			if (posts.Count == 0)
				return NotFound();
			var signatures = _profileService.GetSignatures(posts);
			var avatars = _profileService.GetAvatars(posts);
			var votedIDs = _postService.GetVotedPostIDs(user, posts);
			var container = ComposeTopicContainer(topic, forum, permissionContext, isSubscribed, posts, pagerContext, isFavorite, signatures, avatars, votedIDs, lastReadTime);
			_topicViewCountService.ProcessView(topic);
			await _topicViewLogService.LogView(user?.UserID, topic.TopicID);
			if (adapter.IsAdapterEnabled)
			{
				adapter.ForumAdapter.AdaptTopic(this, container);
				if (String.IsNullOrWhiteSpace(adapter.ForumAdapter.ViewName))
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

		public ActionResult TopicPage(int id, int page, int low, int high)
		{
			var topic = _topicService.Get(id);
			if (topic == null)
				return NotFound();
			var forum = _forumService.Get(topic.ForumID);
			if (forum == null)
				throw new Exception(String.Format("TopicID {0} references ForumID {1}, which does not exist.", topic.TopicID, topic.ForumID));
			var user = _userRetrievalShim.GetUser(HttpContext);

			var permissionContext = _forumPermissionService.GetPermissionContext(forum, user, topic);
			if (!permissionContext.UserCanView)
			{
				return StatusCode(403);
			}

			DateTime? lastReadTime = DateTime.UtcNow;
			if (user != null)
			{
				lastReadTime = _lastReadService.GetLastReadTime(user, topic);
			}

			PagerContext pagerContext;
			var posts = _postService.GetPosts(topic, permissionContext.UserCanModerate, page, out pagerContext);
			if (posts.Count == 0)
				return NotFound();
			var signatures = _profileService.GetSignatures(posts);
			var avatars = _profileService.GetAvatars(posts);
			var votedIDs = _postService.GetVotedPostIDs(user, posts);
			var container = ComposeTopicContainer(topic, forum, permissionContext, false, posts, pagerContext, false, signatures, avatars, votedIDs, lastReadTime);
			_topicViewCountService.ProcessView(topic);
			ViewBag.Low = low;
			ViewBag.High = high;
			return View(container);
		}

		public ActionResult PostReply(int id, int quotePostID = 0, int replyID = 0)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return Content(Resources.LoginToPost);
			var topic = _topicService.Get(id);
			if (topic == null)
				return Content(Resources.TopicNotExist);
			var forum = _forumService.Get(topic.ForumID);
			if (forum == null)
				throw new Exception(String.Format("TopicID {0} references ForumID {1}, which does not exist.", topic.TopicID, topic.ForumID));
			if (topic.IsClosed)
				return Content(Resources.Closed);
			var permissionContext = _forumPermissionService.GetPermissionContext(forum, user, topic);
			if (!permissionContext.UserCanView)
				return Content(Resources.ForumNoView);
			if (!permissionContext.UserCanPost)
				return Content(Resources.ForumNoPost);

			var title = topic.Title;
			if (!title.ToLower().StartsWith("re:"))
				title = "Re: " + title;
			var profile = _profileService.GetProfile(user);
			var newPost = new NewPost { ItemID = topic.TopicID, Title = title, IncludeSignature = profile.Signature.Length > 0, IsPlainText = profile.IsPlainText, IsImageEnabled = _settingsManager.Current.AllowImages, ParentPostID = replyID };

			if (quotePostID != 0)
			{
				var post = _postService.Get(quotePostID);
				newPost.FullText = _postService.GetPostForQuote(post, user, profile.IsPlainText);
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
		public JsonResult PostReply(NewPost newPost)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			var userProfileUrl = Url.Action("ViewProfile", "Account", new { id = user.UserID });
			string TopicLinkGenerator(Topic t) => this.FullUrlHelper("GoToNewestPost", Name, new { id = t.TopicID });
			string UnsubscribeLinkGenerator(User u, Topic t) => this.FullUrlHelper("Unsubscribe", SubscriptionController.Name, new {topicID = t.TopicID, authKey = u.AuthorizationKey});
			string PostLinkGenerator(Post p) => Url.Action("PostLink", "Forum", new {id = p.PostID});
			string RedirectLinkGenerator(Post p) => Url.RouteUrl(new {controller = "Forum", action = "PostLink", id = p.PostID});
			var ip = HttpContext.Connection.RemoteIpAddress.ToString();

			var result = _postMasterService.PostReply(user, newPost.ParentPostID, ip, false, newPost, DateTime.UtcNow, TopicLinkGenerator, UnsubscribeLinkGenerator, userProfileUrl, PostLinkGenerator, RedirectLinkGenerator);

			return Json(new BasicJsonMessage { Result = true, Redirect = result.Redirect });
		}

		public ActionResult Post(int id)
		{
			var post = _postService.Get(id);
			if (post == null)
				return NotFound();
			Topic topic;
			var permissionContext = GetPermissionContextByTopicID(post.TopicID, out topic);
			if (!permissionContext.UserCanView)
				return StatusCode(403);
			var user = _userRetrievalShim.GetUser(HttpContext);
			var postList = new List<Post> { post };
			var signatures = _profileService.GetSignatures(postList);
			var avatars = _profileService.GetAvatars(postList);
			var votedPostIDs = _postService.GetVotedPostIDs(user, postList);
			ViewData["PopForums.Identity.CurrentUser"] = user; // TODO: what is this used for?
			if (user != null)
				_lastReadService.MarkTopicRead(user, topic);
			return View("PostItem", new PostItemContainer { Post = post, Avatars = avatars, Signatures = signatures, VotedPostIDs = votedPostIDs, Topic = topic, User = user });
		}
		
		public ViewResult Recent(int page = 1)
		{
			var includeDeleted = false;
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user != null && user.IsInRole(PermanentRoles.Moderator))
				includeDeleted = true;
			var titles = _forumService.GetAllForumTitles();
			PagerContext pagerContext;
			var topics = _forumService.GetRecentTopics(user, includeDeleted, page, out pagerContext);
			var container = new PagedTopicContainer { ForumTitles = titles, PagerContext = pagerContext, Topics = topics };
			_lastReadService.GetTopicReadStatus(user, container);
			return View(container);
		}

		[HttpPost]
		public RedirectToActionResult MarkForumRead(int id)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				throw new Exception("There is no logged in user. Can't mark forum read.");
			var forum = _forumService.Get(id);
			if (forum == null)
				throw new Exception(String.Format("There is no ForumID {0} to mark as read.", id));
			_lastReadService.MarkForumRead(user, forum);
			return RedirectToAction("Index", HomeController.Name);
		}

		[HttpPost]
		public RedirectToActionResult MarkAllForumsRead()
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				throw new Exception("There is no logged in user. Can't mark forum read.");
			_lastReadService.MarkAllForumsRead(user);
			return RedirectToAction("Index", HomeController.Name);
		}

		public ActionResult PostLink(int id)
		{
			var includeDeleted = false;
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user != null && user.IsInRole(PermanentRoles.Moderator))
				includeDeleted = true;
			var post = _postService.Get(id);
			if (post == null || (post.IsDeleted && (user == null || !user.IsInRole(PermanentRoles.Moderator))))
				return NotFound();
			Topic topic;
			var page = _postService.GetTopicPageForPost(post, includeDeleted, out topic);
			var forum = _forumService.Get(topic.ForumID);
			var adapter = new ForumAdapterFactory(forum);
			if (adapter.IsAdapterEnabled)
			{
				var result = adapter.ForumAdapter.AdaptPostLink(this, post, topic, forum);
				if (result != null)
					return result;
			}
			var url = Url.Action("Topic", new { id = topic.UrlName, page }) + "#" + post.PostID;
			return Redirect(url);
		}

		public ActionResult GoToNewestPost(int id)
		{
			var topic = _topicService.Get(id);
			if (topic == null)
				return NotFound();
			var includeDeleted = false;
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user != null && user.IsInRole(PermanentRoles.Moderator))
				includeDeleted = true;
			if (user == null)
				return RedirectToAction("Topic", new { id = topic.UrlName });
			var post = _lastReadService.GetFirstUnreadPost(user, topic);
			var page = _postService.GetTopicPageForPost(post, includeDeleted, out topic);
			var url = Url.Action("Topic", new { id = topic.UrlName, page }) + "#" + post.PostID;
			return Redirect(url);
		}

		public ActionResult Edit(int id)
		{
			var post = _postService.Get(id);
			if (post == null)
				return NotFound();
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (!user.IsPostEditable(post))
				return StatusCode(403);
			var postEdit = _postService.GetPostForEdit(post, user);
			return View(postEdit);
		}

		[HttpPost]
		public ActionResult Edit(int id, PostEdit postEdit)
		{
			var user = _userRetrievalShim.GetUser(HttpContext);
			string RedirectLinkGenerator(Post p) => Url.RouteUrl(new { controller = "Forum", action = "PostLink", id = p.PostID });
			var result = _postMasterService.EditPost(id, postEdit, user, RedirectLinkGenerator);
			if (result.IsSuccessful)
				return Redirect(result.Redirect);
			ViewBag.Message = result.Message;
			return View(postEdit);
		}

		[HttpPost]
		public ActionResult DeletePost(int id)
		{
			var post = _postService.Get(id);
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (!user.IsPostEditable(post))
				return StatusCode(403);
			_postService.Delete(post, user);
			if (post.IsFirstInTopic || !user.IsInRole(PermanentRoles.Moderator))
			{
				var topic = _topicService.Get(post.TopicID);
				var forum = _forumService.Get(topic.ForumID);
				return RedirectToAction("Index", "Forum", new { urlName = forum.UrlName });
			}
			return RedirectToAction("PostLink", "Forum", new { id = post.PostID });
		}

		public ContentResult IsLastPostInTopic(int id, int lastPostID)
		{
			var last = _postService.GetLastPostID(id);
			var result = last == lastPostID;
			return Content(result.ToString());
		}

		public ActionResult TopicPartial(int id, int lastPost, int lowPage)
		{
			var topic = _topicService.Get(id);
			if (topic == null)
				return NotFound();
			var forum = _forumService.Get(topic.ForumID);
			if (forum == null)
				throw new Exception(String.Format("TopicID {0} references ForumID {1}, which does not exist.", topic.TopicID, topic.ForumID));
			var user = _userRetrievalShim.GetUser(HttpContext);

			var permissionContext = _forumPermissionService.GetPermissionContext(forum, user, topic);
			if (!permissionContext.UserCanView)
			{
				return StatusCode(403);
			}

			DateTime? lastReadTime = DateTime.UtcNow;
			if (user != null)
			{
				lastReadTime = _lastReadService.GetLastReadTime(user, topic);
			}

			PagerContext pagerContext;
			var posts = _postService.GetPosts(topic, lastPost, permissionContext.UserCanModerate, out pagerContext);
			var signatures = _profileService.GetSignatures(posts);
			var avatars = _profileService.GetAvatars(posts);
			var votedIDs = _postService.GetVotedPostIDs(user, posts);
			var container = ComposeTopicContainer(topic, forum, permissionContext, false, posts, pagerContext, false, signatures, avatars, votedIDs, lastReadTime);
			ViewBag.Low = lowPage;
			ViewBag.High = pagerContext.PageCount;
			return View("TopicPage", container);
		}

		public ActionResult Voters(int id)
		{
			var post = _postService.Get(id);
			if (post == null)
				return NotFound();
			var voters = _postService.GetVoters(post);
			return View(voters);
		}

		[HttpPost]
		public ActionResult VotePost(int id)
		{
			var post = _postService.Get(id);
			if (post == null)
				return NotFound();
			var topic = _topicService.Get(post.TopicID);
			if (topic == null)
				throw new Exception(String.Format("Post {0} appears to be orphaned from a topic.", post.PostID));
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return StatusCode(403);
			var helper = Url;
			var userProfileUrl = helper.Action("ViewProfile", "Account", new { id = user.UserID });
			var topicUrl = helper.Action("PostLink", "Forum", new { id = post.PostID });
			_postService.VotePost(post, user, userProfileUrl, topicUrl, topic.Title);
			var count = _postService.GetVoteCount(post);
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
		public ActionResult SetAnswer(int topicID, int postID)
		{
			var post = _postService.Get(postID);
			if (post == null)
				return NotFound();
			var topic = _topicService.Get(topicID);
			if (topic == null)
				return NotFound();
			var user = _userRetrievalShim.GetUser(HttpContext);
			if (user == null)
				return StatusCode(403);
			try
			{
				var helper = Url;
				var userProfileUrl = helper.Action("ViewProfile", "Account", new { id = user.UserID });
				var topicUrl = helper.Action("PostLink", "Forum", new { id = post.PostID });
				_topicService.SetAnswer(user, topic, post, userProfileUrl, topicUrl);
			}
			catch (SecurityException) // TODO: what is this?
			{
				return StatusCode(403);
			}
			return new EmptyResult();
		}
	}
}
