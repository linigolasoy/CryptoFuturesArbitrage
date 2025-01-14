using CoinEx.Net.Objects.Models.V2;
using Crypto.Common;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using CryptoClients.Net;
using CryptoClients.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexHistory : IFuturesHistory
    {
        private CoinexFutures m_oExchange;
        private IExchangeRestClient m_oGlobalClient;

        public CoinexHistory(CoinexFutures oExchange)
        {
            m_oExchange = oExchange;
            m_oGlobalClient = new ExchangeRestClient();
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol oSymbol, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            throw new NotImplementedException();
        }

        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol[] aSymbols, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            throw new NotImplementedException();
        }

        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol oSymbol, DateTime dFrom)
        {

            DateTime dFromActual = dFrom.Date;
            DateTime dToActual = DateTime.Now;

            int nLimit = 1000;
            int nPage = 1;
            List<IFundingRate> aResult = new List<IFundingRate>();
            while (true)
            {
                var oResult = await m_oGlobalClient.CoinEx.FuturesApi.ExchangeData.GetFundingRateHistoryAsync(oSymbol.Symbol, dFromActual, dToActual, nPage, nLimit);
                if (oResult == null || !oResult.Success) break;
                if (oResult.Data == null) break;

                if (oResult.Data.Items != null && oResult.Data.Items.Count() > 0)
                {
                    foreach (CoinExFundingRateHistory oData in oResult.Data.Items)
                    {
                        if (oData.FundingTime == null) continue;
                        aResult.Add(new CoinexFundingRate(oSymbol, oData));
                    }
                }

                if (!oResult.Data.HasNext) break;
                nPage++;

            }
            return aResult.ToArray();
        }

        /// <summary>
        /// Multi symbol history
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol[] aSymbols, DateTime dFrom)
        {
            ITaskManager<IFundingRate[]?> oTaskManager = CommonFactory.CreateTaskManager<IFundingRate[]?>(CoinexFutures.TASK_COUNT);
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
