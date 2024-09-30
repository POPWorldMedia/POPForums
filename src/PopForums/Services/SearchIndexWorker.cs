namespace PopForums.Services;

public interface ISearchIndexWorker
{
	void Execute();
}

public class SearchIndexWorker(IErrorLog errorLog, ISearchIndexSubsystem searchIndexSubsystem, ISearchService searchService) : ISearchIndexWorker
{
	public async void Execute()
	{
		try
		{
			var payload = await searchService.GetNextTopicForIndexing();
			if (payload == null)
				return;
			searchIndexSubsystem.DoIndex(payload.TopicID, payload.TenantID, payload.IsForRemoval);
		}
		catch (Exception exc)
		{
			errorLog.Log(exc, ErrorSeverity.Error);
		}
	}
}