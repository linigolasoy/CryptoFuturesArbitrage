using Crypto.Interface.Futures;
using CryptoClients.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetAccount : IFuturesAccount
    {
        public ICryptoFuturesExchange Exchange { get; }
        private IExchangeRestClient m_oGlobalClient;

        public BitgetAccount( ICryptoFuturesExchange oExchange, IExchangeRestClient oGlobalClient)
        {
            Exchange = oExchange;
            m_oGlobalClient = oGlobalClient;
        }

        public async Task<IFuturesBalance[]?> GetBalances()
        {
            throw new NotImplementedException();
        }

        public async Task<IFuturesPosition[]?> GetPositions()
        {
            throw new NotImplementedException();
        }
    }
}
