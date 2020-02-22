namespace PopForums.AzureKit.Redis
{
	public interface ICacheTelemetry
	{
		void Start();
		void End(string eventName, string key);
	}
}