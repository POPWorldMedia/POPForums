using System;
using Dapper;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class QueuedEmailMessageRepository : IQueuedEmailMessageRepository
	{
		public QueuedEmailMessageRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public int CreateMessage(QueuedEmailMessage message)
		{
			var id = 0;
			_sqlObjectFactory.GetConnection().Using(connection => 
				id = connection.QuerySingle<int>("INSERT INTO pf_QueuedEmailMessage (FromEmail, FromName, ToEmail, ToName, Subject, Body, HtmlBody, QueueTime) VALUES (@FromEmail, @FromName, @ToEmail, @ToName, @Subject, @Body, @HtmlBody, @QueueTime);SELECT CAST(SCOPE_IDENTITY() as int)", new { message.FromEmail, message.FromName, message.ToEmail, message.ToName, message.Subject, message.Body, message.HtmlBody, message.QueueTime }));
			if (id == 0)
				throw new Exception("MessageID was not returned from creation of a QueuedEmailMessage.");
			return id;
		}

		public void DeleteMessage(int messageID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_QueuedEmailMessage WHERE MessageID = @MessageID", new { MessageID = messageID }));
		}

		public QueuedEmailMessage GetMessage(int messageID)
		{
			QueuedEmailMessage message = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				message = connection.QuerySingleOrDefault<QueuedEmailMessage>("SELECT MessageID, FromEmail, FromName, ToEmail, ToName, Subject, Body, HtmlBody, QueueTime FROM pf_QueuedEmailMessage WHERE MessageID = @MessageID", new {messageID}));
			return message;
		}
	}
}