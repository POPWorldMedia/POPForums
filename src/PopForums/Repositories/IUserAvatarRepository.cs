using System;
using System.Collections.Generic;

namespace PopForums.Repositories
{
	public interface IUserAvatarRepository
	{
		byte[] GetImageData(int userAvatarID);
		List<int> GetUserAvatarIDs(int userID);
		int SaveNewAvatar(int userID, byte[] imageData, DateTime timeStamp);
		void DeleteAvatarsByUserID(int userID);
		DateTime? GetLastModificationDate(int userAvatarID);
	}
}
