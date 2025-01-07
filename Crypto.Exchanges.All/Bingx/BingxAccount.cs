using BingX.Net.Objects.Models;
using Crypto.Interface.Futures;
using CryptoClients.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxAccount : IFuturesAccount
    {
        private IExchangeRestClient m_oGlobalClient;

        public BingxAccount(ICryptoFuturesExchange oExchange, IExchangeRestClient oClient) 
        { 
            Exchange = oExchange;   
            m_oGlobalClient = oClient;
        }
        public ICryptoFuturesExchange Exchange { get; }

        public async Task<IFuturesBalance[]?> GetBalances()
        {
            var oResult = await m_oGlobalClient.BingX.PerpetualFuturesApi.Account.GetBalancesAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;

            List<IFuturesBalance> aResult = new List<IFuturesBalance>();
            foreach (BingXFuturesBalance oData in oResult.Data)
            {
                aResult.Add(new BingxBalance(oData));
            }

            return aResult.ToArray();
        }

        public async Task<IFuturesPosition[]?> GetPositions()
        {
            throw new NotImplementedException();
        }
    }
}
