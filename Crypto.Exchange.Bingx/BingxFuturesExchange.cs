using Crypto.Common;
using Crypto.Exchange.Bingx.Feeders;
using Crypto.Exchange.Bingx.Futures;
using Crypto.Exchange.Bingx.Responses;
using Crypto.Exchange.Bingx.Websocket;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;

namespace Crypto.Exchange.Bingx
{
    public partial class BingxFuturesExchange : ICryptoFuturesExchange
    {

        private static IRequestHelper m_oRequestHelper = CommonFactory.CreateRequestHelper(BingxCommon.GetHttpClient(), 500);

        private const string ENDPOINT_SYMBOLS           = "openApi/swap/v2/quote/contracts";
        private const string ENDPOINT_FUNDING_HISTORY   = "openApi/swap/v2/quote/fundingRate";
        private const string ENDPOINT_FUNDING           = "openApi/swap/v2/quote/premiumIndex";
        private const string ENDPOINT_BALANCE           = "openApi/swap/v3/user/balance";
        private const string ENDPOINT_API_WS            = "openApi/user/auth/userDataStream";

        public const int TASK_COUNT = 20;

        private IApiKey m_oApiKey;

        private static IFuturesSymbol[]? m_aSymbols = null;

        private IFuturesBarFeeder m_oBarFeeder;
        public BingxFuturesExchange( ICryptoSetup oSetup ) 
        { 
            Setup = oSetup;
            IApiKey? oKeyFound = oSetup.ApiKeys.FirstOrDefault(p => p.ExchangeType == this.ExchangeType);
            if (oKeyFound == null) throw new Exception("No api key found");
            m_oApiKey = oKeyFound;
            m_oBarFeeder = new FuturesBarFeeder(this);  
        }
        public ICryptoSetup Setup { get; }
        public ExchangeType ExchangeType { get => ExchangeType.BingxFutures; }

        public IFuturesBarFeeder BarFeeder { get => m_oBarFeeder; }
        public async Task<IFundingRateSnapShot?> GetFundingRates(IFuturesSymbol oSymbol)
        {
            var oParameters = new
            {
                symbol = oSymbol.Symbol
            };
            ResponseFutures? oResult = await DoPublicGet(ENDPOINT_FUNDING, oParameters);
            if (oResult == null || oResult.Code != 0 || !string.IsNullOrEmpty(oResult.Message)) return null;
            if (oResult.Data == null) return null;
            if (!(oResult.Data is JObject)) return null;
            JObject oObject = (JObject)oResult.Data;
            FundingParsed? oParsed = oObject.ToObject<FundingParsed>(); 
            if( oParsed == null ) return null;  
            return new FuturesFundingSnapshot(oSymbol, oParsed);    
        }

        /// <summary>
        /// Funding rate symbol list
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRateSnapShot[]?> GetFundingRates(IFuturesSymbol[] aSymbols)
        {
            ResponseFutures? oResult = await DoPublicGet(ENDPOINT_FUNDING, null);
            if (oResult == null || oResult.Code != 0 || !string.IsNullOrEmpty(oResult.Message)) return null;
            if (oResult.Data == null) return null;
            if (!(oResult.Data is JArray)) return null;
            JArray oArray = (JArray)oResult.Data;
            List<IFundingRateSnapShot> aResult = new List<IFundingRateSnapShot>();
            foreach( JToken oToken in oArray )
            {
                if (!(oToken is JObject)) continue;
                JObject oObject = (JObject)oToken;
                FundingParsed? oParsed = oObject.ToObject<FundingParsed>();
                if (oParsed == null) continue;
                IFuturesSymbol? oSymbol = aSymbols.FirstOrDefault(p=> p.Symbol == oParsed.Symbol);
                if (oSymbol == null) continue;
                aResult.Add( new FuturesFundingSnapshot(oSymbol, oParsed) );
            }

            return aResult.ToArray();
        }

        /// <summary>
        /// Funding rate history
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol oSymbol)
        {
            DateTime dFromActual = DateTime.Today.AddYears(-2);
            DateTime dToActual = DateTime.Now;
            List<IFundingRate> aResult = new List<IFundingRate>();
            
            while( true )
            {
                DateTimeOffset dFromOffset = new DateTimeOffset(dFromActual.ToUniversalTime());
                DateTimeOffset dToOffset = new DateTimeOffset(dToActual.ToUniversalTime());

                var oParameters = new
                {
                    symbol = oSymbol.Symbol,
                    limit = 1000,
                    startTime = dFromOffset.ToUnixTimeMilliseconds(),
                    endTime = dToOffset.ToUnixTimeMilliseconds()
                };

                ResponseFutures? oResult = await DoPublicGet(ENDPOINT_FUNDING_HISTORY, oParameters);
                if (oResult == null || oResult.Code != 0 || !string.IsNullOrEmpty(oResult.Message)) break;
                if (!(oResult.Data is JArray)) continue;
                JArray oData = (JArray) oResult.Data;
                List<IFundingRate> aPartial = new List<IFundingRate>();
                foreach( JToken oToken in oData )
                {
                    if (!(oToken is JObject)) continue;
                    JObject oObject = (JObject) oToken; 
                    FundingHistoryParsed? oParsed = oObject.ToObject<FundingHistoryParsed>();
                    if (oParsed == null) continue;
                    IFundingRate oFundingRate = new FuturesFunding(oSymbol, oParsed);
                    aPartial.Add(oFundingRate);
                }

                if (aPartial.Count <= 0) break;
                DateTime dMinimum = aPartial.Select(p=> p.DateTime).Min();
                dToActual = dMinimum.AddHours(-1);
                aResult.AddRange(aPartial);
                if (dMinimum.Date <= dFromActual.Date) break;
            }
            return aResult.ToArray();
        }


        /// <summary>
        /// Funding rate history multiple symbols
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol[] aSymbols)
        {
            ITaskManager<IFundingRate[]?> oTaskManager = CommonFactory.CreateTaskManager<IFundingRate[]?>(TASK_COUNT);
            List<IFundingRate> aResult = new List<IFundingRate>();

            foreach (IFuturesSymbol oSymbol in aSymbols)
            {
                await oTaskManager.Add(GetFundingRatesHistory(oSymbol));
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


        /// <summary>
        /// Raw symbols
        /// </summary>
        /// <returns></returns>
        public async Task<ISymbol[]?> GetRawSymbols()
        {
            return await GetSymbols();
        }

        /// <summary>
        /// Get symbols
        /// </summary>
        /// <returns></returns>
        public async Task<IFuturesSymbol[]?> GetSymbols()
        {
            if (m_aSymbols != null) return m_aSymbols;
            ResponseFutures? oResult = await DoPublicGet(ENDPOINT_SYMBOLS, null);

            if (oResult == null || oResult.Code != 0 || !string.IsNullOrEmpty(oResult.Message)) return null;
            if (oResult.Data == null) return null;
            if (!(oResult.Data is JArray)) return null;
            JArray aData = (JArray)oResult.Data;    
            List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();  
            foreach(JToken oToken  in aData)
            {
                if (!(oToken is JObject)) continue;
                JObject oJsonObject = (JObject)oToken;  
                FuturesSymbolParsed? oParsed = oJsonObject.ToObject<FuturesSymbolParsed>();
                if(oParsed == null) continue;
                if (oParsed.Status != 1) continue;
                IFuturesSymbol oSymbol = new FuturesSymbol(oParsed);
                aResult.Add(oSymbol);
            }
            m_aSymbols = aResult.ToArray(); 
            return m_aSymbols;
        }



        

        /// <summary>
        /// Get private websocket listen key
        /// </summary>
        /// <returns></returns>
        private async Task<string?> GetListenKey()
        {
            var oPayload = new { };

            string? strResponse = await SignRequest(ENDPOINT_API_WS, HttpMethod.Post, oPayload, null);
            if (strResponse == null) return null;
            JObject? oObject = JObject.Parse(strResponse);   
            if( oObject == null) return null;
            if (!oObject.ContainsKey("listenKey")) return null;
            string strKey = oObject["listenKey"]!.ToString();
            return strKey;

        }

        /// <summary>
        /// Creates a mew wensocket
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ICryptoWebsocket?> CreateWebsocket()
        {
            string? strListenKey = await GetListenKey();
            if (strListenKey == null) return null;
            return new BingxFuturesWebsocket(this,strListenKey);
        }
        /// <summary>
        /// Get request
        /// </summary>
        /// <param name="strEndPoint"></param>
        /// <param name="oParameters"></param>
        /// <returns></returns>
        internal static async Task<ResponseFutures?> DoPublicGet( string strEndPoint, object? oParameters = null )
        {
            long nTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            StringBuilder oBuildParams = new StringBuilder();
            oBuildParams.Append($"timestamp={nTimestamp}");
            if( oParameters != null )
            {
                foreach (var oProperty in oParameters.GetType().GetProperties())
                {
                    oBuildParams.Append($"&{oProperty.Name}={oProperty.GetValue(oParameters)}");
                }
            }
            string strUrl = $"{BingxCommon.URL_FUTURES_BASE}{strEndPoint}?{oBuildParams.ToString()}";

            string? strResult = await m_oRequestHelper.GetRequest(strUrl);
            if (strResult == null) return null;
            return JsonConvert.DeserializeObject<ResponseFutures>(strResult);   
        }


        
        /// <summary>
        /// Send signed request
        /// </summary>
        /// <param name="strApi"></param>
        /// <param name="oMethod"></param>
        /// <param name="oPayLoad"></param>
        /// <param name="oBody"></param>
        /// <returns></returns>
        private async Task<string?> SignRequest( string strApi, HttpMethod oMethod, object? oPayLoad, object? oBody)
        //private static async Task DoRequest(string api, HttpMethod oMethod, object payload)
        {
            long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string parameters = $"timestamp={timestamp}";

            if (oPayLoad != null)
            {
                foreach (var property in oPayLoad.GetType().GetProperties())
                {
                    parameters += $"&{property.Name}={property.GetValue(oPayLoad)}";
                }
            }

            string sign = CalculateHmacSha256(parameters, m_oApiKey.ApiSecret);
            string url = $"{BingxCommon.URL_FUTURES_BASE}{strApi}?{parameters}&signature={sign}";

            // Console.WriteLine("protocol: " + protocol);
            // Console.WriteLine("method: " + method);
            // Console.WriteLine("host: " + host);
            // Console.WriteLine("api: " + api);
            // Console.WriteLine("parameters: " + parameters);
            // Console.WriteLine("sign: " + sign);
            // Console.WriteLine(method + " " + url);

            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (HttpClient client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("X-BX-APIKEY", m_oApiKey.ApiKey);
                    HttpResponseMessage? oResponse = null;
                    if ( oMethod == HttpMethod.Get )
                    {
                        oResponse = await client.GetAsync(url);

                    }
                    else if ( oMethod == HttpMethod.Post )
                    {
                        oResponse = await client.PostAsync(url, null);
                    }
                    else if ( oMethod == HttpMethod.Delete )
                    {
                        oResponse = await client.DeleteAsync(url);
                    }
                    else if (oMethod == HttpMethod.Put)
                    {
                        oResponse = await client.PutAsync(url, null);
                    }
                    else
                    {
                        throw new NotSupportedException("Unsupported HTTP method: " + oMethod.Method);
                    }
                    oResponse.EnsureSuccessStatusCode();
                    string responseBody = await oResponse.Content.ReadAsStringAsync();
                    // Console.WriteLine("Response status code: " + response.StatusCode);
                    // Console.WriteLine("Response body: " + responseBody);
                    return responseBody;
                }
            }
        }

        static string CalculateHmacSha256(string input, string key)
        {
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }


        /// <summary>
        /// Get balances
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IFuturesBalance[]?> GetBalances()
        {
            string? strResponse = await SignRequest(ENDPOINT_BALANCE, HttpMethod.Get, null, null);
            if (strResponse == null) return null;
            ResponseFutures? oResponse = JsonConvert.DeserializeObject<ResponseFutures>(strResponse);
            if (oResponse == null || oResponse.Code != 0 || !string.IsNullOrEmpty(oResponse.Message)) return null;
            if (oResponse.Data == null) return null;
            IFuturesBalance[]? aResult = FuturesBalance.Create(oResponse.Data);
            if( aResult == null) return null;   
            return aResult.ToArray();
        }

    }
}
