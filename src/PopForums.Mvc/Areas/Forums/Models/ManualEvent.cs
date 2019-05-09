namespace PopForums.Mvc.Areas.Forums.Models
{
	public class ManualEvent
	{
		public int UserID { get; set; }
		public string Message { get; set; }
		public int? Points { get; set; }
		public string EventDefinitionID { get; set; }
	}
}