using Bitget.Net.Clients;
using Bitget.Net.Objects.Models.V2;
using Bitget.Net.Enums;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget.Websocket
{
    internal class BitgetWebsocketPrivate : IWebsocketPrivate
    {
        private BitgetFutures m_oExchange;

        private BitgetSocketClient? m_oPrivateClient = null;
        private BitgetBalanceManager m_oBalanceManager;
        private BitgetOrderManager m_oOrderManager;
        private BitgetPositionManager m_oPositionManager;
        public BitgetWebsocketPrivate( BitgetFutures oExchange)
        {
            m_oExchange = oExchange;
            m_oBalanceManager = new BitgetBalanceManager();
            m_oOrderManager = new BitgetOrderManager(this);
            m_oPositionManager = new BitgetPositionManager(this);   
        }

        public ICryptoFuturesExchange Exchange { get => m_oExchange; }

        public IFuturesSymbol[] FuturesSymbols { get; internal set; } = Array.Empty<IFuturesSymbol>();

        public IWebsocketManager<IFuturesBalance> BalanceManager { get => m_oBalanceManager; }

        public IWebsocketManager<IFuturesOrder> OrderManager { get => m_oOrderManager; }

        public IWebsocketManager<IFuturesPosition> PositionManager { get => m_oPositionManager; }

        public async Task<bool> Start()
        {
            if (m_oPrivateClient != null)
            {
                await m_oPrivateClient.UnsubscribeAllAsync();
                await Task.Delay(1000);
                m_oPrivateClient.Dispose();
                m_oPrivateClient = null;
            }

            m_oPrivateClient = new BitgetSocketClient();
            m_oPrivateClient.SetApiCredentials(m_oExchange.ApiCredentials);

            var oResult = await m_oPrivateClient.FuturesApiV2.SubscribeToBalanceUpdatesAsync( BitgetProductTypeV2.UsdtFutures, OnBalance);
            if (oResult == null || !oResult.Success) return false;

            oResult = await m_oPrivateClient.FuturesApiV2.SubscribeToOrderUpdatesAsync(BitgetProductTypeV2.UsdtFutures, OnOrder);
            if (oResult == null || !oResult.Success) return false;

            oResult = await m_oPrivateClient.FuturesApiV2.SubscribeToPositionUpdatesAsync(BitgetProductTypeV2.UsdtFutures, OnPosition);
            if (oResult == null || !oResult.Success) return false;
            oResult = await m_oPrivateClient.FuturesApiV2.SubscribeToPositionHistoryUpdatesAsync(BitgetProductTypeV2.UsdtFutures, OnPositionHistory);
            if (oResult == null || !oResult.Success) return false;

            return true;

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
        private void OnOrder(DataEvent<IEnumerable<BitgetFuturesOrderUpdate>> oEvent)
        {
            if( oEvent == null || oEvent.Data == null || oEvent.Data.Count() <= 0 ) return;
            m_oOrderManager.Put(oEvent.Data);
            return;
        }
        private void OnPosition(DataEvent<IEnumerable<BitgetPositionUpdate>> oEvent)
        {
            if (oEvent == null || oEvent.Data == null || oEvent.Data.Count() <= 0) return;
            m_oPositionManager.Put(oEvent.Data);
            return;
        }
        private void OnPositionHistory(DataEvent<IEnumerable<BitgetPositionHistoryUpdate>> oEvent)
        {
            if (oEvent == null || oEvent.Data == null || oEvent.Data.Count() <= 0) return;
            m_oPositionManager.PutHistory(oEvent.Data);
            return;
        }

    }
}
