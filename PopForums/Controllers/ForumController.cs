using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Ninject;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Services;
using PopForums.Web;

namespace PopForums.Controllers
{
	public class ForumController : Controller
	{
		public ForumController()
		{
			var container = PopForumsActivation.Kernel;
			SettingsManager = container.Get<ISettingsManager>();
			ForumService = container.Get<IForumService>();
			TopicService = container.Get<ITopicService>();
			PostService = container.Get<IPostService>();
			TopicViewCountService = container.Get<ITopicViewCountService>();
			SubService = container.Get<ISubscribedTopicsService>();
			LastReadService = container.Get<ILastReadService>();
			FavoriteTopicService = container.Get<IFavoriteTopicService>();
			ProfileService = container.Get<IProfileService>();
			MobileDetectionWrapper = container.Get<IMobileDetectionWrapper>();
		}

		protected internal ForumController(ISettingsManager settingsManager, IForumService forumService, ITopicService topicService, IPostService postService, ITopicViewCountService topicViewCountService, ISubscribedTopicsService subService, ILastReadService lastReadService, IFavoriteTopicService favoriteTopicService, IProfileService profileService, IMobileDetectionWrapper mobileDetectionWrapper)
		{
			SettingsManager = settingsManager;
			ForumService = forumService;
			TopicService = topicService;
			PostService = postService;
			TopicViewCountService = topicViewCountService;
			SubService = subService;
			LastReadService = lastReadService;
			FavoriteTopicService = favoriteTopicService;
			ProfileService = profileService;
			MobileDetectionWrapper = mobileDetectionWrapper;
		}

		public static string Name = "Forum";

		public ISettingsManager SettingsManager { get; private set; }
		public IForumService ForumService { get; private set; }
		public ITopicService TopicService { get; private set; }
		public IPostService PostService { get; private set; }
		public ITopicViewCountService TopicViewCountService { get; private set; }
		public ISubscribedTopicsService SubService { get; private set; }
		public ILastReadService LastReadService { get; private set; }
		public IFavoriteTopicService FavoriteTopicService { get; private set; }
		public IProfileService ProfileService { get; private set; }
		public IMobileDetectionWrapper MobileDetectionWrapper { get; private set; }

		public ActionResult Index(string urlName, int page = 1)
		{
			var forum = ForumService.Get(urlName);
			if (forum == null)
				return this.NotFound("NotFound", null);
			var user = this.CurrentUser();
			var permissionContext = ForumService.GetPermissionContext(forum, user);
			if (!permissionContext.UserCanView)
			{
				return this.Forbidden("Forbidden", null);
			}

			PagerContext pagerContext;
			var topics = TopicService.GetTopics(forum, permissionContext.UserCanModerate, page, out pagerContext);
			var container = new ForumTopicContainer {Forum = forum, Topics = topics, PagerContext = pagerContext, PermissionContext = permissionContext};
			LastReadService.GetTopicReadStatus(user, container);
			var adapter = new ForumAdapterFactory(forum);
			if (adapter.IsAdapterEnabled)
			{
				adapter.ForumAdapter.AdaptForum(this, container);
				if (String.IsNullOrWhiteSpace(adapter.ForumAdapter.ViewName))
					return View(adapter.ForumAdapter.Model);
				return View(adapter.ForumAdapter.ViewName, adapter.ForumAdapter.Model);
			}
			return View(container);
		}

		public ActionResult PostTopic(int id)
		{
			var user = this.CurrentUser();
			if (user == null)
				return Content(Resources.LoginToPost);
			ForumPermissionContext permissionContext;
			var forum = GetForumByIdWithPermissionContext(id, out permissionContext);
			if (!permissionContext.UserCanView)
				return Content(Resources.ForumNoView);
			if (!permissionContext.UserCanPost)
				return Content(Resources.ForumNoPost);

			var profile = ProfileService.GetProfile(user);
			var newPost = new NewPost { ItemID = forum.ForumID, IncludeSignature = profile.Signature.Length > 0, IsPlainText = MobileDetectionWrapper.IsMobileDevice(HttpContext) || profile.IsPlainText, IsImageEnabled = SettingsManager.Current.AllowImages };
			return View("NewTopic", newPost);
		}

		[HttpPost]
		[ValidateInput(false)]
		public JsonResult PostTopic(NewPost newPost)
		{
			if (this.CurrentUser() == null)
				return Json(new BasicJsonMessage { Message = Resources.LoginToPost, Result = false });
			ForumPermissionContext permissionContext;
			var forum = GetForumByIdWithPermissionContext(newPost.ItemID, out permissionContext);
			if (!permissionContext.UserCanView)
				return Json(new BasicJsonMessage {Message = Resources.ForumNoView, Result = false});
			if (!permissionContext.UserCanPost)
				return Json(new BasicJsonMessage {Message = Resources.ForumNoPost, Result = false});
			if (PostService.IsNewPostDupeOrInTimeLimit(newPost, this.CurrentUser()))
				return Json(new BasicJsonMessage { Message = String.Format(Resources.PostWait, SettingsManager.Current.MinimumSecondsBetweenPosts), Result = false });
            if (String.IsNullOrEmpty(newPost.FullText))
                return Json(new BasicJsonMessage { Message = Resources.PostEmpty, Result = false });

			var user = this.CurrentUser();
			var urlHelper = new UrlHelper(ControllerContext.RequestContext);
			var userProfileUrl = urlHelper.Action("ViewProfile", "Account", new { id = user.UserID });
			Func<Topic, string> topicLinkGenerator = t => urlHelper.Action("Topic", "Forum", new { id = t.UrlName });
			var topic = ForumService.PostNewTopic(forum, user, permissionContext, newPost, Request.UserHostAddress, userProfileUrl, topicLinkGenerator);
			TopicViewCountService.SetViewedTopic(topic, HttpContext);
			return Json(new BasicJsonMessage { Result = true, Redirect = urlHelper.RouteUrl(new { controller = "Forum", action = "Topic", id = topic.UrlName }) });
		}

		private Forum GetForumByIdWithPermissionContext(int forumID, out ForumPermissionContext permissionContext)
		{
			var forum = ForumService.Get(forumID);
			if (forum == null)
				throw new Exception(String.Format("Forum {0} not found", forumID));
			permissionContext = ForumService.GetPermissionContext(forum, this.CurrentUser());
			return forum;
		}

		private ForumPermissionContext GetPermissionContextByTopicID(int topicID, out Topic topic)
		{
			topic = TopicService.Get(topicID);
			if (topic == null)
				throw new Exception(String.Format("Topic {0} not found", topicID));
			var forum = ForumService.Get(topic.ForumID);
			if (forum == null)
				throw new Exception(String.Format("Forum {0} not found", topic.ForumID));
			return ForumService.GetPermissionContext(forum, this.CurrentUser());
		}

		public ActionResult TopicID(int id)
		{
			var topic = TopicService.Get(id);
			if (topic == null)
				return this.NotFound("NotFound", null);
			return RedirectToActionPermanent("Topic", new {id = topic.UrlName});
		}

		public ViewResult Topic(string id, int page = 1)
		{
			var topic = TopicService.Get(id);
			if (topic == null)
				return this.NotFound("NotFound", null);
			var forum = ForumService.Get(topic.ForumID);
			if (forum == null)
				throw new Exception(String.Format("TopicID {0} references ForumID {1}, which does not exist.", topic.TopicID, topic.ForumID));

			var adapter = new ForumAdapterFactory(forum);
			var permissionContext = ForumService.GetPermissionContext(forum, this.CurrentUser(), topic);
			if (!permissionContext.UserCanView)
			{
				return this.Forbidden("Forbidden", null);
			}

			PagerContext pagerContext;
			var isSubscribed = false;
			var isFavorite = false;
			var user = this.CurrentUser();
			if (user != null)
			{
				isFavorite = FavoriteTopicService.IsTopicFavorite(user, topic);
				isSubscribed = SubService.IsTopicSubscribed(user, topic);
				if (isSubscribed)
					SubService.MarkSubscribedTopicViewed(user, topic);
				if (!adapter.IsAdapterEnabled || (adapter.IsAdapterEnabled && adapter.ForumAdapter.MarkViewedTopicRead))
					LastReadService.MarkTopicRead(user, topic);
				if (user.IsInRole(PermanentRoles.Moderator))
					ViewBag.CategorizedForums = ForumService.GetCategorizedForumContainer();
			}
			var posts = PostService.GetPosts(topic, permissionContext.UserCanModerate, page, out pagerContext);
			if (posts.Count == 0)
				return this.NotFound("NotFound", null);
			var signatures = ProfileService.GetSignatures(posts);
			var avatars = ProfileService.GetAvatars(posts);
			var votedIDs = PostService.GetVotedPostIDs(this.CurrentUser(), posts);
			var container = ComposeTopicContainer(topic, forum, permissionContext, isSubscribed, posts, pagerContext, isFavorite, signatures, avatars, votedIDs);
			TopicViewCountService.ProcessView(topic, HttpContext);
			if (adapter.IsAdapterEnabled)
			{
				adapter.ForumAdapter.AdaptTopic(this, container);
				if (String.IsNullOrWhiteSpace(adapter.ForumAdapter.ViewName))
					return View(adapter.ForumAdapter.Model);
				return View(adapter.ForumAdapter.ViewName, adapter.ForumAdapter.Model);
			}
			return View(container);
		}

		public ActionResult TopicPage(int id, int page, int low, int high)
		{
			var topic = TopicService.Get(id);
			if (topic == null)
				return this.NotFound("NotFound", null);
			var forum = ForumService.Get(topic.ForumID);
			if (forum == null)
				throw new Exception(String.Format("TopicID {0} references ForumID {1}, which does not exist.", topic.TopicID, topic.ForumID));

			var permissionContext = ForumService.GetPermissionContext(forum, this.CurrentUser(), topic);
			if (!permissionContext.UserCanView)
			{
				return this.Forbidden("Forbidden", null);
			}

			PagerContext pagerContext;
			var posts = PostService.GetPosts(topic, permissionContext.UserCanModerate, page, out pagerContext);
			if (posts.Count == 0)
				return this.NotFound("NotFound", null);
			var signatures = ProfileService.GetSignatures(posts);
			var avatars = ProfileService.GetAvatars(posts);
			var votedIDs = PostService.GetVotedPostIDs(this.CurrentUser(), posts);
			var container = ComposeTopicContainer(topic, forum, permissionContext, false, posts, pagerContext, false, signatures, avatars, votedIDs);
			TopicViewCountService.ProcessView(topic, HttpContext);
			ViewBag.Low = low;
			ViewBag.High = high;
			return View(container);
		}

		public ActionResult PostReply(int id, int quotePostID = 0, int replyID = 0)
		{
			var user = this.CurrentUser();
			if (user == null)
				return Content(Resources.LoginToPost);
			var topic = TopicService.Get(id);
			if (topic == null)
				return Content(Resources.TopicNotExist);
			ForumPermissionContext permissionContext;
			GetForumByIdWithPermissionContext(topic.ForumID, out permissionContext);
			if (!permissionContext.UserCanView)
				return Content(Resources.ForumNoView);
			if (!permissionContext.UserCanPost)
				return Content(Resources.ForumNoPost);

			var title = topic.Title;
			if (!title.ToLower().StartsWith("re:"))
				title = "Re: " + title;
			var profile = ProfileService.GetProfile(user);
			var forcePlainText = MobileDetectionWrapper.IsMobileDevice(HttpContext);
			var newPost = new NewPost { ItemID = topic.TopicID, Title = title, IncludeSignature = profile.Signature.Length > 0, IsPlainText = forcePlainText || profile.IsPlainText, IsImageEnabled = SettingsManager.Current.AllowImages, ParentPostID = replyID };

			if (quotePostID != 0)
			{
				var post = PostService.Get(quotePostID);
				newPost.FullText = PostService.GetPostForQuote(post, user, forcePlainText);
			}
			return View("NewReply", newPost);
		}

		[HttpPost]
		[ValidateInput(false)]
		public JsonResult PostReply(NewPost newPost)
		{
			if (this.CurrentUser() == null)
				return Json(new BasicJsonMessage { Message = Resources.LoginToPost, Result = false });
			ForumPermissionContext permissionContext;
			var topic = TopicService.Get(newPost.ItemID);
			if (topic == null)
				return Json(new BasicJsonMessage { Message = Resources.TopicNotExist, Result = false });
			GetForumByIdWithPermissionContext(topic.ForumID, out permissionContext);
			if (!permissionContext.UserCanView)
				return Json(new BasicJsonMessage { Message = Resources.ForumNoView, Result = false });
			if (!permissionContext.UserCanPost)
				return Json(new BasicJsonMessage { Message = Resources.ForumNoPost, Result = false });
			if (PostService.IsNewPostDupeOrInTimeLimit(newPost, this.CurrentUser()))
				return Json(new BasicJsonMessage { Message = String.Format(Resources.PostWait, SettingsManager.Current.MinimumSecondsBetweenPosts), Result = false });
			if (String.IsNullOrEmpty(newPost.FullText))
				return Json(new BasicJsonMessage { Message = Resources.PostEmpty, Result = false });
			if (newPost.ParentPostID != 0)
			{
				var parentPost = PostService.Get(newPost.ParentPostID);
				if (parentPost == null || parentPost.TopicID != topic.TopicID)
					return Json(new BasicJsonMessage { Message = "This reply attempt is being made to a post in another topic", Result = false });
			}

			var user = this.CurrentUser();
			var topicLink = this.FullUrlHelper("GoToNewestPost", Name, new { id = topic.TopicID });
			Func<User, string> unsubscribeLinkGenerator =
				u => this.FullUrlHelper("Unsubscribe", SubscriptionController.Name, new { topicID = topic.TopicID, authKey = u.AuthorizationKey });
			var helper = new UrlHelper(Request.RequestContext);
			var userProfileUrl = helper.Action("ViewProfile", "Account", new { id = user.UserID });
			Func<Post, string> postLinkGenerator = p => helper.Action("PostLink", "Forum", new { id = p.PostID });
			var post = TopicService.PostReply(topic, user, newPost.ParentPostID, Request.UserHostAddress, false, newPost, DateTime.UtcNow, topicLink, unsubscribeLinkGenerator, userProfileUrl, postLinkGenerator);
			TopicViewCountService.SetViewedTopic(topic, HttpContext);
			var currentUser = this.CurrentUser();
			if (newPost.CloseOnReply && currentUser.IsInRole(PermanentRoles.Moderator))
				TopicService.CloseTopic(topic, currentUser);
			var urlHelper = new UrlHelper(ControllerContext.RequestContext);
			return Json(new BasicJsonMessage { Result = true, Redirect = urlHelper.RouteUrl(new { controller = "Forum", action = "PostLink", id = post.PostID }) });
		}

		public ViewResult Post(int id)
		{
			var post = PostService.Get(id);
			if (post == null)
				return this.NotFound("NotFound", null);
			Topic topic;
			var permissionContext = GetPermissionContextByTopicID(post.TopicID, out topic);
			if (!permissionContext.UserCanView)
				return this.Forbidden("Forbidden", null);
			var user = this.CurrentUser();
			var postList = new List<Post> {post};
			ViewBag.Signatures = ProfileService.GetSignatures(postList);
			ViewBag.Avatars = ProfileService.GetAvatars(postList);
			ViewBag.VotedPostIDs = PostService.GetVotedPostIDs(user, postList);
			ViewData[ViewDataDictionaries.ViewDataUserKey] = user;
			LastReadService.MarkTopicRead(user, topic);
			return View("PostItem", post);
		}

		public JsonResult FirstPostPreview(int id)
		{
			var topic = TopicService.Get(id);
			if (topic == null)
				return Json(new BasicJsonMessage {Message = Resources.TopicNotExist, Result = false}, JsonRequestBehavior.AllowGet);
			ForumPermissionContext permissionContext;
			GetForumByIdWithPermissionContext(topic.ForumID, out permissionContext);
			if (!permissionContext.UserCanView)
				return Json(new BasicJsonMessage { Message = Resources.ForumNoView, Result = false }, JsonRequestBehavior.AllowGet);
			var post = PostService.GetFirstInTopic(topic);
			var result = new BasicJsonMessage {Result = true, Data = new {post.FullText, post.Name, post.UserID}};
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		public ViewResult Recent(int page = 1)
		{
			var includeDeleted = false;
			var user = this.CurrentUser();
			if (user != null && user.IsInRole(PermanentRoles.Moderator))
				includeDeleted = true;
			var titles = ForumService.GetAllForumTitles();
			PagerContext pagerContext;
			var topics = ForumService.GetRecentTopics(user, includeDeleted, page, out pagerContext);
			var container = new PagedTopicContainer { ForumTitles = titles, PagerContext = pagerContext, Topics = topics };
			LastReadService.GetTopicReadStatus(user, container);
			return View(container);
		}

		[HttpPost]
		public RedirectToRouteResult MarkForumRead(int id)
		{
			var user = this.CurrentUser();
			if (user == null)
				throw new Exception("There is no logged in user. Can't mark forum read.");
			var forum = ForumService.Get(id);
			if (forum == null)
				throw new Exception(String.Format("There is no ForumID {0} to mark as read.", id));
			LastReadService.MarkForumRead(user, forum);
			return RedirectToAction("Index", ForumHomeController.Name);
		}

		[HttpPost]
		public RedirectToRouteResult MarkAllForumsRead()
		{
			var user = this.CurrentUser();
			if (user == null)
				throw new Exception("There is no logged in user. Can't mark forum read.");
			LastReadService.MarkAllForumsRead(user);
			return RedirectToAction("Index", ForumHomeController.Name);
		}

		public ActionResult PostLink(int id)
		{
			var includeDeleted = false;
			var user = this.CurrentUser();
			if (user != null && user.IsInRole(PermanentRoles.Moderator))
				includeDeleted = true;
			var post = PostService.Get(id);
			if (post == null || (post.IsDeleted && (user == null || !user.IsInRole(PermanentRoles.Moderator))))
				return this.NotFound("NotFound", null);
			Topic topic;
			var page = PostService.GetTopicPageForPost(post, includeDeleted, out topic);
			var forum = ForumService.Get(topic.ForumID);
			var adapter = new ForumAdapterFactory(forum);
			if (adapter.IsAdapterEnabled)
			{
				var result = adapter.ForumAdapter.AdaptPostLink(this, post, topic, forum);
				if (result != null)
					return result;
			}
			var url = Url.Action("Topic", new {id = topic.UrlName, page}) + "#" + post.PostID;
			return Redirect(url);
		}

		public ActionResult GoToNewestPost(int id)
		{
			var topic = TopicService.Get(id);
			if (topic == null)
				return this.NotFound("NotFound", null);
			var includeDeleted = false;
			var user = this.CurrentUser();
			if (user != null && user.IsInRole(PermanentRoles.Moderator))
				includeDeleted = true;
			if (user == null)
				return RedirectToAction("Topic", new {id = topic.UrlName});
			var post = LastReadService.GetFirstUnreadPost(user, topic);
			var page = PostService.GetTopicPageForPost(post, includeDeleted, out topic);
			var url = Url.Action("Topic", new { id = topic.UrlName, page }) + "#" + post.PostID;
			return Redirect(url);
		}

		public ActionResult Edit(int id)
		{
			var post = PostService.Get(id);
			if (post == null)
				return this.NotFound("NotFound", null);
			if (!User.IsPostEditable(post))
				return this.Forbidden("Forbidden", null);
			var isMobile = MobileDetectionWrapper.IsMobileDevice(HttpContext);
			var postEdit = PostService.GetPostForEdit(post, this.CurrentUser(), isMobile);
			return View(postEdit);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(int id, PostEdit postEdit)
		{
			var post = PostService.Get(id);
			if (!User.IsPostEditable(post))
				return this.Forbidden("Forbidden", null);
			PostService.EditPost(post, postEdit, this.CurrentUser());
			return RedirectToAction("PostLink", new { id = post.PostID });
		}

		[HttpPost]
		public ActionResult DeletePost(int id)
		{
			var post = PostService.Get(id);
			if (!User.IsPostEditable(post))
				return this.Forbidden("Forbidden", null);
			var user = this.CurrentUser();
			PostService.Delete(post, user);
			if (post.IsFirstInTopic || !user.IsInRole(PermanentRoles.Moderator))
			{
				var topic = TopicService.Get(post.TopicID);
				var forum = ForumService.Get(topic.ForumID);
				return RedirectToAction("Index", "Forum", new { urlName = forum.UrlName });
			}
			return RedirectToAction("PostLink", "Forum", new { id = post.PostID });
		}

		public ContentResult IsLastPostInTopic(int id, int lastPostID)
		{
			var last = PostService.GetLastPostID(id);
			var result = last == lastPostID;
			return Content(result.ToString());
		}

		public ActionResult TopicPartial(int id, int lastPost, int lowPage)
		{
			var topic = TopicService.Get(id);
			if (topic == null)
				return this.NotFound("NotFound", null);
			var forum = ForumService.Get(topic.ForumID);
			if (forum == null)
				throw new Exception(String.Format("TopicID {0} references ForumID {1}, which does not exist.", topic.TopicID, topic.ForumID));

			var permissionContext = ForumService.GetPermissionContext(forum, this.CurrentUser(), topic);
			if (!permissionContext.UserCanView)
			{
				return this.Forbidden("Forbidden", null);
			}

			PagerContext pagerContext;
			var posts = PostService.GetPosts(topic, lastPost, permissionContext.UserCanModerate, out pagerContext);
			var signatures = ProfileService.GetSignatures(posts);
			var avatars = ProfileService.GetAvatars(posts);
			var votedIDs = PostService.GetVotedPostIDs(this.CurrentUser(), posts);
			var container = ComposeTopicContainer(topic, forum, permissionContext, false, posts, pagerContext, false, signatures, avatars, votedIDs);
			ViewBag.Low = lowPage;
			ViewBag.High = pagerContext.PageCount;
			return View("TopicPage", container);
		}

		public ActionResult Voters(int id)
		{
			var post = PostService.Get(id);
			if (post == null)
				return HttpNotFound();
			var voters = PostService.GetVoters(post);
			return View(voters);
		}

		[HttpPost]
		public ActionResult VotePost(int id)
		{
			var post = PostService.Get(id);
			if (post == null)
				return HttpNotFound();
			var topic = TopicService.Get(post.TopicID);
			if (topic == null)
				throw new Exception(String.Format("Post {0} appears to be orphaned from a topic.", post.PostID));
			var user = this.CurrentUser();
			if (user == null)
				return this.Forbidden("Forbidden", null);
			var helper = new UrlHelper(Request.RequestContext);
			var userProfileUrl = helper.Action("ViewProfile", "Account", new {id = user.UserID});
			var topicUrl = helper.Action("PostLink", "Forum", new {id = post.PostID});
			PostService.VotePost(post, user, userProfileUrl, topicUrl, topic.Title);
			var count = PostService.GetVoteCount(post);
			return View("Votes", count);
		}

		private static TopicContainer ComposeTopicContainer(Topic topic, Forum forum, ForumPermissionContext permissionContext, bool isSubscribed, List<Post> posts, PagerContext pagerContext, bool isFavorite, Dictionary<int, string> signatures, Dictionary<int, int> avatars, List<int> votedPostIDs)
		{
			return new TopicContainer { Forum = forum, Topic = topic, Posts = posts, PagerContext = pagerContext, PermissionContext = permissionContext, IsSubscribed = isSubscribed, IsFavorite = isFavorite, Signatures = signatures, Avatars = avatars, VotedPostIDs = votedPostIDs };
		}
	}
}
