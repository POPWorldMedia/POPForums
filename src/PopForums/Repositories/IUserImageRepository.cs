using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IUserImageRepository
	{
		Task<byte[]> GetImageData(int userImageID);
		Task<List<UserImage>> GetUserImages(int userID);
		Task<int> SaveNewImage(int userID, int sortOrder, bool isApproved, byte[] imageData, DateTime timeStamp);
		Task DeleteImagesByUserID(int userID);
		Task<DateTime?> GetLastModificationDate(int userImageID);
		Task<List<UserImage>> GetUnapprovedUserImages();
		Task<bool?> IsUserImageApproved(int userImageID);
		Task ApproveUserImage(int userImageID);
		Task DeleteUserImage(int userImageID);
		Task<UserImage> Get(int userImageID);
	}
}
