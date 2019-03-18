using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace PopForums.AzureKit.Functions
{
    public static class EmailProcessor
    {
        [FunctionName("EmailProcessor")]
        public static void Run([QueueTrigger("pfemailqueue", Connection = "")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
