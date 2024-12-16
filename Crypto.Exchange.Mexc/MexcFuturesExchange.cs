using Crypto.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc
{
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

        public async Task<IFuturesSymbol[]?> GetSymbols()
        {
            HttpClient oClient = MexcCommon.GetHttpClient();

            HttpResponseMessage oResponse = await oClient.GetAsync(MexcCommon.URL_FUTURES_BASE + ENDPOINT_CONTRACTS);
            if (!oResponse.IsSuccessStatusCode) return null;

            string strResponse = await oResponse.Content.ReadAsStringAsync();
            JObject oObject = JObject.Parse(strResponse);
            if (!oObject.ContainsKey(eTags.data.ToString())) return null;
            JArray oArray = (JArray)oObject[eTags.data.ToString()]!;


            List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
            foreach (JToken oToken in oArray)
            {
                if (!(oToken is JObject)) continue;
                JObject oJsonSymbol = (JObject)oToken;
                IFuturesSymbol? oSymbol = MexcFuturesSymbol.CreateFutures(oJsonSymbol); 
                if (oSymbol == null) continue;
                aResult.Add(oSymbol);

            }

            return aResult.ToArray();
        }
    }
}
