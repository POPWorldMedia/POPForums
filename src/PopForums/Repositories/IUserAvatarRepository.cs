using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PopForums.Repositories
{
	public interface IUserAvatarRepository
	{
		Task<byte[]> GetImageData(int userAvatarID);
		Task<List<int>> GetUserAvatarIDs(int userID);
		Task<int> SaveNewAvatar(int userID, byte[] imageData, DateTime timeStamp);
		Task DeleteAvatarsByUserID(int userID);
		Task<DateTime?> GetLastModificationDate(int userAvatarID);
	}
}
