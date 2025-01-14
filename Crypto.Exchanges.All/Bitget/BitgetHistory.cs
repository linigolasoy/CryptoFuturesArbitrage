using Bitget.Net;
using Bitget.Net.Enums;
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

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetHistory : IFuturesHistory
    {
        private BitgetFutures m_oExchange;
        private IExchangeRestClient m_oGlobalClient;

        public BitgetHistory(BitgetFutures oExchange)
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

        /// <summary>
        /// Get funding rate history on single symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol oSymbol, DateTime dFrom)
        {
            int nPage = 1;
            int nPageSize = 100;
            DateTime dLimit = dFrom.Date;
            List<IFundingRate> aResult = new List<IFundingRate>();

            bool bLimit = false;
            while (!bLimit)
            {
                var oResult = await m_oGlobalClient.Bitget.FuturesApiV2.ExchangeData.GetHistoricalFundingRateAsync(BitgetProductTypeV2.UsdtFutures, oSymbol.Symbol, nPageSize, nPage);
                if (oResult == null || oResult.Data == null) break;
                if (!oResult.Success) break;
                if (oResult.Data.Count() <= 0) break;

                foreach (var oData in oResult.Data)
                {
                    if (oData.FundingTime == null) continue;
                    aResult.Add(new BitgetFuturesFundingRate(oSymbol, oData));
                    if (oData.FundingTime.Value.Date <= dLimit)
                    {
                        bLimit = true;
                        break;
                    }
                }
                nPage++;
            }


            return aResult.ToArray();
        }

        /// <summary>
        /// Funding rate history multiple
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol[] aSymbols, DateTime dFrom)
        {
            ITaskManager<IFundingRate[]?> oTaskManager = CommonFactory.CreateTaskManager<IFundingRate[]?>(BitgetFutures.TASK_COUNT);
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
