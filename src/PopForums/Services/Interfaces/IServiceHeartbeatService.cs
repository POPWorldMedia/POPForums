namespace PopForums.Services.Interfaces;

public interface IServiceHeartbeatService
{
    Task RecordHeartbeat(string serviceName, string machineName);

    Task<List<ServiceHeartbeat>> GetAll();

    Task ClearAll();
}
