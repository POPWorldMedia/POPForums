using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace PopForums.Services
{
	public interface IImageService
	{
		Task<bool?> IsUserImageApproved(int userImageID);
		Task<byte[]> GetAvatarImageData(int userAvatarID);
		Task<byte[]> GetUserImageData(int userImageID);
		Task<DateTime?> GetAvatarImageLastModification(int userAvatarID);
		Task<DateTime?> GetUserImageLastModifcation(int userImageID);
		byte[] ConstrainResize(byte[] bytes, int maxWidth, int maxHeight, int qualityLevel);
		Task<List<UserImage>> GetUnapprovedUserImages();
		Task ApproveUserImage(int userImageID);
		Task DeleteUserImage(int userImageID);
		Task<UserImage> GetUserImage(int userImageID);
		Task<UserImageApprovalContainer> GetUnapprovedUserImageContainer();
	}

	public class ImageService : IImageService
	{
		public ImageService(IUserAvatarRepository userAvatarRepository, IUserImageRepository userImageRepository, IProfileService profileService, IUserRepository userRepository, ISettingsManager settingsManager)
		{
			_userAvatarRepository = userAvatarRepository;
			_userImageRepository = userImageRepository;
			_profileService = profileService;
			_userRepository = userRepository;
			_settingsManager = settingsManager;
		}

		private readonly IUserAvatarRepository _userAvatarRepository;
		private readonly IUserImageRepository _userImageRepository;
		private readonly IProfileService _profileService;
		private readonly IUserRepository _userRepository;
		private readonly ISettingsManager _settingsManager;

		public async Task<bool?> IsUserImageApproved(int userImageID)
		{
			return await _userImageRepository.IsUserImageApproved(userImageID);
		}

		public async Task<UserImage> GetUserImage(int userImageID)
		{
			return await _userImageRepository.Get(userImageID);
		}

		public async Task ApproveUserImage(int userImageID)
		{
			await _userImageRepository.ApproveUserImage(userImageID);
		}

		public async Task DeleteUserImage(int userImageID)
		{
			var userImage = await _userImageRepository.Get(userImageID);
			if (userImage != null)
				await _profileService.SetCurrentImageIDToNull(userImage.UserID);
			await _userImageRepository.DeleteUserImage(userImageID);
		}

		public async Task<byte[]> GetAvatarImageData(int userAvatarID)
		{
			return await _userAvatarRepository.GetImageData(userAvatarID);
		}

		public async Task<byte[]> GetUserImageData(int userImageID)
		{
			return await _userImageRepository.GetImageData(userImageID);
		}

		public async Task<List<UserImage>> GetUnapprovedUserImages()
		{
			return await _userImageRepository.GetUnapprovedUserImages();
		}

		public async Task<DateTime?> GetAvatarImageLastModification(int userAvatarID)
		{
			return await _userAvatarRepository.GetLastModificationDate(userAvatarID);
		}

		public async Task<DateTime?> GetUserImageLastModifcation(int userImageID)
		{
			return await _userImageRepository.GetLastModificationDate(userImageID);
		}

		public byte[] ConstrainResize(byte[] bytes, int maxWidth, int maxHeight, int qualityLevel)
		{
			if (bytes == null)
				throw new Exception("Bytes parameter is null.");
			using (var stream = new MemoryStream(bytes))
			using (var image = Image.Load<Rgba32>(stream))
			using (var output = new MemoryStream())
			{
				var options = new ResizeOptions
				{
					Size = new Size(maxWidth, maxHeight),
					Mode = ResizeMode.Max
				};
				image.Mutate(x => x
					.Resize(options)
					.GaussianSharpen(0.5f));
				image.Save(output, new JpegEncoder { Quality = qualityLevel });
				return output.ToArray();
			}
		}

		public async Task<UserImageApprovalContainer> GetUnapprovedUserImageContainer()
		{
			var isNewUserImageApproved = _settingsManager.Current.IsNewUserImageApproved;
			var dictionary = new Dictionary<UserImage, User>();
			var unapprovedImages = await GetUnapprovedUserImages();
			var users = await _userRepository.GetUsersFromIDs(unapprovedImages.Select(i => i.UserID).ToList());
			var container = new UserImageApprovalContainer { Unapproved = new List<UserImagePair>(), IsNewUserImageApproved = isNewUserImageApproved };
			foreach (var image in unapprovedImages)
			{
				container.Unapproved.Add(new UserImagePair { User = users.Single(u => u.UserID == image.UserID), UserImage = image });
				dictionary.Add(image, users.Single(u => u.UserID == image.UserID));
			}
			return container;
		}
	}
}