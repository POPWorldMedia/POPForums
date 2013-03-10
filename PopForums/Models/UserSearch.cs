namespace PopForums.Models
{
	public class UserSearch
	{
		public string SearchText { get; set; }
		public UserSearchType SearchType { get; set; }

		public enum UserSearchType
		{
			Name, Email, Role
		}
	}
}