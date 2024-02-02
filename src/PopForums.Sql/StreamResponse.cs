namespace PopForums.Sql;

public class StreamResponse(Stream stream, SqlConnection connection, SqlDataReader reader) : IStreamResponse
{
	public Stream Stream => stream;
	
	public void Dispose()
	{
		reader.Close();
		connection.Close();
		stream.Close();
        
		reader.Dispose();
		connection.Dispose();
		stream.Dispose();
	}
}