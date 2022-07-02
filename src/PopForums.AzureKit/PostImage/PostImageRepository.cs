using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PopForums.Configuration;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.AzureKit.PostImage;

public class PostImageRepository : IPostImageRepository
{
	private readonly IConfig _config;
	private readonly ITenantService _tenantService;

	private static string _containerName = "postimage";

	public PostImageRepository(IConfig config, ITenantService tenantService)
	{
		_config = config;
		_tenantService = tenantService;
	}

	public async Task<string> Persist(byte[] bytes, string contentType)
	{
		var container = new BlobContainerClient(_config.QueueConnectionString, _containerName);
		await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
		var path = $"{_tenantService.GetTenant()}_{Guid.NewGuid()}";
		var blob = container.GetBlobClient(path);
		var binary = new BinaryData(bytes);
		await blob.UploadAsync(binary);
		await blob.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = contentType });
		return "http://127.0.0.1:10000/devstoreaccount1/" + _containerName + "/" + path;
	}
}