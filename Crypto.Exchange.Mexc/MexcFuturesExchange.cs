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

        private const string ENDPOINT_CONTRACTS = "/api/v1/contract/detail";

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
            HttpClient oClient = MexcCommon.GetHttpClient();

            HttpResponseMessage oResponse = await oClient.GetAsync(MexcCommon.URL_FUTURES_BASE + ENDPOINT_CONTRACTS);
            if (!oResponse.IsSuccessStatusCode) return null;
            string strResponse = await oResponse.Content.ReadAsStringAsync();

            ResponseFutures? oResult = JsonConvert.DeserializeObject<ResponseFutures?>(strResponse);
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


        public async Task<IFundingRate?> GetFundingRates(IFuturesSymbol oSymbol)
        {
            throw new NotImplementedException();    
        }
        public async Task<IFundingRate[]?> GetFundingRates(IFuturesSymbol[] aSymbols)
        {
            throw new NotImplementedException();

        }

    }
}
