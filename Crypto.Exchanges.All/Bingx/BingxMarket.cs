using BingX.Net.Objects.Models;
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
        private IFuturesSymbol[]? m_aSymbols = null;
        public BingxMarket( BingxFutures oExchange ) 
        { 
            m_oExchange = oExchange;
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

        /// <summary>
        /// Get futures symbols
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesSymbol[]?> GetSymbols()
        {
            if (m_aSymbols != null) return m_aSymbols;
            var oResult = await m_oExchange.GlobalClient.BingX.PerpetualFuturesApi.ExchangeData.GetContractsAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.Count() <= 0) return null;

            List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
            foreach (BingXContract oData in oResult.Data)
            {
                aResult.Add(new BingxSymbol(this.Exchange, oData));
            }

            m_aSymbols = aResult.ToArray();
            return m_aSymbols;
        }
    }
}
