using CoinEx.Net.Clients;
using CoinEx.Net.Objects.Models.V2;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx.Websocket
{
    internal class CoinexWebsocketPrivate : IWebsocketPrivate
    {
        private CoinExSocketClient? m_oPrivateClient = null;
        private CoinexFutures m_oExchange;

        private CoinexOrderManager m_oOrderManager;
        private CoinexPoisitionManager m_oPositionManager;
        private CoinexBalanceManager m_oBalanceManager;

        public CoinexWebsocketPrivate( CoinexFutures oExchange) 
        { 
            m_oExchange = oExchange;    
            m_oOrderManager = new CoinexOrderManager(this); 
            m_oPositionManager = new CoinexPoisitionManager(this);
            m_oBalanceManager = new CoinexBalanceManager(this.m_oExchange);
        } 
        public ICryptoFuturesExchange Exchange { get => m_oExchange; }

        public IFuturesSymbol[] FuturesSymbols { get; internal set; } = Array.Empty<IFuturesSymbol>();

        public IWebsocketManager<IFuturesBalance> BalanceManager => throw new NotImplementedException();

        public IWebsocketManager<IFuturesOrder> OrderManager { get => m_oOrderManager; }

        public IWebsocketManager<IFuturesPosition> PositionManager { get => m_oPositionManager; }

        /// <summary>
        /// Start private socket
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Start()
        {
            if( m_oPrivateClient != null )
            {
                await m_oPrivateClient.UnsubscribeAllAsync();
                await Task.Delay(1000);
                m_oPrivateClient.Dispose();
                m_oPrivateClient=null;  
            }
            m_oPrivateClient = new CoinExSocketClient();
            m_oPrivateClient.SetApiCredentials(m_oExchange.ApiCredentials);

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
            m_oPositionManager.Put(oEvent.Data);
        }

    }
}
