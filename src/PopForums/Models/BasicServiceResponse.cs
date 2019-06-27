namespace PopForums.Models
{
	public class BasicServiceResponse<T> where T : class
	{
		public bool IsSuccessful { get; set; }
		public string Message { get; set; }
		public T Data { get; set; }
		public string Redirect { get; set; }
	}
}
