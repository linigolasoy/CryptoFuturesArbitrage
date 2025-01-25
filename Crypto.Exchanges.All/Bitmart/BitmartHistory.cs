using BitMart.Net.Objects.Models;
using Crypto.Common;
using Crypto.Exchanges.All.Bingx;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using CryptoClients.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitmart
{
    internal class BitmartHistory : IFuturesHistory
    {
        private BitmartFutures m_oExchange;
        private IExchangeRestClient m_oGlobalClient;
        public BitmartHistory(BitmartFutures oExchange) 
        {
            m_oExchange = oExchange;
            m_oGlobalClient = oExchange.GlobalClient;
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
            int nLimit = 100;

            List<IFundingRate> aResult = new List<IFundingRate>();
            var oResult = await m_oGlobalClient.BitMart.UsdFuturesApi.ExchangeData.GetFundingRateHistoryAsync(oSymbol.Symbol, nLimit);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;

            foreach (BitMartFundingRateHistory oData in oResult.Data)
            {
                aResult.Add(new BitmartFundingRateLocal(oSymbol, oData));
            }
            if (aResult.Count <= 0) return null;

            return aResult.ToArray();
        }

        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol[] aSymbols, DateTime dFrom)
        {
            ITaskManager<IFundingRate[]?> oTaskManager = CommonFactory.CreateTaskManager<IFundingRate[]?>(BitmartFutures.TASK_COUNT);
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
