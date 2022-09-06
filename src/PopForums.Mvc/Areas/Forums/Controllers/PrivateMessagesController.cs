using PopForums.Composers;

namespace PopForums.Mvc.Areas.Forums.Controllers;

[Area("Forums")]
[TypeFilter(typeof(PopForumsPrivateForumsFilter))]
public class PrivateMessagesController : Controller
{
	public PrivateMessagesController(IPrivateMessageService privateMessageService, IUserService userService, IUserRetrievalShim userRetrievalShim, IPrivateMessageStateComposer privateMessageStateComposer)
	{
		_privateMessageService = privateMessageService;
		_userService = userService;
		_userRetrievalShim = userRetrievalShim;
		_privateMessageStateComposer = privateMessageStateComposer;
	}

	private readonly IPrivateMessageService _privateMessageService;
	private readonly IUserService _userService;
	private readonly IUserRetrievalShim _userRetrievalShim;
	private readonly IPrivateMessageStateComposer _privateMessageStateComposer;

	public static string Name = "PrivateMessages";

	public async Task<ActionResult> Index(int pageNumber = 1)
	{
		var user = _userRetrievalShim.GetUser();
		if (user == null)
			return StatusCode(403);
		var (privateMessages, pagerContext) = await _privateMessageService.GetPrivateMessages(user, PrivateMessageBoxType.Inbox, pageNumber);
		ViewBag.PagerContext = pagerContext;
		return View(privateMessages);
	}

	public async Task<ActionResult> Archive(int pageNumber = 1)
	{
		var user = _userRetrievalShim.GetUser();
		if (user == null)
			return StatusCode(403);
		var (privateMessages, pagerContext) = await _privateMessageService.GetPrivateMessages(user, PrivateMessageBoxType.Archive, pageNumber);
		ViewBag.PagerContext = pagerContext;
		return View(privateMessages);
	}

	public async Task<ActionResult> View(int id)
	{
		var user = _userRetrievalShim.GetUser();
		if (user == null)
			return StatusCode(403);
		var pm = await _privateMessageService.Get(id, user.UserID);
		if (await _privateMessageService.IsUserInPM(user.UserID, id) == false)
			return StatusCode(403);
		var state = await _privateMessageStateComposer.GetState(pm);
		var model = new PrivateMessageView
		{
			PrivateMessage = pm,
			State = state
		};
		await _privateMessageService.MarkPMRead(user.UserID, id);
		return View(model);
	}

	public async Task<ActionResult> Create(int? id)
	{
		var user = _userRetrievalShim.GetUser();
		if (user == null)
			return StatusCode(403);
		ViewBag.UserIDs = " ";
		if (id.HasValue)
		{
			var targetUser = await _userService.GetUser(id.Value);
			ViewBag.UserIDs = targetUser.UserID.ToString(CultureInfo.InvariantCulture);
			ViewBag.UserID = targetUser.UserID.ToString(CultureInfo.InvariantCulture);
			ViewBag.TargetUserID = targetUser.UserID;
			ViewBag.TargetUserName = targetUser.Name;
		}
		return View();
	}

	[HttpPost]
	public async Task<ActionResult> Create(string fullText, string userIDs)
	{
		var user = _userRetrievalShim.GetUser();
		if (user == null)
			return StatusCode(403);
		if (string.IsNullOrWhiteSpace(userIDs) || string.IsNullOrWhiteSpace(fullText))
		{
			ViewBag.Warning = Resources.PMCreateWarnings;
			return View("Create");
		}
		var ids = userIDs.Split(new[] { ',' }).Select(i => Convert.ToInt32(i));
		if (ids.Count() > 10)
			ids = ids.Take(10);
		var users = ids.Select(id => _userService.GetUser(id).Result).ToList();
		var pm = await _privateMessageService.Create(fullText, user, users);
		return RedirectToAction("View", new { id = pm.PMID });
	}

	public async Task<JsonResult> GetNames(string id)
	{
		var users = await _userService.SearchByName(id);
		var projection = users.Select(u => new { u.UserID, value = u.Name });
		return Json(projection);
	}

	[HttpPost]
	public async Task<ActionResult> ArchivePM(int id)
	{
		var user = _userRetrievalShim.GetUser();
		if (user == null)
			return StatusCode(403);
		var pm = await _privateMessageService.Get(id, user.UserID);
		if (await _privateMessageService.IsUserInPM(user.UserID, pm.PMID) == false)
			return StatusCode(403);
		await _privateMessageService.Archive(user, pm);
		return RedirectToAction("Index");
	}

	[HttpPost]
	public async Task<ActionResult> UnarchivePM(int id)
	{
		var user = _userRetrievalShim.GetUser();
		if (user == null)
			return StatusCode(403);
		var pm = await _privateMessageService.Get(id, user.UserID);
		if (await _privateMessageService.IsUserInPM(user.UserID, pm.PMID) == false)
			return StatusCode(403);
		await _privateMessageService.Unarchive(user, pm);
		return RedirectToAction("Archive");
	}

	public async Task<ContentResult> NewPMCount()
	{
		var user = _userRetrievalShim.GetUser();
		if (user == null)
			return Content(String.Empty);
		var count = await _privateMessageService.GetUnreadCount(user.UserID);
		return Content(count.ToString());
	}
}