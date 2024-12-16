using Crypto.Interface;
using Newtonsoft.Json.Linq;

namespace Crypto.Exchange.Mexc
{
    public class MexcSpotExchange : ICryptoSpotExchange
    {

        private const string ENDPOINT_EXCHANGE_INFO = "/api/v3/exchangeInfo";

        private enum eTags
        {
            symbols
        }
        public MexcSpotExchange(ICryptoSetup oSetup)
        {
            Setup = oSetup; 
        }
        public ICryptoSetup Setup { get; }

        public async Task<ISymbol[]?> GetSymbols()
        {
            HttpClient oClient = MexcCommon.GetHttpClient();    

            HttpResponseMessage oResponse = await oClient.GetAsync(MexcCommon.URL_SPOT_BASE + ENDPOINT_EXCHANGE_INFO);
            if (!oResponse.IsSuccessStatusCode) return null;

            string strResponse = await oResponse.Content.ReadAsStringAsync();  
            JObject oObject = JObject.Parse(strResponse);   
            if( !oObject.ContainsKey(eTags.symbols.ToString())) return null;    

            JArray oArray = (JArray)oObject[eTags.symbols.ToString()]!;

            List<ISymbol> aResult = new List<ISymbol>();
            foreach( JToken oToken in oArray ) 
            { 
                if( !(oToken is JObject)) continue;
                JObject oJsonSymbol = (JObject)oToken;  
                ISymbol? oSymbol = MexcSymbol.Create(oJsonSymbol);
                if (oSymbol == null) continue;
                aResult.Add(oSymbol);

            }

            return aResult.ToArray();
        }
    }
}
