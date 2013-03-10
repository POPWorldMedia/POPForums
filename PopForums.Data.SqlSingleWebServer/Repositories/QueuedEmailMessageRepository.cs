using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Data.SqlSingleWebServer.Repositories
{
	public class QueuedEmailMessageRepository : IQueuedEmailMessageRepository
	{
		public QueuedEmailMessageRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public void CreateMessage(QueuedEmailMessage message)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("INSERT INTO pf_QueuedEmailMessage (FromEmail, FromName, ToEmail, ToName, Subject, Body, HtmlBody, QueueTime) VALUES (@FromEmail, @FromName, @ToEmail, @ToName, @Subject, @Body, @HtmlBody, @QueueTime)")
					.AddParameter("@FromEmail", message.FromEmail)
					.AddParameter("@FromName", message.FromName)
					.AddParameter("@ToEmail", message.ToEmail)
					.AddParameter("@ToName", message.ToName)
					.AddParameter("@Subject", message.Subject)
					.AddParameter("@Body", message.Body)
					.AddParameter("@HtmlBody", message.HtmlBody.GetObjectOrDbNull())
					.AddParameter("@QueueTime", message.QueueTime)
					.ExecuteNonQuery());
		}

		public void DeleteMessage(int messageID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("DELETE FROM pf_QueuedEmailMessage WHERE MessageID = @MessageID")
					.AddParameter("@MessageID", messageID)
					.ExecuteNonQuery());
		}

		public QueuedEmailMessage GetOldestQueuedEmailMessage()
		{
			QueuedEmailMessage message = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT TOP 1 MessageID, FromEmail, FromName, ToEmail, ToName, Subject, Body, HtmlBody, QueueTime FROM pf_QueuedEmailMessage ORDER BY QueueTime")
					.ExecuteReader()
					.ReadOne(r => message = new QueuedEmailMessage
												{
													MessageID = r.GetInt32(0),
													FromEmail = r.GetString(1),
													FromName = r.GetString(2),
													ToEmail = r.GetString(3),
													ToName = r.GetString(4),
													Subject = r.GetString(5),
													Body = r.GetString(6),
													HtmlBody = r.NullStringDbHelper(7),
													QueueTime = r.GetDateTime(8)
												}));
			return message;
		}
	}
}