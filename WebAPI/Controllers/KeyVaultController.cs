using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCandidateAPI.Interfaces;

namespace WebCandidateAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeyVaultController : ControllerBase
    {
        private readonly IKeyVaultManager _secretManager;

        public KeyVaultController(IKeyVaultManager secretManager)
        {
            _secretManager = secretManager;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery(Name ="sn")] string secretName)
        {
            try
            {
                if (string.IsNullOrEmpty(secretName))
                {
                    return BadRequest();
                }
                string secretValue = await  _secretManager.GetSecret(secretName);
                if (!string.IsNullOrEmpty(secretValue))
                {
                    return Ok(secretValue);
                }
                else
                {
                    return NotFound("Secret key not found.");
                }
            }
            catch(Exception ex)
            {
                return BadRequest("Error: Unable to read secret");
            }
        }
    }
}
