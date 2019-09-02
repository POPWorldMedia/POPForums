using System;
using System.Threading.Tasks;
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

		public async Task<int> CreateMessage(QueuedEmailMessage message)
		{
			Task<int> id = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				id = connection.QuerySingleAsync<int>("INSERT INTO pf_QueuedEmailMessage (FromEmail, FromName, ToEmail, ToName, Subject, Body, HtmlBody, QueueTime) VALUES (@FromEmail, @FromName, @ToEmail, @ToName, @Subject, @Body, @HtmlBody, @QueueTime);SELECT CAST(SCOPE_IDENTITY() as int)", new { message.FromEmail, message.FromName, message.ToEmail, message.ToName, message.Subject, message.Body, message.HtmlBody, message.QueueTime }));
			if (id.Result == 0)
				throw new Exception("MessageID was not returned from creation of a QueuedEmailMessage.");
			return await id;
		}

		public async Task DeleteMessage(int messageID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_QueuedEmailMessage WHERE MessageID = @MessageID", new { MessageID = messageID }));
		}

		public async Task<QueuedEmailMessage> GetMessage(int messageID)
		{
			Task<QueuedEmailMessage> message = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				message = connection.QuerySingleOrDefaultAsync<QueuedEmailMessage>("SELECT MessageID, FromEmail, FromName, ToEmail, ToName, Subject, Body, HtmlBody, QueueTime FROM pf_QueuedEmailMessage WHERE MessageID = @MessageID", new {messageID}));
			return await message;
		}
	}
}