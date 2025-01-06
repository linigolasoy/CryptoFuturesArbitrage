using Bitget.Net.Interfaces.Clients;
using CryptoExchange.Net.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Blofin.NetClient
{
    internal class BlofinRestClient : BaseRestClient
    {

        public BlofinRestClient(): base(null, "Blofin")
        {

        }
    }
}
