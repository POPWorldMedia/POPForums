namespace PopForums.Mvc.Areas.Forums.Controllers;

[Area("Forums")]
public class IgnoreController(IIgnoreService ignoreService, IUserRetrievalShim userRetrievalShim) : Controller
{
	public static readonly string Name = "Ignore";
	
	[HttpPost]
	public async Task<IActionResult> Add(int userID, int postID)
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return Forbid();
		await ignoreService.AddIgnore(user.UserID, userID);
		return RedirectToAction("PostLink", "Forum", new { id = postID });
	}
	
	[HttpPost]
	public async Task<IActionResult> Remove(int userID, int postID)
	{
		var user = userRetrievalShim.GetUser();
		if (user == null)
			return Forbid();
		await ignoreService.DeleteIgnore(user.UserID, userID);
		return RedirectToAction("PostLink", "Forum", new { id = postID });
	}
}