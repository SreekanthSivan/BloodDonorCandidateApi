using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;
using WebCandidateAPI.Interfaces;

namespace WebCandidateAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileController> _logger;
        private readonly IKeyVaultManager _secretManager;

        public FileController(IConfiguration configuration, ILogger<FileController> logger, IKeyVaultManager secretManager)
        {
            _configuration = configuration;
            _logger = logger;
            _secretManager = secretManager;
        }

        [HttpGet]
        public  void Get()
        {
            string blobstorageconnection = _configuration.GetValue<string>("BlobConnectionString");
            string blobStorageConnectionString = GetVaultSecretKey(blobstorageconnection).Result.ToString();
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                string systemFileName = file.FileName;
                string blobstorageconnection = _configuration.GetValue<string>("BlobConnectionString");
                string blobStorageConnectionString = GetVaultSecretKey(blobstorageconnection).Result.ToString();
                // Retrieve storage account from connection string.
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(blobStorageConnectionString);
                // Create the blob client.
                CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
                // Retrieve a reference to a container.
                CloudBlobContainer container = blobClient.GetContainerReference(_configuration.GetValue<string>("BlobContainerName"));
                // This also does not make a service call; it only creates a local object.
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(systemFileName);
                await using (var data = file.OpenReadStream())
                {
                    await blockBlob.UploadFromStreamAsync(data);
                }

                object result = new { url = blockBlob.Uri };
                return Ok(result);
            }
            catch  (Exception ex)
            {
                _logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, ex, ex.Message);
                return BadRequest(ex.Message);
            }

        }
        [HttpPost("Download")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            CloudBlockBlob blockBlob;
            await using (MemoryStream memoryStream = new MemoryStream())
            {
                string blobstorageconnection = _configuration.GetValue<string>("BlobConnectionString");
                string blobStorageConnectionString =GetVaultSecretKey(blobstorageconnection).Result.ToString();
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(blobStorageConnectionString);
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(_configuration.GetValue<string>("BlobContainerName"));
                blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                await blockBlob.DownloadToStreamAsync(memoryStream);
            }

            Stream blobStream = blockBlob.OpenReadAsync().Result;
            return File(blobStream, blockBlob.Properties.ContentType, blockBlob.Name);
        }
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            string blobstorageconnection = _configuration.GetValue<string>("BlobConnectionString");
            string blobStorageConnectionString = GetVaultSecretKey(blobstorageconnection).Result.ToString();
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(blobStorageConnectionString);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            string strContainerName = _configuration.GetValue<string>("BlobContainerName");
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(strContainerName);
            var blob = cloudBlobContainer.GetBlobReference(fileName);
            await blob.DeleteIfExistsAsync();
            return Ok("File Deleted");
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
