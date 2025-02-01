using CoinEx.Net.Clients;
using CoinEx.Net.Objects.Models.V2;
using CoinEx.Net.Enums;
using Crypto.Exchanges.All.Common;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx.Websocket
{
    internal class CoinexWebsocketPrivate : BasePrivateQueueManager, IFuturesWebsocketPrivate
    {
        private CoinExSocketClient? m_oPrivateClient = null;
        private IFuturesExchange m_oExchange;

        private CoinexOrderManager m_oOrderManager;
        private CoinexPoisitionManager m_oPositionManager;
        private CoinexBalanceManager m_oBalanceManager;

        public CoinexWebsocketPrivate( IFuturesAccount oAccount): base(oAccount) 
        { 
            m_oExchange = oAccount.Exchange;    
            m_oOrderManager = new CoinexOrderManager(this); 
            m_oPositionManager = new CoinexPoisitionManager(this);
            m_oBalanceManager = new CoinexBalanceManager(this);
        } 
        public IFuturesExchange Exchange { get => m_oExchange; }


        public IWebsocketPrivateManager<IFuturesBalance> BalanceManager { get => m_oBalanceManager; }

        public IWebsocketPrivateManager<IFuturesOrder> OrderManager { get => m_oOrderManager; }

        public IWebsocketPrivateManager<IFuturesPosition> PositionManager { get => m_oPositionManager; }

        /// <summary>
        /// Start private socket
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Start()
        {
            await Stop();
            await StartLoop();
            m_oPrivateClient = new CoinExSocketClient();
            m_oPrivateClient.SetApiCredentials( ((CoinexFutures)m_oExchange).ApiCredentials);
            await m_oBalanceManager.LoadInitialBalances();  

            var oResult = await m_oPrivateClient.FuturesApi.SubscribeToBalanceUpdatesAsync(OnBalanceUpdates);
            if (oResult == null || !oResult.Success) return false;
            await Task.Delay(500);

            oResult = await m_oPrivateClient.FuturesApi.SubscribeToOrderUpdatesAsync(OnOrderUpdates);
            if (oResult == null || !oResult.Success) return false;
            await Task.Delay(500);

            oResult = await m_oPrivateClient.FuturesApi.SubscribeToPositionUpdatesAsync(OnPositionUpdates);
            if (oResult == null || !oResult.Success) return false;
            await Task.Delay(500);


            return true;
        }


        public async Task Stop()
        {
            if (m_oPrivateClient != null)
            {
                await m_oPrivateClient.UnsubscribeAllAsync();
                await Task.Delay(1000);
                m_oPrivateClient.Dispose();
                m_oPrivateClient = null;
            }
            await StopLoop();
        }
        private void OnBalanceUpdates(DataEvent<IEnumerable<CoinExFuturesBalance>> oEvent)
        {
            if (oEvent == null || oEvent.Data == null || oEvent.Data.Count() <= 0) return;
            foreach (var oData in oEvent.Data)
            {
                m_oBalanceManager.Put(oData);
            }
        }

        private void OnOrderUpdates(DataEvent<CoinExFuturesOrderUpdate> oEvent)
        {
            if( oEvent == null || oEvent.Data == null ) return;
            m_oOrderManager.Put(oEvent.Data);
        }
        private void OnPositionUpdates(DataEvent<CoinExPositionUpdate> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            bool bClose = (oEvent.Data.Event == PositionUpdateType.Close);
            m_oPositionManager.Put(oEvent.Data, bClose);
        }

    }
}
