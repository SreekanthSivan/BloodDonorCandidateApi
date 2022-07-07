using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CandidateFunction
{
    public static class UpdateCandidateInfoFunction
    {
        [FunctionName("UpdateCandidateInfoFunction")]
        public static async Task Run([QueueTrigger("donormessagequeue", Connection = "connectionStr")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
            try
            {
                Candidate item = Newtonsoft.Json.JsonConvert.DeserializeObject<Candidate>(myQueueItem);
                if (item != null && item.id > 0)
                {
                    var address = string.IsNullOrEmpty(item.address) ? item.location : item.address;
                    var str = Environment.GetEnvironmentVariable("sqldb_connection");
                    using (SqlConnection conn = new SqlConnection(str))
                    {
                        conn.Open();
                        var text = "UPDATE DCandidates " +
                                "SET [address] ='" + address + "' WHERE id = " + item.id;
                        log.LogInformation("Query -> " + text);
                        using (SqlCommand cmd = new SqlCommand(text, conn))
                        {
                            var rows = await cmd.ExecuteNonQueryAsync();
                            
                            log.LogInformation($"{rows} rows were updated");
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
            }
        }

    }
}
