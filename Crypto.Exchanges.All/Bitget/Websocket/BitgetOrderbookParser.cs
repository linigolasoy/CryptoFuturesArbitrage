using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget.Websocket
{


    internal class BitgetDataArray
    {
        [JsonProperty("asks")]
        public List<List<string>>? AskData { get; set; }
        [JsonProperty("bids")]
        public List<List<string>>? BidData { get; set; }
        // public string? Action { get; set; }
    }
    internal class BitgetDataArguments
    {
        [JsonProperty("instId")]
        public string? Symbol { get; set; }
    }

    internal class BitgetDataAction
    {
        [JsonProperty("action")]  
        public string? Action {  get; set; }

        [JsonProperty("arg")]
        public BitgetDataArguments? Arguments { get; set; }

        [JsonProperty("data")]
        public List<BitgetDataArray>? Data { get; set; }

        [JsonProperty("ts")]
        public string? Timestamp { get; set; }  
    }

    internal class BitgetOrderbookParser : IOrderbookParser
    {

        private const string KEY_EVENT  = "event";
        private const string KEY_ACTION = "action";
        private const string KEY_ARG    = "arg";
        private const string KEY_DATA   = "data";
        private const string KEY_SYMBOL = "instId";
        private const string KEY_ASKS   = "asks";
        private const string KEY_BIDS   = "bids";
        private const string KEY_TS     = "ts";

        private const string MSG_PONG = "pong";
        public static string GetSubscribeMessage(IFuturesSymbol[] aSymbols)
        {
            JObject oObject = new JObject();
            oObject["op"] = "subscribe";

            JArray oArray = new JArray();
            foreach (var oSymbol in aSymbols)
            {
                JObject oNew = new JObject();
                oNew["instType"] = "USDT-FUTURES";
                oNew["channel"] = "books15";
                oNew["instId"] = oSymbol.Symbol;
                oArray.Add(oNew);
            }
            oObject["args"] = oArray;

            return oObject.ToString();
        }
        public IOrderbook? Parse(string strMessage, IFuturesSymbolManager oSymbolManager)
        {
            try
            {
                if (strMessage.Equals(MSG_PONG) ) return null;
                BitgetDataAction? oAction = JsonConvert.DeserializeObject<BitgetDataAction>(strMessage);
                if (oAction == null || oAction.Action == null) return null;
                if( oAction.Arguments == null || oAction.Arguments.Symbol == null ) return null;
                /*
                JObject? oObject = JObject.Parse(strMessage);
                if (oObject == null) return null;
                if (oObject.ContainsKey(KEY_EVENT)) return null;
                if (!oObject.ContainsKey(KEY_ACTION)) return null;

                JToken? oArgs = oObject[KEY_ARG];
                if (oArgs == null) return null; JArray oArray = new JArray();
                JToken? oData = oObject[KEY_DATA];
                if (oData == null) return null;

                JToken? oTokenSymbol = oArgs[KEY_SYMBOL];
                if (oTokenSymbol == null) return null;
                if (!(oData is JArray)) return null;
                JArray oDataArray = (JArray)oData;
                if (oDataArray.Count > 1 || oDataArray.Count <= 0) return null;
                JObject oDataObject = (JObject)oData[0]!;
                JArray oAsks = (JArray)oDataObject[KEY_ASKS]!;
                JArray oBids = (JArray)oDataObject[KEY_BIDS]!;

                string strTs = oDataObject[KEY_TS]!.ToString();
                */
                IFuturesSymbol? oSymbol = oSymbolManager.GetSymbol(oAction.Arguments.Symbol);
                if (oSymbol == null) return null;
                long nTs = long.Parse(oAction.Timestamp!);
                DateTime dTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(nTs).DateTime;
                return new BitgetOrderbook(oSymbol, dTimestamp, oAction);
            }
            catch(Exception e)
            {
                return null;
            }
        }

        public string PingMessage { get => "ping"; }
    }
}
