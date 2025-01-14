using BingX.Net.Enums;
using BingX.Net.Objects.Models;
using Crypto.Common;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxBarFeeder : IFuturesBarFeeder
    {

        private BingxFutures m_oExchange;
        public BingxBarFeeder( BingxFutures oExchange ) 
        { 
            m_oExchange = oExchange;
        }
        public IFuturesExchange Exchange { get => m_oExchange; }


        private KlineInterval? TimeframeToBingX( Timeframe eFrame )
        {
            switch( eFrame )
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

        /// <summary>
        /// Get bars, single symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="eTimeframe"></param>
        /// <param name="dFrom"></param>
        /// <param name="dTo"></param>
        /// <returns></returns>
        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol oSymbol, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            KlineInterval? eInterval = TimeframeToBingX(eTimeframe);
            if( eInterval == null ) return null;
            int nDays = CommonFactory.DaysFromTimeframe(eTimeframe);

            DateTime dFromActual = dFrom.Date;
            List<IFuturesBar> aResult = new List<IFuturesBar>();
            while (dFromActual.Date <= dTo.Date)
            {
                DateTime dToActual = dFromActual.Date.AddDays(nDays).AddSeconds(-1);
                if (dToActual > dTo) dToActual = dTo.Date.AddDays(1).AddSeconds(-1);

                var oResult = await m_oExchange.GlobalClient.BingX.PerpetualFuturesApi.ExchangeData.GetKlinesAsync(oSymbol.Symbol, eInterval.Value, dFromActual, dToActual,1000);
                if (oResult == null || !oResult.Success) break;
                if (oResult.Data == null || oResult.Data.Count() <= 0) break;

                foreach( BingXFuturesKline oData in oResult.Data )
                {
                    aResult.Add(new BingxBar(oSymbol, eTimeframe, oData));
                }
                dFromActual = dToActual.Date.AddDays(1).Date;
            }

            return aResult.ToArray();
        }

        /// <summary>
        /// Get bars, multiple symbols
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <param name="eTimeframe"></param>
        /// <param name="dFrom"></param>
        /// <param name="dTo"></param>
        /// <returns></returns>
        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol[] aSymbols, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            ITaskManager<IFuturesBar[]?> oTaskManager = CommonFactory.CreateTaskManager<IFuturesBar[]?>(BingxFutures.TASK_COUNT);
            List<IFuturesBar> aResult = new List<IFuturesBar>();

            foreach (IFuturesSymbol oSymbol in aSymbols)
            {
                await oTaskManager.Add(GetBars(oSymbol, eTimeframe, dFrom, dTo));
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
