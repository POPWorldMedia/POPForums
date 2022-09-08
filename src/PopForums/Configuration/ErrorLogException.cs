namespace PopForums.Configuration;

public class ErrorLogException : Exception
{
	public ErrorLogException(string message) : base(message)
	{
		
	}

	public override string Message => "Can't log exception: " + base.Message;
}