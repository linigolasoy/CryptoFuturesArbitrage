using Crypto.Common;
using Crypto.Exchange.Bingx.Futures;
using Crypto.Exchange.Bingx.Responses;
using Crypto.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Crypto.Exchange.Bingx
{
    public class BingxFuturesExchange : ICryptoFuturesExchange
    {

        private static IRequestHelper m_oRequestHelper = CommonFactory.CreateRequestHelper(BingxCommon.GetHttpClient(), 500);

        private const string ENDPOINT_SYMBOLS           = "openApi/swap/v2/quote/contracts";
        private const string ENDPOINT_FUNDING_HISTORY   = "openApi/swap/v2/quote/fundingRate";

        public BingxFuturesExchange( ICryptoSetup oSetup ) 
        { 
            Setup = oSetup;
        }
        public ICryptoSetup Setup { get; }

        public async Task<IFundingRateSnapShot?> GetFundingRates(IFuturesSymbol oSymbol)
        {
            throw new NotImplementedException();
        }

        public async Task<IFundingRateSnapShot[]?> GetFundingRates(IFuturesSymbol[] aSymbols)
        {
            throw new NotImplementedException();
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
            int nTaskCount = 20;

            List<Task<IFundingRate[]?>> aTasks = new List<Task<IFundingRate[]?>>();
            List<IFundingRate> aResult = new List<IFundingRate>();

            foreach (IFuturesSymbol oSymbol in aSymbols)
            {
                // await oLimiter.Wait();
                if (aTasks.Count >= nTaskCount)
                {
                    await Task.WhenAll(aTasks);
                    foreach (var oTask in aTasks)
                    {
                        if (oTask.Result != null) aResult.AddRange(oTask.Result);
                    }
                    aTasks.Clear();
                }
                aTasks.Add(GetFundingRatesHistory(oSymbol));
            }
            if (aTasks.Count >= 0)
            {
                await Task.WhenAll(aTasks);
                foreach (var oTask in aTasks)
                {
                    if (oTask.Result != null) aResult.AddRange(oTask.Result);
                }
            }


            return aResult.ToArray();
        }

        public async Task<IFuturesSymbol[]?> GetSymbols()
        {
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
            return aResult.ToArray();
        }


        /// <summary>
        /// Get request
        /// </summary>
        /// <param name="strEndPoint"></param>
        /// <param name="oParameters"></param>
        /// <returns></returns>
        private static async Task<ResponseFutures?> DoPublicGet( string strEndPoint, object? oParameters = null )
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
        /*
        private static async Task DoRequest(string api, HttpMethod oMethod, object payload)
        {
            long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string parameters = $"timestamp={timestamp}";

            if (payload != null)
            {
                foreach (var property in payload.GetType().GetProperties())
                {
                    parameters += $"&{property.Name}={property.GetValue(payload)}";
                }
            }

            string sign = CalculateHmacSha256(parameters, API_SECRET);
            string url = $"{BingxCommon.URL_FUTURES_BASE}{api}?{parameters}&signature={sign}";

            Console.WriteLine("protocol: " + protocol);
            Console.WriteLine("method: " + method);
            Console.WriteLine("host: " + host);
            Console.WriteLine("api: " + api);
            Console.WriteLine("parameters: " + parameters);
            Console.WriteLine("sign: " + sign);
            Console.WriteLine(method + " " + url);

            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (HttpClient client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("X-BX-APIKEY", API_KEY);
                    HttpResponseMessage response;
                    if (method.ToUpper() == "GET")
                    {
                        response = await client.GetAsync(url);
                    }
                    else if (method.ToUpper() == "POST")
                    {
                        response = await client.PostAsync(url, null);
                    }
                    else if (method.ToUpper() == "DELETE")
                    {
                        response = await client.DeleteAsync(url);
                    }
                    else if (method.ToUpper() == "PUT")
                    {
                        response = await client.PutAsync(url, null);
                    }
                    else
                    {
                        throw new NotSupportedException("Unsupported HTTP method: " + method);
                    }
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Response status code: " + response.StatusCode);
                    Console.WriteLine("Response body: " + responseBody);
                }
            }
        }
        */

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
    }
}
