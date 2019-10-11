namespace PopForums.Models
{
	public class SearchIndexPayload
	{
		public int TopicID { get; set; }
		public string TenantID { get; set; }
		public bool IsForRemoval { get; set; }
	}
}