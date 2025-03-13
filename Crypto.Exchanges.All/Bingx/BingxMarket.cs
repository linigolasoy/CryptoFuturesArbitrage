using BingX.Net.Objects.Models;
using Crypto.Exchanges.All.Bingx.Websocket;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxMarket : IFuturesMarket
    {
        private BingxFutures m_oExchange;
        public BingxMarket( BingxFutures oExchange ) 
        { 
            m_oExchange = oExchange;
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        private BingxWebsocket? m_oWebsocket = null;

        public IFuturesWebsocketPublic? Websocket { get => m_oWebsocket; }



        public async Task<bool> StartSockets()
        {
            await EndSockets();
            m_oWebsocket = new BingxWebsocket(m_oExchange);
            
            bool bResult = await m_oWebsocket.Start();
            if( !bResult ) return false;
            bResult = await m_oWebsocket.SubscribeToMarket(m_oExchange.SymbolManager.GetAllValues());
            return bResult;
        }
        public async Task<bool> EndSockets()
        {
            if (m_oWebsocket == null) return true;

            await m_oWebsocket.Stop();
            await Task.Delay(2000);
            m_oWebsocket = null;
            return true;
        }


        /// <summary>
        /// Get tickets
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesTicker[]?> GetTickers()
        {
            DateTime dNow = DateTime.Now;   
            var oResult = await m_oExchange.GlobalClient.BingX.PerpetualFuturesApi.ExchangeData.GetTickersAsync(); //.GetLastTradePricesAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IFuturesTicker> aResult = new List<IFuturesTicker>();  

            foreach( var oData in oResult.Data )
            {
                IFuturesSymbol? oSymbol = Exchange.SymbolManager.GetSymbol(oData.Symbol);
                if (oSymbol == null) continue;
                aResult.Add(new BaseTicker(oSymbol, oData.LastPrice, dNow, oData.BestAskPrice, oData.BestBidPrice));
            }
            return aResult.ToArray();   
        }


        /// <summary>
        /// Funding rates single symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<IFundingRateSnapShot?> GetFundingRates(IFuturesSymbol oSymbol)
        {
            IFundingRateSnapShot[]? aResults = await GetFundingRates(new IFuturesSymbol[] { oSymbol });

            if (aResults == null || aResults.Length <= 0) return null;
            return aResults.FirstOrDefault(p => p.Symbol.Symbol == oSymbol.Symbol);
        }

        /// <summary>
        /// Get funding rates actual
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRateSnapShot[]?> GetFundingRates(IFuturesSymbol[] aSymbols)
        {
            var oResult = await m_oExchange.GlobalClient.BingX.PerpetualFuturesApi.ExchangeData.GetFundingRatesAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;

            List<IFundingRateSnapShot> aResult = new List<IFundingRateSnapShot>();
            foreach (BingXFundingRate oData in oResult.Data)
            {
                if (oData == null) continue;
                IFuturesSymbol? oFound = aSymbols.FirstOrDefault(p => p.Symbol == oData.Symbol);
                if (oFound == null) continue;
                aResult.Add(new BingxFundingRateSnapshot(oFound, oData));

            }
            return aResult.ToArray();
        }

    }
}
