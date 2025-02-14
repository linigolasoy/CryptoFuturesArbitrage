using Crypto.Exchanges.All.IpfCryptoClients.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.IpfCryptoClients.Rest
{

    internal class StdResultResponse
    {
        [JsonProperty("code")]
        public long ErrorCode { get; set; }

        [JsonProperty("data")]
        public JToken? Data { get; set; }
    }

    internal class CryptoRestResult<T> : ICryptoRestResult<T>
    {
        private CryptoRestResult(ICryptoErrorCode oErrorCode)
        {
            Error = oErrorCode;
            Success = false;
        }

        public CryptoRestResult(T oResult)
        {
            Success = true;
            Data = oResult; 
        }

        public bool Success { get; private set; } = false;

        public ICryptoErrorCode? Error { get; private set; } = null;

        public T? Data { get; private set; }

        public static async Task<ICryptoRestResult<T>> CreateFromResponse(HttpResponseMessage oResponse, Func<JToken?, T> oParserAction)
        {
            ICryptoErrorCode? oHttpError = CryptoRestError.Create(oResponse);
            if( oHttpError != null) return new CryptoRestResult<T>(oHttpError); 

            string strResponse = await oResponse.Content.ReadAsStringAsync();
            StdResultResponse? oStdResponse = JsonConvert.DeserializeObject<StdResultResponse>(strResponse);


            if (oStdResponse == null) oHttpError = CryptoRestError.Create(-99999, "Could not parse error code");

            if( oHttpError != null ) return new CryptoRestResult<T>(oHttpError);

            T? oData = oParserAction(oStdResponse!.Data);

            throw new NotImplementedException();
        }

        public static async Task<ICryptoRestResult<T[]>> CreateFromResponseArray(HttpResponseMessage oResponse, Func<JToken, T> oParserAction)
        {
            ICryptoErrorCode? oHttpError = CryptoRestError.Create(oResponse);
            if (oHttpError != null) return new CryptoRestResult<T[]>(oHttpError);

            string strResponse = await oResponse.Content.ReadAsStringAsync();
            StdResultResponse? oStdResponse = JsonConvert.DeserializeObject<StdResultResponse>(strResponse);


            if (oStdResponse == null) oHttpError = CryptoRestError.Create(-99999, "Could not parse error code");

            if (oHttpError != null && oStdResponse!.Data == null) oHttpError = CryptoRestError.Create(-99998, "No Json received");

            if( oHttpError != null && !(oStdResponse!.Data is JArray) ) oHttpError = CryptoRestError.Create(-99997, "Not a Json array");

            if (oHttpError != null) return new CryptoRestResult<T[]>(oHttpError);

            JArray oArray = (JArray)(oStdResponse!.Data!);

            List<T> aList = new List<T>();
            foreach( var oItem in oArray )
            {
                T oParsed = oParserAction(oItem);
                aList.Add(oParsed);
            }
            return new CryptoRestResult<T[]>(aList.ToArray());  
        }

    }
}
