using Crypto.Common;
using Crypto.Exchange.Mexc.Futures;
using Crypto.Exchange.Mexc.Responses;
using Crypto.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc
{

    /// <summary>
    /// Mexc Futures exchange
    /// </summary>
    public class MexcFuturesExchange : ICryptoFuturesExchange
    {

        private const string ENDPOINT_CONTRACTS = "api/v1/contract/detail";
        private const string ENDPOINT_FUNDING = "{0}api/v1/contract/funding_rate/{1}";
        private const string ENDPOINT_FUNDING_HISTORY = "{0}api/v1/contract/funding_rate/history?symbol={1}&page_num={2}&page_size=100";
        private const string ENDPOINT_BARS = "{0}api/v1/contract/kline/{1}?interval={2}&start={3}&end={4}";


        private const int TASK_COUNT = 20;        
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


        private static IRequestHelper m_oRequestHelper = CommonFactory.CreateRequestHelper(MexcCommon.GetHttpClient(), 300);
        private enum eTags
        {
            data
        }

        public MexcFuturesExchange( ICryptoSetup setup)
        {
            Setup = setup;
        }

        public ICryptoSetup Setup { get; }

        /// <summary>
        /// Simple url-based get request
        /// </summary>
        /// <param name="strUrl"></param>
        /// <returns></returns>
        private async Task<ResponseFutures?> PerformGet( string strUrl )
        {
            string? strResult = await m_oRequestHelper.GetRequest( strUrl );
            if (strResult == null) return null;

            // HttpClient oClient = MexcCommon.GetHttpClient();

            // HttpResponseMessage oResponse = await oClient.GetAsync(strUrl);
            // if (!oResponse.IsSuccessStatusCode) return null;
            // string strResponse = await oResponse.Content.ReadAsStringAsync();

            ResponseFutures? oResult = JsonConvert.DeserializeObject<ResponseFutures?>(strResult);
            return oResult;

        }

        /// <summary>
        /// Get futures symbols
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesSymbol[]?> GetSymbols()
        {
            string strUrl = MexcCommon.URL_FUTURES_BASE + ENDPOINT_CONTRACTS;
            ResponseFutures? oResponse = await PerformGet(strUrl);  
            if( oResponse == null || !oResponse.Success || oResponse.Data == null ) return null;
            if (!(oResponse.Data is JArray)) return null;
            JArray oArray = (JArray)oResponse.Data;


            List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
            foreach (JToken oToken in oArray)
            {
                if (!(oToken is JObject)) continue;
                JObject oJsonSymbol = (JObject)oToken;
                IFuturesSymbol? oSymbol = FuturesSymbol.Create(oJsonSymbol); 
                if (oSymbol == null) continue;
                aResult.Add(oSymbol);

            }

            return aResult.ToArray();
        }

        /// <summary>
        /// Funding rate of single symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<IFundingRateSnapShot?> GetFundingRates(IFuturesSymbol oSymbol)
        {
            // IFundingRate[]? aResult = await GetFundingRates( new IFuturesSymbol[] { oSymbol });
            // if (aResult == null || aResult.Length <= 0 ) return null;
            string strUrl = string.Format(ENDPOINT_FUNDING, MexcCommon.URL_FUTURES_BASE, oSymbol.Symbol);
            // https://contract.mexc.com/api/v1/contract/funding_rate/BTC_USDT

            ResponseFutures? oResponse = await PerformGet(strUrl);
            if (oResponse == null || !oResponse.Success || oResponse.Data == null) return null;

            if (!(oResponse.Data is JObject)) return null;
            JObject oData = (JObject)oResponse.Data;
            FuturesFundingParsed? oParsed = oData.ToObject<FuturesFundingParsed>();
            if( oParsed == null ) return null;
            return new FuturesFundingStapshot(oSymbol, oParsed);
        }

        /// <summary>
        /// Returns funding rates of multiple symbols
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRateSnapShot[]?> GetFundingRates(IFuturesSymbol[] aSymbols)
        {
            ITaskManager<IFundingRateSnapShot?> oTaskManager = CommonFactory.CreateTaskManager<IFundingRateSnapShot?>(TASK_COUNT);

            List<IFundingRateSnapShot> aResult = new List<IFundingRateSnapShot>();

            foreach (IFuturesSymbol oSymbol in aSymbols)
            {
                await oTaskManager.Add(GetFundingRates(oSymbol));   
            }
            var aTaskResults = await oTaskManager.GetResults();
            if (aTaskResults == null) return null;
            foreach (var oResult in aTaskResults)
            {
                if (oResult == null ) continue;
                aResult.Add(oResult);
            }


            return aResult.ToArray();

        }


        /// <summary>
        /// Get funding rate history single symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>

        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol oSymbol)
        {
            // IFundingRate[]? aResult = await GetFundingRates( new IFuturesSymbol[] { oSymbol });
            // if (aResult == null || aResult.Length <= 0 ) return null;

            int nPage = 1;
            int nTotal = -1;

            List<IFundingRate> aResult = new List<IFundingRate>();

            while (nTotal < 0 || nPage <= nTotal)
            {
                string strUrl = string.Format(ENDPOINT_FUNDING_HISTORY, MexcCommon.URL_FUTURES_BASE, oSymbol.Symbol, nPage);
                // https://contract.mexc.com/api/v1/contract/funding_rate/BTC_USDT

                ResponseFutures? oResponse = await PerformGet(strUrl);
                if (oResponse == null || !oResponse.Success || oResponse.Data == null) return null;

                if (!(oResponse.Data is JObject)) return null;
                JObject oData = (JObject)oResponse.Data;
                FundingHistoryPageParsed? oParsed = oData.ToObject<FundingHistoryPageParsed>();
                if (oParsed == null) continue;

                if( nTotal < 0 )
                {
                    nTotal = oParsed.Pages;
                }
                if (oParsed.History == null || oParsed.History.Count <= 0) continue;

                foreach( var oHist  in oParsed.History ) 
                { 
                    aResult.Add(new FuturesFunding(oSymbol, oHist));    
                }
                // return new FuturesFundingStapshot(oSymbol, oParsed);
                // throw new NotImplementedException();    
                nPage++;
            }
            return aResult.ToArray();   
        }

        /// <summary>
        /// Funding rate history multi symbol
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol[] aSymbols)
        {
            ITaskManager<IFundingRate[]?> oTaskManager = CommonFactory.CreateTaskManager<IFundingRate[]?>(TASK_COUNT);

            List<IFundingRate> aResult = new List<IFundingRate>();

            foreach (IFuturesSymbol oSymbol in aSymbols)
            {
                await oTaskManager.Add( GetFundingRatesHistory(oSymbol));
            }

            var aTaskResults = await oTaskManager.GetResults();
            if (aTaskResults == null) return null;
            foreach( var oResult in aTaskResults )
            {
                if( oResult == null || oResult.Length <= 0 ) continue;  
                aResult.AddRange( oResult );    
            }
            return aResult.ToArray();
        }

        /// <summary>
        /// Number of days in single request
        /// </summary>
        /// <param name="eFrame"></param>
        /// <returns></returns>
        private int DaysFromTimeframe(Timeframe eFrame )
        {
            switch(eFrame)
            {
                case Timeframe.M1:
                    return 1;
                case Timeframe.M5:
                    return 5;
                case Timeframe.M15:
                    return 15;
                case Timeframe.H1:
                    return 60;
                case Timeframe.H4:
                    return 240;
                case Timeframe.D1:
                    return 365 * 2;
            }
            return 0;
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
        /// Get bars
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="eTimeframe"></param>
        /// <param name="dFrom"></param>
        /// <param name="dTo"></param>
        /// <returns></returns>
        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol oSymbol, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            int nDays = DaysFromTimeframe(eTimeframe);
            string? strMexcFrame = TimeframeToMexc(eTimeframe);

            if (nDays <= 0) return null;
            if (strMexcFrame == null) return null;
            DateTime dFromActual = dFrom.Date;
            List<IFuturesBar> aResult = new List<IFuturesBar> ();   
            while( dFromActual.Date <= dTo.Date )
            {
                DateTime dToActual = dFromActual.Date.AddDays(nDays).AddSeconds(-1);
                if (dToActual > dTo) dToActual = dTo.Date.AddDays(1).AddSeconds(-1);

                long nOffsetFrom = (new DateTimeOffset(dFromActual.ToUniversalTime())).ToUnixTimeSeconds();
                long nOffsetTo = (new DateTimeOffset(dToActual.ToUniversalTime())).ToUnixTimeSeconds();
                string strUrl = string.Format(ENDPOINT_BARS, MexcCommon.URL_FUTURES_BASE, oSymbol.Symbol, strMexcFrame, nOffsetFrom, nOffsetTo);

                ResponseFutures? oResponse = await PerformGet(strUrl);
                if (oResponse == null || !oResponse.Success || oResponse.Data == null) return null;
                if( !(oResponse.Data is JObject)) return null;
                JObject oData = (JObject)oResponse.Data;    
                FuturesBarParsed? oParsed = oData.ToObject<FuturesBarParsed>(); 
                if( oParsed == null ) return null;
                if (oParsed.Open == null || oParsed.Close == null || oParsed.High == null || oParsed.Low == null || oParsed.Volume == null || oParsed.Times == null) return null;
                if (oParsed.Times.Count != oParsed.Open.Count || 
                    oParsed.Open.Count  != oParsed.High.Count || 
                    oParsed.High.Count  != oParsed.Low.Count ||
                    oParsed.Low.Count   != oParsed.Close.Count ||
                    oParsed.Close.Count != oParsed.Volume.Count ) return null;
                for( int i = 0; i < oParsed.Times.Count; i++ )
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
        /// Multi symbol bars
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <param name="eTimeframe"></param>
        /// <param name="dFrom"></param>
        /// <param name="dTo"></param>
        /// <returns></returns>
        public async Task<IFuturesBar[]?> GetBars(IFuturesSymbol[] aSymbols, Timeframe eTimeframe, DateTime dFrom, DateTime dTo)
        {
            ITaskManager<IFuturesBar[]?> oTaskManager = CommonFactory.CreateTaskManager<IFuturesBar[]?>( TASK_COUNT );
            List<IFuturesBar> aResult = new List<IFuturesBar>();    

            foreach(IFuturesSymbol oSymbol in aSymbols)
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
