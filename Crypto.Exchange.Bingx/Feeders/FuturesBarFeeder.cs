using Crypto.Common;
using Crypto.Exchange.Bingx.Futures;
using Crypto.Exchange.Bingx.Responses;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx.Feeders
{
    internal class FuturesBarFeeder : IFuturesBarFeeder
    {
        private const string ENDPOINT_BARS = "openApi/swap/v3/quote/klines";

        private BingxFuturesExchange m_oExchange;

        public FuturesBarFeeder( BingxFuturesExchange oExchange )
        {
            m_oExchange = oExchange;    
        }
        public ICryptoFuturesExchange Exchange { get => m_oExchange; }

        /// <summary>
        /// Timeframe to string
        /// </summary>
        /// <param name="eTimeframe"></param>
        /// <returns></returns>
        private string? TimeframeToBingx(Timeframe eTimeframe)
        {
            switch (eTimeframe)
            {
                case Timeframe.M1:
                    return "1m";
                case Timeframe.M5:
                    return "5m";
                case Timeframe.M15:
                    return "15m";
                case Timeframe.M30:
                    return "30m";
                case Timeframe.H1:
                    return "1h";
                case Timeframe.H4:
                    return "4h";
                case Timeframe.D1:
                    return "1d";
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
            string? strFrame = TimeframeToBingx(eTimeframe);

            if (nDays <= 0) return null;
            if (strFrame == null) return null;
            DateTime dFromActual = dFrom.Date;
            List<IFuturesBar> aResult = new List<IFuturesBar>();
            while (dFromActual.Date <= dTo.Date)
            {
                DateTime dToActual = dFromActual.Date.AddDays(nDays).AddSeconds(-1);
                if (dToActual > dTo) dToActual = dTo.Date.AddDays(1).AddSeconds(-1);


                long nOffsetFrom = (new DateTimeOffset(dFromActual.ToUniversalTime())).ToUnixTimeMilliseconds();
                long nOffsetTo = (new DateTimeOffset(dToActual.ToUniversalTime())).ToUnixTimeMilliseconds();
                var oParameters = new
                {
                    symbol = oSymbol.Symbol,
                    interval = strFrame,
                    startTime = nOffsetFrom,
                    endTime = nOffsetTo,
                    limit = 1440
                };

                ResponseFutures? oResult = await BingxFuturesExchange.DoPublicGet(ENDPOINT_BARS, oParameters);
                if (oResult == null || oResult.Code != 0 || !string.IsNullOrEmpty(oResult.Message)) break;
                if (!(oResult.Data is JArray)) continue;
                JArray oArray = (JArray)oResult.Data;

                foreach (var oToken in oArray)
                {
                    if (!(oToken is JObject)) continue;
                    JObject oObject = (JObject)oToken;
                    FuturesBarParsed? oParsed = oObject.ToObject<FuturesBarParsed>();
                    if (oParsed == null) continue;
                    IFuturesBar oBar = new FuturesBar(oSymbol, eTimeframe, oParsed);
                    if (aResult.Any(p => p.DateTime == oBar.DateTime)) continue;
                    aResult.Add(oBar);

                }
                dFromActual = dToActual.Date.AddDays(1).Date;
            }

            return aResult.ToArray();
        }

        /// <summary>
        /// Get bars multiple
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <param name="eTimeframe"></param>
        /// <param name="dFrom"></param>
        /// <param name="dTo"></param>
        /// <returns></returns>
        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol[] aSymbols, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            ITaskManager<IFuturesBar[]?> oTaskManager = CommonFactory.CreateTaskManager<IFuturesBar[]?>(BingxFuturesExchange.TASK_COUNT);
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
