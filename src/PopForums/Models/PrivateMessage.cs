namespace PopForums.Models;

public class PrivateMessage
{
	public int PMID { get; set; }
	public DateTime LastPostTime { get; set; }
	public JsonElement Users { get; set; }
	/// <summary>
	/// This property is populated only when getting lists of PM's.
	/// </summary>
	public DateTime LastViewDate { get; set; }

	public static string GetUserNames(PrivateMessage pm, int excludeUserID)
	{
		var users = pm.Users.Deserialize<UserNamePair[]>(new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
		if (users == null)
			return string.Empty;
		var names = new List<string>();
		foreach (var item in users)
		{
			if (item.UserID != excludeUserID)
				names.Add(item.Name as string);
		}
		var result = string.Join(", ", names);
		return result;
	}
	
	private class UserNamePair
	{
		public int UserID { get; set; }
		public string Name { get; set; }
	}
}