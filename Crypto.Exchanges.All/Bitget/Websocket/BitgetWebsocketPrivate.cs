using Bitget.Net.Clients;
using Bitget.Net.Objects.Models.V2;
using Bitget.Net.Enums;
using Crypto.Interface;
using Crypto.Interface.Futures;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Websockets;
using Crypto.Exchanges.All.Common;

namespace Crypto.Exchanges.All.Bitget.Websocket
{
    internal class BitgetWebsocketPrivate : BasePrivateQueueManager, IFuturesWebsocketPrivate
    {
        private IFuturesExchange m_oExchange;

        private BitgetSocketClient? m_oPrivateClient = null;
        private BitgetBalanceManager m_oBalanceManager;
        private BitgetOrderManager m_oOrderManager;
        private BitgetPositionManager m_oPositionManager;
        public BitgetWebsocketPrivate( IFuturesAccount oAccount ): base(oAccount)
        {
            m_oExchange = oAccount.Exchange;
            m_oBalanceManager = new BitgetBalanceManager(this);
            m_oOrderManager = new BitgetOrderManager(this);
            m_oPositionManager = new BitgetPositionManager(this);   
        }

        public IFuturesExchange Exchange { get => m_oExchange; }


        public IWebsocketPrivateManager<IFuturesBalance> BalanceManager { get => m_oBalanceManager; }

        public IWebsocketPrivateManager<IFuturesOrder> OrderManager { get => m_oOrderManager; }

        public IWebsocketPrivateManager<IFuturesPosition> PositionManager { get => m_oPositionManager; }

        public async Task<bool> Start()
        {
            await Stop();
            await StartLoop();

            bool bPosStarted = await m_oPositionManager.Start(); 
            m_oPrivateClient = new BitgetSocketClient();
            m_oPrivateClient.SetApiCredentials( ((BitgetFutures)m_oExchange).ApiCredentials);

            var oResult = await m_oPrivateClient.FuturesApiV2.SubscribeToBalanceUpdatesAsync( BitgetProductTypeV2.UsdtFutures, OnBalance);
            if (oResult == null || !oResult.Success) return false;

            oResult = await m_oPrivateClient.FuturesApiV2.SubscribeToOrderUpdatesAsync(BitgetProductTypeV2.UsdtFutures, OnOrder);
            if (oResult == null || !oResult.Success) return false;

            // oResult = await m_oPrivateClient.FuturesApiV2.SubscribeToPositionUpdatesAsync(BitgetProductTypeV2.UsdtFutures, OnPosition);
            // if (oResult == null || !oResult.Success) return false;
            // oResult = await m_oPrivateClient.FuturesApiV2.SubscribeToPositionHistoryUpdatesAsync(BitgetProductTypeV2.UsdtFutures, OnPositionHistory);
            // if (oResult == null || !oResult.Success) return false;

            // m_oPrivateClient.FuturesApiV2.SubscribeToUserTradeUpdatesAsync(BitgetProductTypeV2.UsdtFutures, OnUserTradeUpdate);

            return true;

        }

        public async Task Stop()
        {
            await m_oPositionManager.Stop();
                
            if (m_oPrivateClient != null)
            {
                await m_oPrivateClient.UnsubscribeAllAsync();
                await Task.Delay(1000);
                m_oPrivateClient.Dispose();
                m_oPrivateClient = null;
            }
            await StopLoop();
        }
        private void OnBalance( DataEvent<IEnumerable<BitgetFuturesBalanceUpdate>> oEvent )
        {
            if (oEvent == null || oEvent.Data == null || oEvent.Data.Count() <= 0) return;
            foreach( var oData in oEvent.Data )
            {
                m_oBalanceManager.Put(oData);
            }
            return;
        }
        private void OnUserTradeUpdate(DataEvent<IEnumerable<BitgetFuturesUserTradeUpdate>> oEvent)
        {
            if (oEvent == null || oEvent.Data == null || oEvent.Data.Count() <= 0) return;
            foreach (var oData in oEvent.Data)
            {
            }
            return;
        }


        private void OnOrder(DataEvent<IEnumerable<BitgetFuturesOrderUpdate>> oEvent)
        {
            if( oEvent == null || oEvent.Data == null ) return;
            m_oOrderManager.Put(oEvent.Data);
            return;
        }
        private void OnPosition(DataEvent<IEnumerable<BitgetPositionUpdate>> oEvent)
        {
            if (oEvent == null || oEvent.Data == null ) return;
            m_oPositionManager.Put(oEvent.Data);
            return;
        }
        private void OnPositionHistory(DataEvent<IEnumerable<BitgetPositionHistoryUpdate>> oEvent)
        {
            if (oEvent == null || oEvent.Data == null ) return;
            // m_oPositionManager.PutHistory(oEvent.Data);
            return;
        }

    }
}
