namespace PopForums.Repositories;

public interface IUserAvatarRepository
{
	[Obsolete("Use GetImageData(int) instead.")]
	Task<byte[]> GetImageData(int userAvatarID);
	Task<IStreamResponse> GetImageStream(int userAvatarID);
	Task<List<int>> GetUserAvatarIDs(int userID);
	Task<int> SaveNewAvatar(int userID, byte[] imageData, DateTime timeStamp);
	Task DeleteAvatarsByUserID(int userID);
	Task<DateTime?> GetLastModificationDate(int userAvatarID);
}