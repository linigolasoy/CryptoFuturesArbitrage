using Crypto.Exchange.Mexc.Spot;
using Crypto.Interface;
using Crypto.Interface.Websockets;
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

        public async Task<ISymbol[]?> GetRawSymbols()
        {
            return await GetSymbols();
        }

        public async Task<ISpotSymbol[]?> GetSymbols()
        {
            HttpClient oClient = MexcCommon.GetHttpClient();    

            HttpResponseMessage oResponse = await oClient.GetAsync(MexcCommon.URL_SPOT_BASE + ENDPOINT_EXCHANGE_INFO);
            if (!oResponse.IsSuccessStatusCode) return null;

            string strResponse = await oResponse.Content.ReadAsStringAsync();  
            JObject oObject = JObject.Parse(strResponse);   
            if( !oObject.ContainsKey(eTags.symbols.ToString())) return null;    

            JArray oArray = (JArray)oObject[eTags.symbols.ToString()]!;

            List<ISpotSymbol> aResult = new List<ISpotSymbol>();
            foreach( JToken oToken in oArray ) 
            { 
                if( !(oToken is JObject)) continue;
                JObject oJsonSymbol = (JObject)oToken;
                ISpotSymbol? oSymbol = SpotSymbol.Create(oJsonSymbol);
                if (oSymbol == null) continue;
                aResult.Add(oSymbol);

            }

            return aResult.ToArray();
        }

        public async Task<ICryptoWebsocket?> CreateWebsocket()
        {
            throw new NotImplementedException();    
        }
    }
}
