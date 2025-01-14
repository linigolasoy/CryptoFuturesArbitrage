using CoinEx.Net.Objects.Models.V2;
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
        private IFuturesSymbol[]? m_aSymbols = null;
        public CoinexMarket( CoinexFutures oExchange) 
        { 
            m_oExchange = oExchange;
            m_oGlobalClient = new ExchangeRestClient();
        }
        public IFuturesExchange Exchange { get => m_oExchange; }
        public IFuturesWebsocketPublic? Websocket { get => throw new NotImplementedException(); }
        public async Task<bool> StartSockets()
        {
            throw new NotImplementedException();
        }
        public async Task<bool> EndSockets()
        {
            throw new NotImplementedException();
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


        /// <summary>
        /// Get symbol list
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesSymbol[]?> GetSymbols()
        {
            if (m_aSymbols != null) return m_aSymbols;
            var oResult = await m_oGlobalClient.CoinEx.FuturesApi.ExchangeData.GetSymbolsAsync();
            if (oResult == null || !oResult.Success) return null;

            if (oResult.Data == null) return null;
            List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
            foreach (CoinExFuturesSymbol oParsed in oResult.Data)
            {
                aResult.Add(new CoinexSymbol(this.Exchange, oParsed));
            }
            m_aSymbols = aResult.ToArray();
            return m_aSymbols;

        }
    }
}
