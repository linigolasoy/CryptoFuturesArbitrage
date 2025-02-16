using Bitget.Net.Enums;
using Bitget.Net.Enums.V2;
using Crypto.Common;
using Crypto.Exchanges.All.Bingx;
using Crypto.Exchanges.All.Common;
using Crypto.Exchanges.All.Common.Storage;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using CryptoClients.Net;
using CryptoClients.Net.Interfaces;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetHistory : IFuturesHistory
    {
        private BitgetFutures m_oExchange;
        private IExchangeRestClient m_oGlobalClient;
        private IFuturesBarFeeder m_oBarFeeder;
        public BitgetHistory(BitgetFutures oExchange)
        {
            m_oExchange = oExchange;
            m_oGlobalClient = new ExchangeRestClient();
            m_oBarFeeder = new BaseBarFeeder(m_oExchange);
            m_oBarFeeder.OnGetBarsDay += OnGetBarsDay;

        }


        private BitgetFuturesKlineInterval? TimeframeToBitget(Timeframe eFrame )
        {
            switch( eFrame )
            {
                case Timeframe.M1:
                    return BitgetFuturesKlineInterval.OneMinute;
                case Timeframe.M5:
                    return BitgetFuturesKlineInterval.FiveMinutes;
                case Timeframe.M15:
                    return BitgetFuturesKlineInterval.FifteenMinutes;
                case Timeframe.M30:
                    return BitgetFuturesKlineInterval.ThirtyMinutes;
                case Timeframe.H1:
                    return BitgetFuturesKlineInterval.OneHour;
                case Timeframe.H4:
                    return BitgetFuturesKlineInterval.FourHours;
                case Timeframe.D1:
                    return BitgetFuturesKlineInterval.OneDay;
                default:
                    return null;
            }
        }


        private async Task<IFuturesBar[]?> OnGetBarsDay(IFuturesSymbol oSymbol, Timeframe eTimeframe, DateTime dDate)
        {
            BitgetFuturesKlineInterval? eInterval = TimeframeToBitget(eTimeframe);
            if (eInterval == null) return null;
            int nDays = CommonFactory.DaysFromTimeframe(eTimeframe);

            DateTime dFrom = dDate.Date.AddHours(-5);
            DateTime dTo = dDate.Date.AddDays(1).AddHours(5);
            List<IFuturesBar> aResult = new List<IFuturesBar>();
            while (dDate.Date <= dTo.Date)
            {
                DateTime dToActual = dFrom.Date.AddDays(nDays).AddSeconds(-1);
                if (dToActual > dTo) dToActual = dTo.Date.AddDays(1).AddSeconds(-1);

                var oResult = await m_oGlobalClient.Bitget.FuturesApiV2.ExchangeData.GetKlinesAsync(
                    BitgetProductTypeV2.UsdtFutures,
                    oSymbol.Symbol,
                    eInterval.Value,
                    KlineType.Market,
                    dFrom,
                    dToActual,
                    1000);
                    //. .GetKlinesAsync(oSymbol.Symbol, eInterval.Value, dFrom, dToActual, 1000);
                if (oResult == null || !oResult.Success) break;
                if (oResult.Data == null || oResult.Data.Count() <= 0) break;
                
                foreach (var oData in oResult.Data)
                {
                    IFuturesBar oBar = new BitgetBar(oSymbol, eTimeframe, oData);
                    if (!aResult.Any(p => p.DateTime == oBar.DateTime))
                    {
                        aResult.Add(oBar);
                    }
                }
                
                dFrom = dToActual.Date.AddDays(1).Date;
            }

            return aResult.Where(p => p.DateTime.Date == dDate.Date).ToArray();

        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol oSymbol, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            return await GetBars( new IFuturesSymbol[] {oSymbol}, eTimeframe, dFrom, dTo);  
        }

        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol[] aSymbols, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            return await m_oBarFeeder.GetBars(aSymbols, eTimeframe, dFrom, dTo);
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
