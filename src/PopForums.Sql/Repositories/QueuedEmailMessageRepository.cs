using System;
using Dapper;
using Newtonsoft.Json;
using PopForums.Email;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Sql.Repositories
{
	public class QueuedEmailMessageRepository : IQueuedEmailMessageRepository
	{
		public QueuedEmailMessageRepository(ISqlObjectFactory sqlObjectFactory, ITenantService tenantService)
		{
			_sqlObjectFactory = sqlObjectFactory;
			_tenantService = tenantService;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;
		private readonly ITenantService _tenantService;

		public void CreateMessage(QueuedEmailMessage message)
		{
			var id = 0;
			_sqlObjectFactory.GetConnection().Using(connection => 
				id = connection.QuerySingle<int>("INSERT INTO pf_QueuedEmailMessage (FromEmail, FromName, ToEmail, ToName, Subject, Body, HtmlBody, QueueTime) VALUES (@FromEmail, @FromName, @ToEmail, @ToName, @Subject, @Body, @HtmlBody, @QueueTime);SELECT CAST(SCOPE_IDENTITY() as int)", new { message.FromEmail, message.FromName, message.ToEmail, message.ToName, message.Subject, message.Body, message.HtmlBody, message.QueueTime }));
			if (id == 0)
				throw new Exception("MessageID was not returned from creation of a QueuedEmailMessage.");
			var tenantID = _tenantService.GetTenant();
			var payload = new EmailQueuePayload { MessageID = id, EmailQueuePayloadType = EmailQueuePayloadType.FullMessage, TenantID = tenantID };
			WriteMessageToEmailQueue(payload);
		}

		protected virtual void WriteMessageToEmailQueue(EmailQueuePayload payload)
		{
			var serializedPayload = JsonConvert.SerializeObject(payload);
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_EmailQueue (Payload) VALUES (@Payload)", new { Payload = serializedPayload }));
		}

		public void DeleteMessage(int messageID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_QueuedEmailMessage WHERE MessageID = @MessageID", new { MessageID = messageID }));
		}

		protected virtual EmailQueuePayload DequeueEmailQueuePayload()
		{
			string serializedPayload = null;
			var sql = @"WITH cte AS (
SELECT TOP(1) Payload
FROM pf_EmailQueue WITH (ROWLOCK, READPAST)
ORDER BY Id)
DELETE FROM cte
OUTPUT DELETED.Payload;";
			_sqlObjectFactory.GetConnection().Using(connection =>
				serializedPayload = connection.QuerySingleOrDefault<string>(sql));
			if (string.IsNullOrEmpty(serializedPayload))
				return null;
			var payload = JsonConvert.DeserializeObject<EmailQueuePayload>(serializedPayload);
			return payload;
		}

		public QueuedEmailMessage GetOldestQueuedEmailMessage()
		{
			var payload = DequeueEmailQueuePayload();
			if (payload == null)
				return null;
			if (payload.EmailQueuePayloadType != EmailQueuePayloadType.FullMessage)
				throw new NotImplementedException($"EmailQueuePayloadType {payload.EmailQueuePayloadType} not implemented.");
			QueuedEmailMessage message = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				message = connection.QuerySingleOrDefault<QueuedEmailMessage>("SELECT MessageID, FromEmail, FromName, ToEmail, ToName, Subject, Body, HtmlBody, QueueTime FROM pf_QueuedEmailMessage WHERE MessageID = @MessageID", new { payload.MessageID }));
			if (message == null)
				throw new Exception($"Queued email with MessageID {payload.MessageID} was not found.");
			return message;
		}
	}
}