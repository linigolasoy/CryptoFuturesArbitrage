using Crypto.Common;
using Crypto.Exchange.Mexc.Futures;
using Crypto.Exchange.Mexc.Responses;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Feeders
{
    internal class FuturesBarFeeder : IFuturesBarFeeder
    {

        private const string ENDPOINT_BARS = "{0}api/v1/contract/kline/{1}?interval={2}&start={3}&end={4}";

        private MexcFuturesExchange m_oExchange;

        public FuturesBarFeeder( MexcFuturesExchange oExchange)
        {
            m_oExchange = oExchange;
        }
    
        public ICryptoFuturesExchange Exchange { get => m_oExchange; }


        private enum eMexcFrames
        {
            Min1,
            Min5,
            Min15,
            Min30,
            Min60,
            Hour4,
            Hour8,
            Day1,
            Week1,
            Month1
        }

        /// <summary>
        /// Conver to timeframe request
        /// </summary>
        /// <param name="eFrame"></param>
        /// <returns></returns>
        private string? TimeframeToMexc(Timeframe eFrame)
        {
            // Min1、Min5、Min15、Min30、Min60、Hour4、Hour8、Day1、Week1、Month1,default: Min1
            switch (eFrame)
            {
                case Timeframe.M1:
                    return eMexcFrames.Min1.ToString();
                case Timeframe.M5:
                    return eMexcFrames.Min5.ToString();
                case Timeframe.M15:
                    return eMexcFrames.Min15.ToString();
                case Timeframe.H1:
                    return eMexcFrames.Min60.ToString();
                case Timeframe.H4:
                    return eMexcFrames.Hour4.ToString();
                case Timeframe.D1:
                    return eMexcFrames.Day1.ToString();
            }
            return null;
        }

        /// <summary>
        /// Get bars single symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="eTimeframe"></param>
        /// <param name="dFrom"></param>
        /// <param name="dTo"></param>
        /// <returns></returns>
        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol oSymbol, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            int nDays = CommonFactory.DaysFromTimeframe(eTimeframe);
            string? strMexcFrame = TimeframeToMexc(eTimeframe);

            if (nDays <= 0) return null;
            if (strMexcFrame == null) return null;
            DateTime dFromActual = dFrom.Date;
            List<IFuturesBar> aResult = new List<IFuturesBar>();
            while (dFromActual.Date <= dTo.Date)
            {
                DateTime dToActual = dFromActual.Date.AddDays(nDays).AddSeconds(-1);
                if (dToActual > dTo) dToActual = dTo.Date.AddDays(1).AddSeconds(-1);

                long nOffsetFrom = (new DateTimeOffset(dFromActual.ToUniversalTime())).ToUnixTimeSeconds();
                long nOffsetTo = (new DateTimeOffset(dToActual.ToUniversalTime())).ToUnixTimeSeconds();
                string strUrl = string.Format(ENDPOINT_BARS, MexcCommon.URL_FUTURES_BASE, oSymbol.Symbol, strMexcFrame, nOffsetFrom, nOffsetTo);

                ResponseFutures? oResponse = await m_oExchange.PerformGet(strUrl);
                if (oResponse == null || !oResponse.Success || oResponse.Data == null) return null;
                if (!(oResponse.Data is JObject)) return null;
                JObject oData = (JObject)oResponse.Data;
                FuturesBarParsed? oParsed = oData.ToObject<FuturesBarParsed>();
                if (oParsed == null) return null;
                if (oParsed.Open == null || oParsed.Close == null || oParsed.High == null || oParsed.Low == null || oParsed.Volume == null || oParsed.Times == null) return null;
                if (oParsed.Times.Count != oParsed.Open.Count ||
                    oParsed.Open.Count != oParsed.High.Count ||
                    oParsed.High.Count != oParsed.Low.Count ||
                    oParsed.Low.Count != oParsed.Close.Count ||
                    oParsed.Close.Count != oParsed.Volume.Count) return null;
                for (int i = 0; i < oParsed.Times.Count; i++)
                {
                    IFuturesBar oNewBar =
                        new FuturesBar(
                            oSymbol,
                            eTimeframe,
                            oParsed.Times[i],
                            oParsed.Open[i],
                            oParsed.High[i],
                            oParsed.Low[i],
                            oParsed.Close[i],
                            oParsed.Volume[i]
                        );
                    if (aResult.Any(p => p.DateTime == oNewBar.DateTime)) continue;
                    aResult.Add(oNewBar);
                }

                dFromActual = dToActual.Date.AddDays(1).Date;
            }
            // "{0}api/v1/contract/kline/{1}?interval={2}&start={3}&end={4}"

            return aResult.ToArray();
        }

        /// <summary>
        /// Get bars multiple symbols
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <param name="eTimeframe"></param>
        /// <param name="dFrom"></param>
        /// <param name="dTo"></param>
        /// <returns></returns>
        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol[] aSymbols, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            ITaskManager<IFuturesBar[]?> oTaskManager = CommonFactory.CreateTaskManager<IFuturesBar[]?>(MexcFuturesExchange.TASK_COUNT);
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
