using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.IpfCryptoClients.Interface
{
    internal interface ICryptoErrorCode
    {
        public HttpStatusCode HttpCode { get; }

        public int ErrorCode { get; }   
        public string? Message { get; }    

    }
    internal interface ICryptoRestResult<T>
    {
        public bool Success { get; }    
        public ICryptoErrorCode? Error { get; }
    
        public T? Data { get; } 
    }

}
