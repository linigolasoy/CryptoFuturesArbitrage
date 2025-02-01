using CoinEx.Net.Clients;
using CoinEx.Net.Objects.Models.V2;
using Crypto.Exchanges.All.Common;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.RateLimiting.Guards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx.Websocket
{
    internal class CoinexWebsocket : IFuturesWebsocketPublic
    {

        private class MarketWebsocket
        {
            public MarketWebsocket(CoinExSocketClient oClient) 
            { 
                Client = oClient;   
            }    
            public CoinExSocketClient Client { get; }
            public List<IFuturesSymbol> Symbols { get; } = new List<IFuturesSymbol>();  
        }
        private CoinexFutures m_oExchange;

        private List<MarketWebsocket> m_aMarketWebsockets = new List<MarketWebsocket>();

        private CoinexOrderbookManager m_oOrderbookManager;
        private CoinexFundingRateManager m_oFundingManager;




        public CoinexWebsocket( CoinexFutures oExchange )
        {
            m_oExchange = oExchange;
            m_oOrderbookManager = new CoinexOrderbookManager(this);
            m_oFundingManager = new CoinexFundingRateManager(this);
        }
    
        public IFuturesExchange Exchange { get => m_oExchange; }


        public IWebsocketManager<IFundingRate> FundingRateManager { get => m_oFundingManager; }
        public IWebsocketManager<IFuturesOrder> FuturesOrderManager => throw new NotImplementedException();

        public IWebsocketManager<IFuturesPosition> FuturesPositionManager => throw new NotImplementedException();

        public IOrderbookManager OrderbookManager { get => m_oOrderbookManager; }


        public async Task<bool> Start()
        {
            await Stop();
            return true;
        }


        public async Task Stop()
        {
            return;
        }

        public async Task<bool> SubscribeToMarket(IFuturesSymbol[] aSymbols)
        {
            MarketWebsocket oNewWs = new MarketWebsocket(new CoinExSocketClient());

            List<string> aSymbolString = aSymbols.Select(p=> p.Symbol).ToList();
            var oResult = await oNewWs.Client.FuturesApi.SubscribeToOrderBookUpdatesAsync(aSymbolString, 10, null, true, OnOrderbook);
            if (oResult == null || !oResult.Success) return false;

            oNewWs.Symbols.AddRange(aSymbols);  
            /*
            foreach ( ISymbol symbol in aSymbols) 
            {
                var oResult = await oNewWs.Client.FuturesApi.SubscribeToOrderBookUpdatesAsync(symbol.Symbol, 10, null, true, OnOrderbook);
                if (!oResult.Success) continue;
                oNewWs.Symbols.Add((IFuturesSymbol)symbol); 
            }
            */
            m_aMarketWebsockets.Add(oNewWs);    
            return true;
        }

        private void OnOrderbook( DataEvent<CoinExOrderBook> oEvent)
        {
            if(oEvent == null) return;  
            if(oEvent.Data == null) return;
            m_oOrderbookManager.Put(oEvent.Timestamp, oEvent.Data);
        }

        /// <summary>
        /// Subscribe to tickers, no funding rates on Coinex
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<bool> SubscribeToFundingRates(IFuturesSymbol[] aSymbols)
        {
            MarketWebsocket oNewWs = new MarketWebsocket(new CoinExSocketClient());

            string[] aSymbolString = aSymbols.Select(p=> p.Symbol).ToArray();

            var oResult = await oNewWs.Client.FuturesApi.SubscribeToTickerUpdatesAsync(aSymbolString, OnTicker);
            if( oResult == null || !oResult.Success ) return false;    
            oNewWs.Symbols.AddRange(aSymbols);
            m_aMarketWebsockets.Add(oNewWs);
            return true;
        }

        /// <summary>
        /// Put on manager
        /// </summary>
        /// <param name="oEvent"></param>
        private void OnTicker(DataEvent<IEnumerable<CoinExFuturesTickerUpdate>> oEvent)
        {
            if( oEvent == null) return;
            if( oEvent.Data == null) return;
            if (oEvent.Data.Count() <= 0) return;
            m_oFundingManager.Put(oEvent.Data);
        }
    }
}
