namespace PopForums.Models;

public class BasicServiceResponse<T> where T : class
{
	public bool IsSuccessful { get; set; }
	public string Message { get; set; }
	public T Data { get; set; }
	public string Redirect { get; set; }
	
	public static BasicServiceResponse<T> Success(T t)
	{
		return new BasicServiceResponse<T>
		{
			IsSuccessful = true,
			Data = t
		};
	}

	public static BasicServiceResponse<T> Failed(string message)
	{
		return new BasicServiceResponse<T>
		{
			IsSuccessful = false,
			Message = message
		};
	}
}