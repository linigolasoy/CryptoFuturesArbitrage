using BingX.Net.Objects.Models;
using Crypto.Exchanges.All.Bingx.Websocket;
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
using static Crypto.Interface.Futures.Account.IFuturesAccount;

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxAccount : IFuturesAccount
    {
        private IExchangeRestClient m_oGlobalClient;


        private IFuturesWebsocketPrivate m_oWebsocket;
        public BingxAccount(IFuturesExchange oExchange, IExchangeRestClient oClient) 
        { 
            Exchange = oExchange;   
            m_oGlobalClient = oClient;
            m_oWebsocket = new BingxWebsocketPrivate(this);
        }
        public IFuturesExchange Exchange { get; }
        public event PrivateDelegate? OnPrivateEvent;

        public IWebsocketManager<IFuturesBalance> BalanceManager { get => m_oWebsocket.BalanceManager; }

        public IWebsocketManager<IFuturesOrder> OrderManager { get => m_oWebsocket.OrderManager; }

        public IWebsocketManager<IFuturesPosition> PositionManager { get => m_oWebsocket.PositionManager; }

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

        public async Task PostEvent(IWebsocketQueueItem oItem)
        {
            if( OnPrivateEvent != null ) await OnPrivateEvent(oItem);
        }
        /// <summary>
        /// Get positions
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesPosition[]?> GetPositions()
        {
            var oResult = await m_oGlobalClient.BingX.PerpetualFuturesApi.Trading.GetPositionsAsync();  
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            IFuturesSymbol[]? aSymbols = await Exchange.Market.GetSymbols();
            if (aSymbols == null) return null;
            List<IFuturesPosition> aResult = new List<IFuturesPosition>();
            foreach( var oData in oResult.Data)
            {
                IFuturesSymbol? oSymbol = aSymbols.FirstOrDefault(p=> p.Symbol== oData.Symbol); 
                if (oSymbol == null) continue;

                IFuturesPosition oNew = new BingxPositionLocal(oSymbol, oData);
                aResult.Add(oNew);
            }
            return aResult.ToArray();
        }

        public async Task<bool> StartSockets()
        {
            return await m_oWebsocket.Start();
        }
    }
}
