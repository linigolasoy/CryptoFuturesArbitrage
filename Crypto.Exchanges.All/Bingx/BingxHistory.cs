using BingX.Net.Objects.Models;
using Crypto.Common;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxHistory : IFuturesHistory
    {

        private BingxFutures m_oExchange;
        private BingxBarFeeder m_oBarFeeder;
        public BingxHistory(BingxFutures oExchange) 
        {
            m_oExchange = oExchange;
            m_oBarFeeder = new BingxBarFeeder(m_oExchange);
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol oSymbol, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            return await m_oBarFeeder.GetBars(oSymbol, eTimeframe, dFrom, dTo);
        }

        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol[] aSymbols, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            return await m_oBarFeeder.GetBars(aSymbols, eTimeframe, dFrom, dTo);
        }

        /// <summary>
        /// Gets funding rates
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol oSymbol, DateTime dFrom)
        {
            DateTime dFromActual = dFrom.Date;
            DateTime dToActual = DateTime.Now;

            int nLimit = 1000;

            List<IFundingRate> aResult = new List<IFundingRate>();
            while (true)
            {
                var oResult = await m_oExchange.GlobalClient.BingX.PerpetualFuturesApi.ExchangeData.GetFundingRateHistoryAsync(oSymbol.Symbol, dFromActual, dToActual, nLimit);
                if (oResult == null || !oResult.Success) break;
                if (oResult.Data == null) break;

                List<IFundingRate> aPartial = new List<IFundingRate>();

                foreach (BingXFundingRateHistory oData in oResult.Data)
                {
                    aPartial.Add(new BingxFundingRate(oSymbol, oData));
                }

                if (aPartial.Count <= 0) break;
                DateTime dMinimum = aPartial.Select(p => p.SettleDate).Min();
                dToActual = dMinimum.AddHours(-1);
                aResult.AddRange(aPartial);
                if (dMinimum.Date <= dFromActual.Date) break;
                if (aPartial.Count < nLimit) break;

            }

            return aResult.ToArray();
        }

        /// <summary>
        /// Get funding rate history, multiple symbols
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol[] aSymbols, DateTime dFrom)
        {

            ITaskManager<IFundingRate[]?> oTaskManager = CommonFactory.CreateTaskManager<IFundingRate[]?>(BingxFutures.TASK_COUNT);
            List<IFundingRate> aResult = new List<IFundingRate>();

            foreach (IFuturesSymbol oSymbol in aSymbols)
            {
                await oTaskManager.Add(GetFundingRatesHistory(oSymbol, dFrom));
            }

            var aTaskResults = await oTaskManager.GetResults();
            if (aTaskResults == null) return null;
            foreach (var oResult in aTaskResults)
            {
                if (oResult == null || oResult.Length <= 0) continue;
                aResult.AddRange(oResult);
            }
            return aResult.ToArray();
        }
    }
}
