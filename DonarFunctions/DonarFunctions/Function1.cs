using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DonarFunctions
{
    public class Function1
    {
        [FunctionName("QueueTrigger")]
        public static void QueueTrigger(
         [QueueTrigger("donaruserdata", Connection ="QueueStorageConString")] string myQueueItem,
         ILogger log)
        {
            log.LogInformation($"C# function processed: {myQueueItem}");
        }
    }
}
