using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.IpfCryptoClients.Interface
{
    internal interface ICryptoRestParser
    {
        public IFuturesExchange Exchange { get; }

        public string? ErrorToMessage(int nError);
    }
}
