using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PopForums.Configuration;
using PopForums.Models;
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

	public async Task<PostImagePersistPayload> Persist(byte[] bytes, string contentType)
	{
		var container = new BlobContainerClient(_config.StorageConnectionString, _containerName);
		await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
		var tenant = _tenantService.GetTenant();
		if (string.IsNullOrWhiteSpace(tenant))
			tenant = "_";
		var id = Guid.NewGuid().ToString();
		var path = $"{tenant}/{id}";
		var blob = container.GetBlobClient(path);
		var binary = new BinaryData(bytes);
		await blob.UploadAsync(binary);
		await blob.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = contentType });
		var url = _config.BaseImageBlobUrl + "/" + _containerName + "/" + path;
		var payload = new PostImagePersistPayload { Url = url, ID = id };
		return payload;
	}

	public async Task DeletePostImageData(string id, string tenantID)
	{
		var container = new BlobContainerClient(_config.StorageConnectionString, _containerName);
		await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
		var tenant = _tenantService.GetTenant();
		if (string.IsNullOrWhiteSpace(tenant))
			tenant = "_";
		var path = $"{tenant}/{id}";
		await container.DeleteBlobAsync(path, DeleteSnapshotsOption.IncludeSnapshots);
	}

	// The next two methods are not used when fetching images from Azure storage. The
	// default SQL implementation does use these.

	public Task<Models.PostImage> GetWithoutData(string id)
	{
		throw new NotImplementedException();
	}

	public Task<Models.PostImage> Get(string id)
	{
		throw new NotImplementedException();
	}
}