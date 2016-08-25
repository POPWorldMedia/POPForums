using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IUserImageRepository
	{
		byte[] GetImageData(int userImageID);
		List<UserImage> GetUserImages(int userID);
		int SaveNewImage(int userID, int sortOrder, bool isApproved, byte[] imageData, DateTime timeStamp);
		void DeleteImagesByUserID(int userID);
		DateTime? GetLastModificationDate(int userImageID);
		List<UserImage> GetUnapprovedUserImages();
		bool? IsUserImageApproved(int userImageID);
		void ApproveUserImage(int userImageID);
		void DeleteUserImage(int userImageID);
		UserImage Get(int userImageID);
	}
}
