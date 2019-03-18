namespace PopForums.Email
{
	public class EmailQueuePayload
	{
		public int MessageID { get; set; }
		public EmailQueuePayloadType EmailQueuePayloadType { get; set; }
		public string ToEmail { get; set; }
		public string ToName { get; set; }
		public string TenantID { get; set; }
	}
}