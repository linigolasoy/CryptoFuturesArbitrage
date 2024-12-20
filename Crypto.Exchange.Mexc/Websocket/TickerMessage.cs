using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Websocket
{
    internal class TickerMessage : IWebsocketMessage
    {
        public WebsocketMessageType MessageType { get => WebsocketMessageType.Ticker; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("timestamp")]
        public long UnixTime { get; set; } = 0;

        [JsonProperty("ask1")]
        public double Ask { get; set; } = 0;
        [JsonProperty("bid1")]
        public double Bid { get; set; } = 0;

        [JsonProperty("fundingRate")]
        public double FundingRate { get; set; } = 0;

        [JsonProperty("lastPrice")]
        public double LastPrice { get; set; } = 0;

        /*
        "contractId":1,
        "fairPrice":6867.4,
        "high24Price":7223.5,
        "indexPrice":6861.6,
        "lower24Price":6756,
        "maxBidPrice":7073.42,
        "minAskPrice":6661.37,
        "riseFallRate":-0.0424,
        "riseFallValue":-304.5,
        "holdVol":2284742,
        "volume24":164586129 
        */


        public static IWebsocketMessage? Create(ChannelMessage oMessage )
        {
            if(oMessage.Data == null || !(oMessage.Data is JObject)) return null;
            JObject oObject = (JObject)oMessage.Data;   
            TickerMessage? oResult = oObject.ToObject<TickerMessage>(); 
            return oResult;
        }
    }
}
