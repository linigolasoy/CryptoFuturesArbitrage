﻿using Crypto.Interface;
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

    internal class SetupMoneyParsed
    {
        [JsonProperty("Leverage")]
        public int Leverage { get; set; } = 1;
        [JsonProperty("Amount")]
        public decimal Amount { get; set; } = 0;
        [JsonProperty("Threshold")]
        public decimal Threshold { get; set; } = 1;
        [JsonProperty("CloseOnProfit")]
        public decimal CloseOnProfit { get; set; } = 0.5M;
    }

    internal class SetupParsed
    {
        [JsonProperty("ApiKeys")]
        public List<ApiKeyParsed>? ApiKeys { get; set; } = null;
        [JsonProperty("Money")]
        public SetupMoneyParsed? Money { get; set; } = null;

        [JsonProperty("LogPath")]
        public string LogPath { get; set; } = string.Empty;
        [JsonProperty("HistoryPath")]
        public string HistoryPath { get; set; } = String.Empty;

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

        internal CryptoSetup(IApiKey[] aApiKeys, SetupParsed oParsed )
        {
            ApiKeys = aApiKeys; 
            if( oParsed.Money != null )
            {
                Leverage = oParsed.Money.Leverage;
                Amount = oParsed.Money.Amount;
                ThresHold = oParsed.Money.Threshold;
                CloseOnProfit = oParsed.Money.CloseOnProfit;
            }
            if( oParsed.LogPath != null ) LogPath = oParsed.LogPath;
            if( oParsed.HistoryPath != null ) HistoryPath = oParsed.HistoryPath;
        }
        public IApiKey[] ApiKeys { get; }


        public ExchangeType[] ExchangeTypes 
        { 
            get => new ExchangeType[] 
                { 
                    ExchangeType.CoinExFutures, 
                    ExchangeType.BingxFutures, 
                    ExchangeType.BitgetFutures, 
                    ExchangeType.BitmartFutures
                    // ,ExchangeType.BitUnixFutures
                }; 
        }
        public decimal Amount { get; private set; } = 0;
        public int Leverage { get; private set; } = 0;
        public decimal ThresHold { get; private set; } = 1;
        public decimal CloseOnProfit { get; private set; } = 0.5M;
        public string LogPath { get; private set; } = string.Empty;
        public string HistoryPath { get; private set; } = string.Empty;

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

            return new CryptoSetup(aFound.ToArray(), oSetupParsed);
        }
    }
}
