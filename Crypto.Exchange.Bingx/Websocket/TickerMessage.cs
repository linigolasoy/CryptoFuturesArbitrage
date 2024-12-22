using Crypto.Interface.Futures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx.Websocket
{
    internal class TickerMessage : IWebsocketMessage
    {

        public WebsocketMessageType MessageType { get => WebsocketMessageType.Ticker; }

        [JsonProperty("e")]
        public string EventType { get; set; } = string.Empty;

        [JsonProperty("u")]
        public long UpdateId { get; set; } = 0;
        [JsonProperty("E")]
        public long EventTime { get; set; } = 0;
        [JsonProperty("T")]
        public long TransactionTime { get; set; } = 0;
        [JsonProperty("s")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("b")]
        public string BidPrice { get; set; } = string.Empty;
        [JsonProperty("B")]
        public string BidVolume { get; set; } = string.Empty;
        [JsonProperty("a")]
        public string AskPrice { get; set; } = string.Empty;
        [JsonProperty("A")]
        public string AskVolume { get; set; } = string.Empty;

        public static IWebsocketMessage? Create( string? strSymbol, JToken? oData )
        {
            if( strSymbol == null ) return null;
            if(!(oData is JObject)) return null;    
            JObject oObject = (JObject)oData;   
            TickerMessage? oJson = oObject.ToObject<TickerMessage>();
            return oJson;
        }
    }
}
