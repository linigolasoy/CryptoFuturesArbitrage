using Crypto.Exchanges.All.Bitmart.Websocket;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using CryptoClients.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitmart
{
    internal class BitmartAccount : IFuturesAccount
    {
        private IExchangeRestClient m_oGlobalClient;
        private BitmartFutures m_oExchange;
        private IFuturesWebsocketPrivate m_oWebsocket;
        public BitmartAccount( BitmartFutures oExchange )         
        { 
            m_oExchange = oExchange;
            m_oGlobalClient = oExchange.GlobalClient;
            m_oWebsocket = new BitmartWebsocketsPrivate(this);
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        public IWebsocketManager<IFuturesBalance> BalanceManager { get => m_oWebsocket.BalanceManager; }

        public IWebsocketManager<IFuturesOrder> OrderManager { get => m_oWebsocket.OrderManager; }

        public IWebsocketManager<IFuturesPosition> PositionManager { get => m_oWebsocket.PositionManager; }

        public event IFuturesAccount.PrivateDelegate? OnPrivateEvent;

        public async Task<IFuturesBalance[]?> GetBalances()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get positions
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesPosition[]?> GetPositions()
        {
            IFuturesSymbol[]? aSymbols = await this.Exchange.Market.GetSymbols();
            if (aSymbols == null) return null;
            var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.Trading.GetPositionsAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IFuturesPosition> aResult = new List<IFuturesPosition>();
            foreach( var oData in  oResult.Data )
            {
                IFuturesSymbol? oFound = aSymbols.FirstOrDefault(p=> p.Symbol == oData.Symbol); 
                if (oFound == null) continue;
                aResult.Add(new BitmartPositionLocal(oFound, oData));
            }
            return aResult.ToArray();
        }

        public async Task PostEvent(IWebsocketQueueItem oItem)
        {
            if (OnPrivateEvent != null) await OnPrivateEvent(oItem);
        }

        public async Task<bool> StartSockets()
        {
            return await m_oWebsocket.Start();
        }
        public async Task StopSockets()
        {
            await m_oWebsocket.Stop();
        }
    }
}
