using System;

namespace PopForums.Models
{
	/// <summary>
	/// A generic container for wrapping the response of external calls for consumption by internal service.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Response<T> where T : class
	{
		/// <summary>
		/// Creates a generic Response with IsValid set to true and no debug information or exception.
		/// </summary>
		/// <param name="data">The strongly typed result to be consumed by the caller.</param>
		public Response(T data)
		{
			Data = data;
			IsValid = true;
		}

		/// <summary>
		/// Creates a generic response with all fields set.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="isValid">Default is false.</param>
		/// <param name="exception">Default is null.</param>
		/// <param name="debugInfo">Default is null.</param>
		public Response(T data, bool isValid = false, Exception exception = null, string debugInfo = null)
		{
			Data = data;
			IsValid = isValid;
			Exception = exception;
			DebugInfo = debugInfo;
		}
		
		public T Data { get; }
		public bool IsValid { get; }
		public Exception Exception { get; }
		public string DebugInfo { get; }
	}
}