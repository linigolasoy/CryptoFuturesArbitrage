using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Common
{
    public interface IRequestHelper
    {

        public HttpClient Client { get; }

        public int RequestsPerMinute { get; }   
        public Task<string?> GetRequest(string strUrl);
    }
}
