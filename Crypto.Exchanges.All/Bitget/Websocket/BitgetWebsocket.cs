using Bitget.Net.Clients;
using Bitget.Net.Enums;
using Bitget.Net.Objects.Models.V2;
using CoinEx.Net.Clients;
using Crypto.Exchanges.All.Common;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget.Websocket
{
    internal class BitgetWebsocket : IFuturesWebsocketPublic
    {


        private BitgetSocketClient? m_oFundingClient = null;
        private BitgetSocketClient? m_oMarketClient = null;

        private BitgetFutures m_oExchange;

        private BitgetFundingRateManager m_oFundingManager;
        private BitgetOrderbookManager m_oOrderbookManager;
        public BitgetWebsocket(BitgetFutures oExchange)
        { 
            m_oExchange = oExchange;
            m_oFundingManager = new BitgetFundingRateManager(this);
            m_oOrderbookManager = new BitgetOrderbookManager(this);
        }
        public IFuturesExchange Exchange { get => m_oExchange; }



        public IOrderbookManager OrderbookManager { get => m_oOrderbookManager; }

        public IWebsocketManager<IFundingRate> FundingRateManager { get => m_oFundingManager; }


        public async Task<bool> Start()
        {
            await Stop();


            bool bResult = await SubscribeToMarket(Exchange.SymbolManager.GetAllValues());
            if (!bResult) return false;
            bResult = await SubscribeToFundingRates(Exchange.SymbolManager.GetAllValues());

            return bResult;
        }

        public async Task Stop()
        {
            /*
            if( m_oPrivateClient != null)
            {
                await m_oPrivateClient.UnsubscribeAllAsync();
                await Task.Delay(1000);
                m_oPrivateClient.Dispose();
                m_oPrivateClient = null;    
            }
            */
            if (m_oFundingClient != null)
            {
                await m_oFundingClient.UnsubscribeAllAsync();
                await Task.Delay(1000);
                m_oFundingClient.Dispose();
                m_oFundingClient = null;
            }
            
            if( m_oMarketClient != null)
            {
                await m_oMarketClient.UnsubscribeAllAsync();    
                await Task.Delay(1000);
                m_oMarketClient.Dispose();
                m_oMarketClient = null;
            }
            
            return;
        }


        public async Task<bool> SubscribeToFundingRates(IFuturesSymbol[] aSymbols)
        {
            if (m_oFundingClient != null) return true;
            m_oFundingClient = new BitgetSocketClient();
            string[] aSymbolString = aSymbols.Select(s => s.Symbol.ToString()).ToArray();

            var oResult = await m_oFundingClient.FuturesApiV2.SubscribeToTickerUpdatesAsync(BitgetProductTypeV2.UsdtFutures, aSymbolString, OnTicker);
            if( oResult == null || !oResult.Success) return false ;
            return true;
        }

        public async Task<bool> SubscribeToMarket(IFuturesSymbol[] aSymbols)
        {
            if (m_oMarketClient != null) return true;
            m_oMarketClient = new BitgetSocketClient();
            string[] aSymbolString = aSymbols.Select(s => s.Symbol.ToString()).ToArray();

            var oResult = await m_oMarketClient.FuturesApiV2.SubscribeToOrderBookUpdatesAsync( BitgetProductTypeV2.UsdtFutures, aSymbolString, 15, OnOrderBook);
            if (oResult == null || !oResult.Success) return false;
            return true;
        }

        /*
        private async Task<bool> SubscribeToOrderBook( IFuturesSymbol[] aSymbols )
        {
            if (m_oMarketClient == null) return false;
            string[] aSymbolString = aSymbols.Select(p => p.Symbol).ToArray();
            var oResult = await m_oMarketClient.FuturesApiV2.SubscribeToOrderBookUpdatesAsync(BitgetProductTypeV2.UsdtFutures, aSymbolString, 15, OnOrderBook);
            if (oResult == null || !oResult.Success) return false;
            return true;

        }

        */
        private void OnOrderBook(DataEvent<BitgetOrderBookUpdate> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            if (oEvent.Symbol == null) return;
            m_oOrderbookManager.Put(oEvent.Symbol, oEvent.Timestamp, oEvent.Data);
        }

        private void OnTicker(DataEvent<BitgetFuturesTickerUpdate> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            m_oFundingManager.Put(oEvent.Data);
        }

    }
}
