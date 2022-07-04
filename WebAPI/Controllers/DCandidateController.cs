using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using WebAPI.Models;
using WebCandidateAPI.Interfaces;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DCandidateController : ControllerBase
    {
        private readonly DonationDBContext _context;
        private readonly ILogger<DCandidateController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IKeyVaultManager _secretManager;

        public DCandidateController(DonationDBContext context, ILogger<DCandidateController> logger, IConfiguration configuration, 
            IKeyVaultManager secretManager)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _secretManager = secretManager;
        }

        // GET: api/DCandidate
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DCandidate>>> GetDCandidates()
        {
            IList<DCandidate> result = new List<DCandidate>();
            try
            {
                result = await _context.DCandidates.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, ex, ex.Message);
            }
            return new ActionResult<IEnumerable<DCandidate>>(result);
        }

        // GET: api/DCandidate/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DCandidate>> GetDCandidate(int id)
        {
            var dCandidate = await _context.DCandidates.FindAsync(id);

            if (dCandidate == null)
            {
                return NotFound();
            }

            return dCandidate;
        }

        // PUT: api/DCandidate/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDCandidate(int id, DCandidate dCandidate)
        {
            dCandidate.id = id;

            _context.Entry(dCandidate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, ex, "");
                if (!DCandidateExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/DCandidate
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<DCandidate>> PostDCandidate(DCandidate dCandidate)
        {
            try
            {
                _context.DCandidates.Add(dCandidate);
                await _context.SaveChangesAsync();
                var a = CreatedAtAction("GetDCandidate", new { id = dCandidate.id }, dCandidate);
                //await pushDonorCreatedMessage(dCandidate.fullName, dCandidate.email);
                await pushDonorCreatedMessage(Newtonsoft.Json.JsonConvert.SerializeObject(a));
                return a;
            }
            catch(Exception ex)
            {
                _logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, ex, ex.Message);
                throw (ex as Exception);
            }
        }

        // DELETE: api/DCandidate/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<DCandidate>> DeleteDCandidate(int id)
        {
            var dCandidate = await _context.DCandidates.FindAsync(id);
            if (dCandidate == null)
            {
                return NotFound();
            }

            _context.DCandidates.Remove(dCandidate);
            await _context.SaveChangesAsync();

            return dCandidate;
        }

        private bool DCandidateExists(int id)
        {
            return _context.DCandidates.Any(e => e.id == id);
        }

        private async Task<bool> pushDonorCreatedMessage(string message)
        {
            try
            {
                string blobstorageconnection = _configuration.GetValue<string>("BlobConnectionString");
                //string storageCnnStr = GetVaultSecretKey(blobstorageconnection).Result.ToString();
                string storageCnnStr = await  _secretManager.GetSecret(blobstorageconnection);

                //string storageCnnStr = "DefaultEndpointsProtocol=https;AccountName=donorfilestorage;AccountKey=SehkQPguSawbcY7ZQxoEAq4zntzMnodYYxtzl3FYhA4Ho7hqBLCOYrjKlPuaGfqVI53njtnzIXNr+ASteGUhBA==;EndpointSuffix=core.windows.net";

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageCnnStr);
                CloudQueueClient cloudQueueClient = storageAccount.CreateCloudQueueClient();
                CloudQueue cloudQueue = cloudQueueClient.GetQueueReference("donormessagequeue");
               
                CloudQueueMessage queueMessage = new CloudQueueMessage(message);
                await cloudQueue.AddMessageAsync(queueMessage);
            }
            catch (Exception ex)
            {

            }
            return true;
        }

        private async Task<bool> pushDonorCreatedMessage(string name, string email)
        {
            string message = name + (!string.IsNullOrEmpty(email) ? "-" + email : "");
            return await pushDonorCreatedMessage(message);
        }
        private async Task<string> GetVaultSecretKey(string secretName)
        {
            try
            {
                if (string.IsNullOrEmpty(secretName))
                {
                    return BadRequest().ToString();
                }
                string secretValue = await _secretManager.GetSecret(secretName);
                if (!string.IsNullOrEmpty(secretValue))
                {
                    return secretValue;
                }
                else
                {
                    return NotFound("Secret key not found.").ToString();
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Error: Unable to read secret").ToString();
            }
        }
    }
}
