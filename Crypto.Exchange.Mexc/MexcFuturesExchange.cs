using Crypto.Common;
using Crypto.Exchange.Mexc.Feeders;
using Crypto.Exchange.Mexc.Futures;
using Crypto.Exchange.Mexc.Responses;
using Crypto.Exchange.Mexc.Websocket;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
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
    public partial class MexcFuturesExchange : ICryptoFuturesExchange
    {

        private const string ENDPOINT_CONTRACTS = "api/v1/contract/detail";
        private const string ENDPOINT_FUNDING = "{0}api/v1/contract/funding_rate/{1}";
        private const string ENDPOINT_FUNDING_HISTORY = "{0}api/v1/contract/funding_rate/history?symbol={1}&page_num={2}&page_size=100";


        internal const int TASK_COUNT = 20;        


        private static IRequestHelper m_oRequestHelper = CommonFactory.CreateRequestHelper(MexcCommon.GetHttpClient(), 300);
        private IFuturesBarFeeder m_oBarFeeder;
        private enum eTags
        {
            data
        }

        public MexcFuturesExchange( ICryptoSetup setup)
        {
            Setup = setup;
            m_oBarFeeder = new FuturesBarFeeder(this);
        }

        public ICryptoSetup Setup { get; }
        public ExchangeType ExchangeType { get => ExchangeType.MexcFutures; }

        public IFuturesBarFeeder BarFeeder { get => m_oBarFeeder; }

        /// <summary>
        /// Simple url-based get request
        /// </summary>
        /// <param name="strUrl"></param>
        /// <returns></returns>
        internal async Task<ResponseFutures?> PerformGet( string strUrl )
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


        public async Task<ISymbol[]?> GetRawSymbols()
        {
            return await GetSymbols();  
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



        public async Task<ICryptoWebsocket?> CreateWebsocket()
        {
            return new MexcFuturesWebsocket(this);
        }

        public async Task<IFuturesBalance[]?> GetBalances()
        {
            throw new NotImplementedException();
        }

    }
}
