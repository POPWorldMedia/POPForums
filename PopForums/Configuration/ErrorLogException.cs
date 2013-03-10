using System;

namespace PopForums.Configuration
{
	[Serializable]
	public class ErrorLogException : Exception
	{
		public ErrorLogException(string message) : base(message)
		{
			
		}

		public override string Message
		{
			get
			{
				return "Can't log exception: " + base.Message;
			}
		}
	}
}