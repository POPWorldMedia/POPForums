using PopForums.Messaging.Models;

namespace PopForums.Mvc.Areas.Forums.Controllers;

[Area("Forums")]
[ApiController]
public class ApiController : Controller
{
	private readonly IUserRetrievalShim _userRetrievalShim;
	private readonly INotificationAdapter _notificationAdapter;
	private readonly INotificationManager _notificationManager;
	private readonly IConfig _config;

	public ApiController(IUserRetrievalShim userRetrievalShim, INotificationAdapter notificationAdapter, INotificationManager notificationManager, IConfig config)
	{
		_userRetrievalShim = userRetrievalShim;
		_notificationAdapter = notificationAdapter;
		_notificationManager = notificationManager;
		_config = config;
	}
	
	[HttpPost("/Forums/Api/NotifyAward")]
	public async Task<IActionResult> NotifyAward(AwardPayload awardPayload)
	{
		var hash = _config.QueueConnectionString.GetSHA256Hash();
		var result = HttpContext.Request.Headers.TryGetValue(NotificationTunnel.HeaderName, out var headerValue);
		if (headerValue != hash)
			return Unauthorized();
		if (awardPayload == null)
			return BadRequest();
		await _notificationAdapter.Award(awardPayload.Title, awardPayload.UserID, awardPayload.TenantID);
		return Ok();
	}

	[HttpGet("/Forums/Api/Notifications")]
	public async Task<IActionResult> Notifications()
	{
		var user = _userRetrievalShim.GetUser();
		if (user == null)
			return Forbid();
		var notifications = await _notificationManager.GetNotifications(user.UserID);
		return Json(notifications);
	}
}