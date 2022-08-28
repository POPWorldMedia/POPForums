namespace PopForums.Models;

public class ClientPrivateMessagePost
{
	public int PMPostID { get; set; }
	public int UserID { get; set; }
	public string Name { get; set; }
	public string PostTime { get; set; }
	public string FullText { get; set; }

	public static ClientPrivateMessagePost[] MapForClient(List<PrivateMessagePost> posts)
	{
		var messages = posts.Select(x => new ClientPrivateMessagePost { PMPostID = x.PMPostID, UserID = x.UserID, Name = x.Name, PostTime = x.PostTime.ToString("o"), FullText = x.FullText }).ToArray();
		return messages;
	}

	public static ClientPrivateMessagePost MapForClient(PrivateMessagePost post)
	{
		var message = new ClientPrivateMessagePost { PMPostID = post.PMPostID, UserID = post.UserID, Name = post.Name, PostTime = post.PostTime.ToString("o"), FullText = post.FullText };
		return message;
	}
}