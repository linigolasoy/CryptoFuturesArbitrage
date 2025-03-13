using Crypto.Exchanges.All.CoinEx.Websocket;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using CryptoClients.Net.Interfaces;
using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Crypto.Interface.Futures.Account.IFuturesAccount;

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexAccount : IFuturesAccount
    {
        public CoinexAccount(CoinexFutures oExchange, IExchangeRestClient oClient) 
        { 
            Exchange = oExchange;
            m_oGlobalClient = oClient;
            m_oWebsocketPrivate = new CoinexWebsocketPrivate(this);
        }

        private CoinexWebsocketPrivate m_oWebsocketPrivate;
        private IExchangeRestClient m_oGlobalClient;
        public IFuturesExchange Exchange { get; }
        public event PrivateDelegate? OnPrivateEvent;

        public IWebsocketManager<IFuturesBalance> BalanceManager { get => m_oWebsocketPrivate.BalanceManager; }

        public IWebsocketManager<IFuturesOrder> OrderManager { get => m_oWebsocketPrivate.OrderManager; }

        public IWebsocketManager<IFuturesPosition> PositionManager { get => m_oWebsocketPrivate.PositionManager; }

        public async Task PostEvent(IWebsocketQueueItem oItem)
        {
            if (OnPrivateEvent != null) await OnPrivateEvent(oItem);
        }

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

        /// <summary>
        /// Get open positions
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesPosition[]?> GetPositions()
        {
            var oResult = await m_oGlobalClient.CoinEx.FuturesApi.Trading.GetPositionsAsync();

            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null || oResult.Data.Items == null ) return null;
            List<IFuturesPosition> aResult = new List<IFuturesPosition>();
            foreach( var oItem in oResult.Data.Items )
            {
                IFuturesSymbol? oFound = Exchange.SymbolManager.GetSymbol(oItem.Symbol);
                if (oFound == null) continue;   
                IFuturesPosition oNew = new CoinexPoisitionLocal(oFound, oItem);
                aResult.Add(oNew);
            }
            return aResult.ToArray();
        }
        public async Task<bool> StartSockets()
        {
            bool bResult = await m_oWebsocketPrivate.Start();

            return bResult;
        }
        public async Task StopSockets()
        {
            if (m_oWebsocketPrivate == null) return;
            await m_oWebsocketPrivate.Stop();
        }
        public async Task<ITradingResult<decimal>> Transfer(decimal nAmoun, bool bFromSpot)
        {
            throw new NotImplementedException();
        }

    }
}
