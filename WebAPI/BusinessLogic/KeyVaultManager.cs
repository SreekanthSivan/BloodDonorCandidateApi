using Azure.Security.KeyVault.Secrets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCandidateAPI.Interfaces;

namespace WebCandidateAPI.BusinessLogic
{
    public class KeyVaultManager: IKeyVaultManager
    {
        private readonly SecretClient _secretClient;

        public KeyVaultManager(SecretClient secretClient)
        {
            _secretClient = secretClient;
        }

        public async Task<string> GetSecret(string secretName)
        {
            try
            {
                KeyVaultSecret keyValueSecret = await

                _secretClient.GetSecretAsync(secretName);

                return keyValueSecret.Value;
            }
            catch
            {
                throw;
            }
        }
    }
}
