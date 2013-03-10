using System;
using System.Collections.Generic;
using PopForums.Models;

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
}
