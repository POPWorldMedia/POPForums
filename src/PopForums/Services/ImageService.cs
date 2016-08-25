using System;
using System.Collections.Generic;
using System.IO;
using ImageProcessorCore;
using PopForums.Models;
using PopForums.Repositories;

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
			var stream = new MemoryStream(bytes);
			var originalImage = new Image(stream);
			int newHeight;
			int newWidth;
			CalculateResize(maxWidth, maxHeight, originalImage.Width, originalImage.Height, out newWidth, out newHeight);
			var image = Resize(bytes, newWidth, newHeight, qualityLevel);
			stream.Dispose();
			return image;
		}

		private void CalculateResize(int maxWidth, int maxHeight, int originalWidth, int originalHeight, out int newWidth, out int newHeight)
		{
			newWidth = originalWidth;
			newHeight = originalHeight;
			if (originalHeight < maxHeight && originalWidth < maxWidth)
				return;
			var xRatio = (double)maxWidth / originalWidth;
			var yRatio = (double)maxHeight / originalHeight;
			var ratio = Math.Min(xRatio, yRatio);
			newWidth = (int)(originalWidth * ratio);
			newHeight = (int)(originalHeight * ratio);
		}

		private byte[] Resize(byte[] bytes, int width, int height, int qualityLevel)
		{
			if (bytes == null)
				throw new Exception("Bytes parameter is null.");
			var output = new MemoryStream();
			using (var stream = new MemoryStream(bytes))
			{
				var image = new Image(stream);
				image.Resize(width, height)
					.SaveAsJpeg(output, qualityLevel);
			}
			return output.ToArray();
		}
	}
}