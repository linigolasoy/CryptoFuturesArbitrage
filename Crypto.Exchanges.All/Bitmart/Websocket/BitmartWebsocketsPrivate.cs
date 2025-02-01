using BingX.Net.Clients;
using BitMart.Net.Clients;
using BitMart.Net.Objects.Models;
using Crypto.Exchanges.All.Bingx;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitmart.Websocket
{
    internal class BitmartWebsocketsPrivate : BasePrivateQueueManager, IFuturesWebsocketPrivate
    {
        private BitMartSocketClient? m_oSocketClient = null;
        private BitmartFutures m_oExchange;

        private BitmartBalanceManager m_oBalanceManager;
        private BitmartOrderManager m_oOrderManager;
        private BitmartPositionManager m_oPositionManager;
        public BitmartWebsocketsPrivate(IFuturesAccount oAccount) : base(oAccount)
        {
            m_oExchange = (BitmartFutures)oAccount.Exchange;
            m_oBalanceManager = new BitmartBalanceManager(this);
            m_oOrderManager = new BitmartOrderManager(this);
            m_oPositionManager = new BitmartPositionManager(this);  

        }

        public IFuturesExchange Exchange { get => m_oExchange; }


        public IWebsocketPrivateManager<IFuturesBalance> BalanceManager { get => m_oBalanceManager; }

        public IWebsocketPrivateManager<IFuturesOrder> OrderManager { get => m_oOrderManager; }

        public IWebsocketPrivateManager<IFuturesPosition> PositionManager { get => m_oPositionManager; }

        public async Task<bool> Start()
        {
            await Stop();
            await StartLoop();
            // m_aSymbols = await m_oExchange.Market.GetSymbols();
            // if (m_aSymbols == null) throw new Exception("No symbols");
            // m_oOrderManager.FuturesSymbols = m_aSymbols;
            // m_oPositionManager.FuturesSymbols = m_aSymbols;

            // m_oCancelSource = new CancellationTokenSource();
            // var oResult = await ((BingxFutures)m_oExchange).GlobalClient.BingX.PerpetualFuturesApi.Account.StartUserStreamAsync(m_oCancelSource.Token);
            // if (oResult == null || !oResult.Success) return false;
            // string strListenKey = oResult.Data;

            m_oSocketClient = new BitMartSocketClient();
            m_oSocketClient.SetApiCredentials(m_oExchange.Credentials);
            var oResultBalances = await m_oSocketClient.UsdFuturesApi.SubscribeToBalanceUpdatesAsync(OnBalances);
            if (oResultBalances == null || !oResultBalances.Success) return false;

            var oResultOrders = await m_oSocketClient.UsdFuturesApi.SubscribeToOrderUpdatesAsync(OnOrders);
            if (oResultOrders == null || !oResultOrders.Success) return false;

            var oResultPositions = await m_oSocketClient.UsdFuturesApi.SubscribeToPositionUpdatesAsync(OnPositions);
            if (oResultPositions == null || !oResultPositions.Success) return false;
            return true;
        }

        private void OnBalances( DataEvent<BitMartFuturesBalanceUpdate> oUpdate)
        {
            m_oBalanceManager.Put(oUpdate);
        }
        private void OnOrders(DataEvent<IEnumerable<BitMartFuturesOrderUpdateEvent>> oUpdate)
        {
            if (oUpdate == null) return;
            m_oOrderManager.Put(oUpdate);   
        }
        private void OnPositions(DataEvent<IEnumerable<BitMartPositionUpdate>> oUpdate)
        {
            if( oUpdate == null) return;    
            m_oPositionManager.Put(oUpdate);    
        }

        public async Task Stop()
        {
            if (m_oSocketClient != null) 
            {
                await m_oSocketClient.UnsubscribeAllAsync();
                await Task.Delay(1000);
                m_oSocketClient.Dispose();
                m_oSocketClient = null;
            }
            await StopLoop();
        }
    }
}
