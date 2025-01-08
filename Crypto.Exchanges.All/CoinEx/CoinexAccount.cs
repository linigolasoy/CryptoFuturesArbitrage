using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using CryptoClients.Net.Interfaces;
using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexAccount : IFuturesAccount
    {
        public CoinexAccount(ICryptoFuturesExchange oExchange, IExchangeRestClient oClient) 
        { 
            Exchange = oExchange;
            m_oGlobalClient = oClient;  
        }
        private IExchangeRestClient m_oGlobalClient;
        public ICryptoFuturesExchange Exchange { get; }

        public IWebsocketManager<IFuturesBalance> BalanceManager => throw new NotImplementedException();

        public IWebsocketManager<IFuturesOrder> OrderManager => throw new NotImplementedException();

        public IWebsocketManager<IFuturesPosition> PositionManager => throw new NotImplementedException();

        public async Task<IFuturesBalance[]?> GetBalances()
        {
            var oResult = await m_oGlobalClient.CoinEx.FuturesApi.Account.GetBalancesAsync();

            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IFuturesBalance> aResult = new List<IFuturesBalance>();
            foreach (var oData in oResult.Data)
            {
                aResult.Add(new CoinexBalance(oData));
            }

            return aResult.ToArray();
        }

        public async Task<IFuturesPosition[]?> GetPositions()
        {
            throw new NotImplementedException();
        }
        public async Task<bool> StartSockets()
        {
            throw new NotImplementedException();
        }
    }
}
