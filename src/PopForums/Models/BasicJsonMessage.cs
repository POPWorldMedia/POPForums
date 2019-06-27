namespace PopForums.Models
{
	public class BasicJsonMessage
	{
		public bool Result { get; set; }
		public string Message { get; set; }
		public object Data { get; set; }
		public string Redirect { get; set; }
	}
}