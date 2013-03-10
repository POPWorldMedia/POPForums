using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
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
			var originalImage = new BitmapImage();
			originalImage.BeginInit();
			originalImage.StreamSource = new MemoryStream(bytes);
			originalImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
			originalImage.EndInit();
			int newHeight;
			int newWidth;
			CalculateResize(maxWidth, maxHeight, originalImage.PixelWidth, originalImage.PixelHeight, out newWidth, out newHeight);
			var image = Resize(bytes, newWidth, newHeight, qualityLevel);
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

		private byte[] Resize(byte[] bytes, int? width, int? height, int qualityLevel)
		{
			if (bytes == null)
				throw new Exception("Bytes parameter is null.");
			var image = new BitmapImage();
			image.BeginInit();
			if (width.HasValue)
				image.DecodePixelWidth = width.Value;
			if (height.HasValue)
				image.DecodePixelHeight = height.Value;
			image.StreamSource = new MemoryStream(bytes);
			image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
			image.EndInit();
			var transform = new TransformedBitmap();
			transform.BeginInit();
			transform.Source = image;
			transform.EndInit();
			return ToJpegBytes(transform, qualityLevel);
		}

		private byte[] ToJpegBytes(BitmapSource image, int qualityLevel)
		{
			if (image == null)
				throw new Exception("Image parameter is null.");
			var encoder = new JpegBitmapEncoder();
			var stream = new MemoryStream();
			encoder.Frames.Add(BitmapFrame.Create(image));
			encoder.QualityLevel = qualityLevel;
			encoder.Save(stream);
			var length = (int)stream.Length;
			var imageData = new byte[length];
			stream.Position = 0;
			stream.Read(imageData, 0, length);
			return imageData;
		}
	}
}