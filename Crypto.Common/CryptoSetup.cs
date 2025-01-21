using Crypto.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Common
{

    internal class ApiKeyParsed
    {
        [JsonProperty("ExchangeType")]
        public string ExchangeType { get; set; } = string.Empty;
        [JsonProperty("ApiKey")]
        public string ApiKey { get; set; } = string.Empty;
        [JsonProperty("ApiSecret")]
        public string ApiSecret { get; set; } = string.Empty;
    }

    internal class SetupParsed
    {
        [JsonProperty("ApiKeys")]
        public List<ApiKeyParsed>? ApiKeys { get; set; } = null;
    }


    internal class CryptoApiKey : IApiKey
    {
        public CryptoApiKey( ExchangeType eType, string strKey, string strSecret )
        {
            ExchangeType = eType;
            ApiKey = strKey;
            ApiSecret = strSecret;
        }
        public string ApiKey { get; }  
        public string ApiSecret { get; }
        public ExchangeType ExchangeType { get; }  
    }
    internal class CryptoSetup : ICryptoSetup
    {

        internal CryptoSetup(IApiKey[] aApiKeys )
        {
            ApiKeys = aApiKeys; 
        }
        public IApiKey[] ApiKeys { get; }


        public ExchangeType[] ExchangeTypes { get => new ExchangeType[] { ExchangeType.CoinExFutures, ExchangeType.BingxFutures, ExchangeType.BitgetFutures}; }
        public decimal Amount { get => 30; }
        public int Leverage { get => 10; }
        public decimal PercentMinimum { get => 0.15M; }
        public string LogPath { get => "D:/Data/CryptoFutures/Log"; }

        public static ICryptoSetup? LoadFromFile( string strFile )
        {
            string? strContent = File.ReadAllText(strFile);
            SetupParsed? oSetupParsed = JsonConvert.DeserializeObject<SetupParsed>(strContent);
            if (oSetupParsed == null) return null;
            if( oSetupParsed.ApiKeys == null) return null;
            List<IApiKey> aFound = new List<IApiKey>();
            foreach( var oKey in oSetupParsed.ApiKeys )
            {
                ExchangeType eType = ExchangeType.BingxFutures;
                if( Enum.TryParse<ExchangeType>(oKey.ExchangeType, out eType ) )
                {
                    aFound.Add(new CryptoApiKey(eType, oKey.ApiKey, oKey.ApiSecret));
                }
            }
            if( aFound.Count <= 0 ) return null;    
            return new CryptoSetup(aFound.ToArray());
        }
    }
}
