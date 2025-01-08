using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
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

        public IWebsocketManager<IFuturesBalance> BalanceManager => throw new NotImplementedException();

        public IWebsocketManager<IFuturesOrder> OrderManager => throw new NotImplementedException();

        public IWebsocketManager<IFuturesPosition> PositionManager => throw new NotImplementedException();

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

        public async Task<bool> StartSockets()
        {
            throw new NotImplementedException();
        }

    }
}
