using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http;
using PopForums.Web;

namespace PopForums.BackgroundWorker
{
	public class MonitorController : ApiController
	{
		public IHttpActionResult Get()
		{
			var services = PopForumsActivation.ApplicationServices.Select(x => new ServiceStatus { Name = x.Name, IsRunning = x.IsRunning, LastExecutionTime = x.LastExecutionTime.HasValue ? x.LastExecutionTime.Value : DateTime.MinValue }).ToList();
			return Ok(services);
		}

		[DataContract]
		public class ServiceStatus
		{
			[DataMember]
			public string Name { get; set; }
			[DataMember]
			public bool IsRunning { get; set; }
			[DataMember]
			public DateTime LastExecutionTime { get; set; }
		}
	}
}