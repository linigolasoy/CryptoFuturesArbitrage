using CoinEx.Net.Objects.Models.V2;
using Crypto.Exchanges.All.CoinEx.Websocket;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using CryptoClients.Net;
using CryptoClients.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexMarket : IFuturesMarket
    {
        private CoinexFutures m_oExchange;
        private IExchangeRestClient m_oGlobalClient;

        private CoinexWebsocket? m_oWebsocket = null;
        
        public CoinexMarket( CoinexFutures oExchange) 
        { 
            m_oExchange = oExchange;
            m_oGlobalClient = new ExchangeRestClient();
        }
        public IFuturesExchange Exchange { get => m_oExchange; }
        public IFuturesWebsocketPublic? Websocket { get => m_oWebsocket; }


        /// <summary>
        /// Start sockets
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartSockets()
        {
            await EndSockets();
            m_oWebsocket = new CoinexWebsocket(m_oExchange);

            bool bResult = await m_oWebsocket.Start();
            if (!bResult) return false;
            await Task.Delay(2000);
            bResult = await m_oWebsocket.SubscribeToFundingRates(m_oExchange.SymbolManager.GetAllValues()); 
            if( !bResult ) return false;    
            bResult = await m_oWebsocket.SubscribeToMarket(m_oExchange.SymbolManager.GetAllValues());
            await Task.Delay(2000); 

            return bResult;
        }

        /// <summary>
        /// End sockets
        /// </summary>
        /// <returns></returns>
        public async Task<bool> EndSockets()
        {
            if (m_oWebsocket == null) return true;
            await m_oWebsocket.Stop();
            await Task.Delay(1000);
            m_oWebsocket = null;
            return true;
        }


        /// <summary>
        /// Get tickets
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesTicker[]?> GetTickers()
        {
            var oResult = await m_oGlobalClient.CoinEx.FuturesApi.ExchangeData.GetTickersAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IFuturesTicker> aResult = new List<IFuturesTicker>();
            DateTime dNow = DateTime.Now;   
            foreach (var oData in oResult.Data)
            {
                IFuturesSymbol? oSymbol = Exchange.SymbolManager.GetSymbol(oData.Symbol);
                if (oSymbol == null) continue;
                aResult.Add(new BaseTicker(oSymbol, oData.LastPrice, dNow));
            }
            return aResult.ToArray();
        }



        /// <summary>
        /// Get funding rates of specific symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<IFundingRateSnapShot?> GetFundingRates(IFuturesSymbol oSymbol)
        {
            IFundingRateSnapShot[]? aResult = await GetFundingRates(new IFuturesSymbol[] { oSymbol });
            if (aResult == null) return null;
            if (aResult.Length <= 0) return null;
            return aResult[0];
        }

        /// <summary>
        /// Get all funding rates
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRateSnapShot[]?> GetFundingRates(IFuturesSymbol[] aSymbols)
        {

            var oResult = await m_oGlobalClient.CoinEx.FuturesApi.ExchangeData.GetFundingRatesAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null || oResult.Data.Count() <= 0) return null;
            List<IFundingRateSnapShot> aResult = new List<IFundingRateSnapShot>();
            foreach (CoinExFundingRate oData in oResult.Data)
            {
                if (oData.LastFundingTime == null) continue;
                IFuturesSymbol? oFound = aSymbols.FirstOrDefault(p => p.Symbol == oData.Symbol);
                if (oFound == null) continue;
                aResult.Add(new CoinexFundingRateSnapshot(oFound, oData));
            }
            return aResult.ToArray();
        }


    }
}
