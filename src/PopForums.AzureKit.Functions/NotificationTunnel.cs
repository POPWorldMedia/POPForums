using System;
using System.Net.Http;
using System.Net.Http.Json;
using PopForums.Configuration;
using PopForums.Extensions;
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
		SendMessage(url, payload);
	}

	public void SendNotificationForReply(string postName, string title, int topicID, int userID, string tenantID)
	{
		var payload = new ReplyPayload
		{
			PostName = postName,
			Title = title,
			TopicID = topicID,
			UserID = userID,
			TenantID = tenantID
		};
		var url = _config.WebAppUrlAndArea + "/api/notifyreply";
		SendMessage(url, payload);
	}

	private void SendMessage(string url, object payload)
	{
		var authHash = _config.QueueConnectionString.GetSHA256Hash();
		using var httpClient = new HttpClient();
		httpClient.DefaultRequestHeaders.Add(Messaging.NotificationTunnel.HeaderName, authHash);
		var result = httpClient.PostAsJsonAsync(url, payload).Result;
		if (!result.IsSuccessStatusCode)
			throw new Exception($"Problem sending message over notification tunnel: HTTP {result.StatusCode}");
	}
}