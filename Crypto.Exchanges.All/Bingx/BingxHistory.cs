using BingX.Net.Enums;
using BingX.Net.Objects.Models;
using Crypto.Common;
using Crypto.Exchanges.All.Common;
using Crypto.Exchanges.All.Common.Storage;
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
        private IFuturesBarFeeder m_oBarFeeder;
        private IFundingRateFeeder m_oFundingFeeder;
        public BingxHistory(BingxFutures oExchange) 
        {
            m_oExchange = oExchange;
            m_oBarFeeder = new BaseBarFeeder(m_oExchange);
            m_oBarFeeder.OnGetBarsDay += OnGetBarsDay;
            m_oFundingFeeder = new BaseFundingFeeder(m_oExchange);
            m_oFundingFeeder.OnGetFunding += OnGetFunding;
        }

        /// <summary>
        /// Get funding rates from web
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="dFrom"></param>
        /// <returns></returns>
        private async Task<IFundingRate[]?> OnGetFunding(IFuturesSymbol oSymbol, DateTime dFrom)
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

        private KlineInterval? TimeframeToBingX(Timeframe eFrame)
        {
            switch (eFrame)
            {
                case Timeframe.M1:
                    return KlineInterval.OneMinute;
                case Timeframe.M5:
                    return KlineInterval.FiveMinutes;
                case Timeframe.M15:
                    return KlineInterval.FifteenMinutes;
                case Timeframe.M30:
                    return KlineInterval.ThirtyMinutes;
                case Timeframe.H1:
                    return KlineInterval.OneHour;
                case Timeframe.H4:
                    return KlineInterval.FourHours;
                case Timeframe.D1:
                    return KlineInterval.OneDay;
                default:
                    return null;
            }
        }

        private async Task<IFuturesBar[]?> OnGetBarsDay(IFuturesSymbol oSymbol, Timeframe eTimeframe, DateTime dDate)
        {
            KlineInterval? eInterval = TimeframeToBingX(eTimeframe);
            if (eInterval == null) return null;
            int nDays = CommonFactory.DaysFromTimeframe(eTimeframe);

            DateTime dFrom = dDate.Date.AddHours(-5);
            DateTime dTo = dDate.Date.AddDays(1).AddHours(5);
            List<IFuturesBar> aResult = new List<IFuturesBar>();
            while (dDate.Date <= dTo.Date)
            {
                DateTime dToActual = dFrom.Date.AddDays(nDays).AddSeconds(-1);
                if (dToActual > dTo) dToActual = dTo.Date.AddDays(1).AddSeconds(-1);

                var oResult = await m_oExchange.GlobalClient.BingX.PerpetualFuturesApi.ExchangeData.GetKlinesAsync(oSymbol.Symbol, eInterval.Value, dFrom, dToActual, 1000);
                if (oResult == null || !oResult.Success) break;
                if (oResult.Data == null || oResult.Data.Count() <= 0) break;

                foreach (BingXFuturesKline oData in oResult.Data)
                {
                    IFuturesBar oBar = new BingxBar(oSymbol, eTimeframe, oData);
                    if( !aResult.Any(p=> p.DateTime == oBar.DateTime) )
                    {
                        aResult.Add(oBar);
                    }
                }
                dFrom= dToActual.Date.AddDays(1).Date;
            }

            return aResult.Where(p=> p.DateTime.Date == dDate.Date).ToArray();
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
            return await m_oFundingFeeder.GetFundingRatesHistory(oSymbol, dFrom);   
            /*
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
            */
        }

        /// <summary>
        /// Get funding rate history, multiple symbols
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol[] aSymbols, DateTime dFrom)
        {
            return await m_oFundingFeeder.GetFundingRatesHistory(aSymbols, dFrom);    
            /*
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
            */
        }
    }
}
