using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Azure.Data.Tables;

namespace PopForums.AzureKit.Logging;

public class ErrorLogRepository : IErrorLogRepository
{
	private readonly ITenantService _tenantService;
	private readonly IConfig _config;
	private const string ErrorTableName = "pferrorlog";
	private TableClient _tableClient;

	public ErrorLogRepository(ITenantService tenantService, IConfig config)
	{
		_tenantService = tenantService;
		_config = config;
	}

	public async Task<ErrorLogEntry> Create(DateTime timeStamp, string message, string stackTrace, string data, ErrorSeverity severity)
	{
		var tableClient = await GetTableClient();
		var errorLog = new ErrorLogEntry
		{
			ErrorID = 0,
			TimeStamp = timeStamp,
			Message = message,
			StackTrace = stackTrace,
			Data = data,
			Severity = severity
		};
		var tenantID = _tenantService.GetTenant();
		var rowKey = DateTime.UtcNow.ToString("s") + Random.Shared.Next(100000,999999);
		var entry = new TableEntity($"{DateTime.UtcNow.Year}-{DateTime.UtcNow.DayOfYear.ToString("D3")}", rowKey)
			{
				{ "TenantID", tenantID },
				{ "Message", errorLog.Message },
				{ "StackTrace", errorLog.StackTrace },
				{ "Data", errorLog.Data },
				{ "Severity", errorLog.Severity.ToString() }
			};
		await tableClient.AddEntityAsync(entry);
		return errorLog;
	}

	private async Task<TableClient> GetTableClient()
	{
		if (_tableClient != null)
			return _tableClient;
		var tableClient = new TableClient(_config.StorageConnectionString, ErrorTableName);
		await tableClient.CreateIfNotExistsAsync();
		_tableClient = tableClient;
		return tableClient;
	}

	public Task<int> GetErrorCount()
	{
		return Task.FromResult(1);
	}

	public Task<List<ErrorLogEntry>> GetErrors(int startRow, int pageSize)
	{
		var entry = new ErrorLogEntry {Message = "Check table storage for errors."};
		var list = new List<ErrorLogEntry> {entry};
		return Task.FromResult(list);
	}

	public Task DeleteError(int errorID)
	{
		throw new NotImplementedException();
	}

	public async Task DeleteAllErrors()
	{
		var tableClient = new TableClient(_config.StorageConnectionString, ErrorTableName);
		await tableClient.DeleteAsync();
	}
}