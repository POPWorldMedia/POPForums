using System;
using System.Net.Http;
using System.Net.Http.Json;
using PopForums.Configuration;
using PopForums.Messaging;
using PopForums.Messaging.Models;

namespace PopForums.AzureKit.Functions;

public class NotificationTunnel : INotificationTunnel
{
	private readonly IConfig _config;

	public NotificationTunnel(IConfig config)
	{
		_config = config;
	}

	public void SendNotificationForUserAward(string title, int userID, string tenantID)
	{
		var payload = new AwardPayload
		{
			Title = title,
			UserID = userID,
			TenantID = tenantID
		};
		var url = _config.WebAppUrlAndArea + "/api/notifyaward";
		var httpClient = new HttpClient();
		var result = httpClient.PostAsJsonAsync(url, payload).Result;
		if (!result.IsSuccessStatusCode)
			throw new Exception($"Problem sending message over notification tunnel: HTTP {result.StatusCode}");
	}
}