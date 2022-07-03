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
		var container = new BlobContainerClient(_config.StorageConnectionString, _containerName);
		await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
		var tenant = _tenantService.GetTenant();
		if (string.IsNullOrWhiteSpace(tenant))
			tenant = "_";
		var path = $"{tenant}/{Guid.NewGuid()}";
		var blob = container.GetBlobClient(path);
		var binary = new BinaryData(bytes);
		await blob.UploadAsync(binary);
		await blob.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = contentType });
		return _config.BaseImageBlobUrl + "/" + _containerName + "/" + path;
	}

	public Task<Models.PostImage> GetWithoutData(string id)
	{
		throw new NotImplementedException();
	}

	public Task<Models.PostImage> Get(string id)
	{
		throw new NotImplementedException();
	}
}