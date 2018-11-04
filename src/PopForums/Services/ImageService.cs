using System;
using System.Collections.Generic;
using System.IO;
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
		bool? IsUserImageApproved(int userImageID);
		byte[] GetAvatarImageData(int userAvatarID);
		byte[] GetUserImageData(int userImageID);
		DateTime? GetAvatarImageLastModification(int userAvatarID);
		DateTime? GetUserImageLastModifcation(int userImageID);
		byte[] ConstrainResize(byte[] bytes, int maxWidth, int maxHeight, int qualityLevel);
		List<UserImage> GetUnapprovedUserImages();
		void ApproveUserImage(int userImageID);
		void DeleteUserImage(int userImageID);
		UserImage GetUserImage(int userImageID);
	}

	public class ImageService : IImageService
	{
		public ImageService(IUserAvatarRepository userAvatarRepository, IUserImageRepository userImageRepository, IProfileService profileService)
		{
			_userAvatarRepository = userAvatarRepository;
			_userImageRepository = userImageRepository;
			_profileService = profileService;
		}

		private readonly IUserAvatarRepository _userAvatarRepository;
		private readonly IUserImageRepository _userImageRepository;
		private readonly IProfileService _profileService;

		public bool? IsUserImageApproved(int userImageID)
		{
			return _userImageRepository.IsUserImageApproved(userImageID);
		}

		public UserImage GetUserImage(int userImageID)
		{
			return _userImageRepository.Get(userImageID);
		}

		public void ApproveUserImage(int userImageID)
		{
			_userImageRepository.ApproveUserImage(userImageID);
		}

		public void DeleteUserImage(int userImageID)
		{
			var userImage = _userImageRepository.Get(userImageID);
			if (userImage != null)
				_profileService.SetCurrentImageIDToNull(userImage.UserID);
			_userImageRepository.DeleteUserImage(userImageID);
		}

		public byte[] GetAvatarImageData(int userAvatarID)
		{
			return _userAvatarRepository.GetImageData(userAvatarID);
		}

		public byte[] GetUserImageData(int userImageID)
		{
			return _userImageRepository.GetImageData(userImageID);
		}

		public List<UserImage> GetUnapprovedUserImages()
		{
			return _userImageRepository.GetUnapprovedUserImages();
		}

		public DateTime? GetAvatarImageLastModification(int userAvatarID)
		{
			return _userAvatarRepository.GetLastModificationDate(userAvatarID);
		}

		public DateTime? GetUserImageLastModifcation(int userImageID)
		{
			return _userImageRepository.GetLastModificationDate(userImageID);
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
	}
}