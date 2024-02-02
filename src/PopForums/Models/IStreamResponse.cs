namespace PopForums.Models;

/// <summary>
/// This interface is used to pass a Stream to send to the client. It implements IDisposable to allow for cleanup
/// of the Stream and any other unmanaged resources (like the database connections, for example). Because using
/// statements would fall out of scope when a controller action returns, the Stream would be disposed before it
/// could be used. This interface allows for the Stream to be used and then disposed of when the client is done by
/// registering it with the HttpResponse RegisterForDispose method.
/// </summary>
public interface IStreamResponse : IDisposable
{
	Stream Stream { get; }
}