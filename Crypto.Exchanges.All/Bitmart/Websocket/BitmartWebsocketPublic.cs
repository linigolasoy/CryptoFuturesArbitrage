using BitMart.Net.Clients;
using BitMart.Net.Interfaces.Clients;
using BitMart.Net.Objects.Models;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitmart.Websocket
{
    internal class BitmartWebsocketPublic : IFuturesWebsocketPublic
    {


        private BitmartFutures m_oExchange;
        private BitMartSocketClient[]? m_aClientFunding = null;
        private BitMartSocketClient[]? m_aClientOrderbook = null;

        private BitmartFundingRateManager m_oFundingManarger;
        private BitmartOrderbookManager m_oOrderbookManager;
        public BitmartWebsocketPublic( BitmartFutures oExchange) 
        {
            m_oExchange = oExchange;
            m_oFundingManarger = new BitmartFundingRateManager(this);
            m_oOrderbookManager = new BitmartOrderbookManager(this);
        }
        public IFuturesExchange Exchange { get => m_oExchange; }


        public IOrderbookManager OrderbookManager { get => m_oOrderbookManager; }

        public IWebsocketManager<IFundingRate> FundingRateManager { get => m_oFundingManarger; }

        /// <summary>
        /// Create funding rate sockets
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CreateFundingSockets()
        {
            string[] aSymbols = Exchange.SymbolManager.GetAllKeys();    
            List<BitMartSocketClient> aFundingClients = new List<BitMartSocketClient>();
            int nActual = 0;
            int nStep = 50;

            while (nActual < aSymbols.Length)
            {
                string[] aPartial = aSymbols.Skip(nActual).Take(nStep).ToArray();
                BitMartSocketClient oClient = new BitMartSocketClient();
                oClient.SetApiCredentials(m_oExchange.Credentials);
                await Task.Delay(500);
                var oResultFunding = await oClient.UsdFuturesApi.SubscribeToFundingRateUpdatesAsync(aPartial, OnFundingRate);
                if (oResultFunding == null || !oResultFunding.Success) return false;
                aFundingClients.Add(oClient);
                nActual += nStep;
            }
            m_aClientFunding = aFundingClients.ToArray();
            return true;
        }

        /// <summary>
        /// Create orderbook sockets
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CreateOrderbookSockets()
        {
            string[] aSymbols = Exchange.SymbolManager.GetAllKeys();
            List<BitMartSocketClient> aClients = new List<BitMartSocketClient>();
            int nActual = 0;
            int nStep = 50;

            while (nActual < aSymbols.Length)
            {
                string[] aPartial = aSymbols.Skip(nActual).Take(nStep).ToArray();
                BitMartSocketClient oClient = new BitMartSocketClient();
                oClient.SetApiCredentials(m_oExchange.Credentials);
                await Task.Delay(500);
                var oResult = await oClient.UsdFuturesApi.SubscribeToOrderBookSnapshotUpdatesAsync(aPartial, 20, OnOrderbook);
                if (oResult == null || !oResult.Success) return false;
                aClients.Add(oClient);
                nActual += nStep;
            }
            m_aClientOrderbook = aClients.ToArray();
            return true;
        }

        public async Task<bool> Start()
        {
            await Stop();
            bool bResult = await CreateFundingSockets();
            if( !bResult ) return false; 
            bResult = await CreateOrderbookSockets();
            if( !bResult ) return false;
            return true;
        }

        private void OnFundingRate( DataEvent<BitMartFundingRateUpdate> oUpdate )
        {
            m_oFundingManarger.Put( oUpdate );  
        }

        private void OnOrderbook(DataEvent<BitMartFuturesFullOrderBookUpdate> oUpdate)
        {
            m_oOrderbookManager.Put( oUpdate ); 
        }
        /// <summary>
        /// Stop websockets
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task Stop()
        {
            if( m_aClientFunding != null )
            {
                foreach (var oClient in m_aClientFunding)
                {
                    await oClient.UnsubscribeAllAsync();
                    await Task.Delay(1000);
                    oClient.Dispose();
                }

                m_aClientFunding = null;  
            }
            if( m_aClientOrderbook != null )
            {
                foreach (var oClient in m_aClientOrderbook)
                {
                    await oClient.UnsubscribeAllAsync();
                    await Task.Delay(1000);
                    oClient.Dispose();
                }
                m_aClientOrderbook = null;
            }
        }

        public async Task<bool> SubscribeToFundingRates(IFuturesSymbol[] aSymbols)
        {
            return true;
        }

        public async Task<bool> SubscribeToMarket(IFuturesSymbol[] aSymbols)
        {
            return true;
        }
    }
}
