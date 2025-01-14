using Bitget.Net.Clients;
using Bitget.Net.Enums;
using Bitget.Net.Objects.Models.V2;
using CoinEx.Net.Clients;
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

namespace Crypto.Exchanges.All.Bitget.Websocket
{
    internal class BitgetWebsocket : IFuturesWebsocketPublic
    {

        private BitgetSocketClient? m_oFundingClient = null;
        private BitgetSocketClient? m_oMarketClient = null;

        private BitgetFutures m_oExchange;

        private BitgetFundingRateManager m_oFundingManager;
        private BitgetOrderbookManager m_oOrderbookManager;
        public BitgetWebsocket( BitgetFutures oExchange, IFuturesSymbol[] aSymbols ) 
        { 
            m_oExchange = oExchange;
            FuturesSymbols = aSymbols;
            m_oFundingManager = new BitgetFundingRateManager(aSymbols);
            m_oOrderbookManager = new BitgetOrderbookManager(aSymbols);
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        public IFuturesSymbol[] FuturesSymbols { get; }

        public IWebsocketManager<IFuturesOrder> FuturesOrderManager => throw new NotImplementedException();

        public IWebsocketManager<IFuturesPosition> FuturesPositionManager => throw new NotImplementedException();

        public IOrderbookManager OrderbookManager { get => m_oOrderbookManager; }

        public IWebsocketManager<IFundingRate> FundingRateManager { get => m_oFundingManager; }


        public async Task<bool> Start()
        {
            await Stop();
            /*
            m_oPrivateClient = new BitgetSocketClient();
            m_oPrivateClient.SetApiCredentials(m_oExchange.ApiCredentials);
            await m_oPrivateClient.FuturesApiV2.PrepareConnectionsAsync();  
            try
            {
                var oResult = await m_oPrivateClient.FuturesApiV2.SubscribeToBalanceUpdatesAsync(BitgetProductTypeV2.UsdtFutures, OnBalance);
                if (oResult == null || !oResult.Success) return false;

                oResult = await m_oPrivateClient.FuturesApiV2.SubscribeToOrderUpdatesAsync(BitgetProductTypeV2.UsdtFutures, OnOrder);
                if (oResult == null || !oResult.Success) return false;

                oResult = await m_oPrivateClient.FuturesApiV2.SubscribeToPositionUpdatesAsync(BitgetProductTypeV2.UsdtFutures, OnPosition);
                if (oResult == null || !oResult.Success) return false;
            }
            catch (Exception ex)
            {
                return false;
            }
            */
            return true;
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
            if (m_oMarketClient == null)
            {
                m_oMarketClient = new BitgetSocketClient();
            }
            await m_oMarketClient.UnsubscribeAllAsync();
            foreach( var symbol in aSymbols) 
            {
                var oResult = await m_oMarketClient.FuturesApiV2.SubscribeToOrderBookUpdatesAsync(BitgetProductTypeV2.UsdtFutures, symbol.Symbol, 5, OnOrderBook);
                if (oResult == null || !oResult.Success) return false;
            }
            
            return true;
        }

        private void OnOrderBook( DataEvent<BitgetOrderBookUpdate> oEvent )
        {
            if( oEvent == null || oEvent.Data == null ) return; 
            if( oEvent.Symbol == null ) return; 
            m_oOrderbookManager.Put(oEvent.Symbol, oEvent.Data);
        }

        private void OnTicker(DataEvent<BitgetFuturesTickerUpdate> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            m_oFundingManager.Put(oEvent.Data);
        }

    }
}
