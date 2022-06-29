using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCandidateAPI.Interfaces
{
    public interface IKeyVaultManager
    {
        public Task<string> GetSecret(string secretName);
    }
}
