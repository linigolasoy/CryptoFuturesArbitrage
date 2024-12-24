using Crypto.Exchange.Bingx.Websocket.Private;
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

    internal enum eChannelTypes
    {
        bookTicker
    }

    internal enum eEventTypes
    {
        SNAPSHOT,
        ORDER_TRADE_UPDATE
    }

    internal class WsCommonMessage
    {
        [JsonProperty("code")]
        public int Code { get; set; } = 0;
        [JsonProperty("id")]
        public string? Id { get; set; } = null;
        [JsonProperty("dataType")]
        public string DataType { get; set; } = string.Empty;
        [JsonProperty("msg")]
        public string? Message { get; set; } = null;
        [JsonProperty("data")]
        public JToken? Data { get; set; } = null;



    }


    internal class WsCommonPrivateMessage
    {
        [JsonProperty("e")]
        public string EventType { get; set; } = string.Empty;
        [JsonProperty("E")]
        public long EventTime { get; set; } = 0;

    }

    internal class WsMessageFactory
    {
        public static IWebsocketMessage? Parse(JToken oToken, IFuturesSymbol[] aAllSymbols)
        {
            if (!(oToken is JObject)) return null;
            JObject oObject = (JObject)oToken;  
            WsCommonMessage? oCommonMessage = oObject.ToObject<WsCommonMessage>();
            if (oCommonMessage == null) return null;
            if (string.IsNullOrEmpty(oCommonMessage.DataType)) return null;
            string[] aType = oCommonMessage.DataType.Split('@');
            string strType = aType[aType.Length - 1];
            string? strSymbol = (aType.Length > 1 ? aType[0] : null);   
            if( strType == eChannelTypes.bookTicker.ToString() )
            {
                return TickerMessage.Create(strSymbol, oCommonMessage.Data);   
            }
            return null;
        }

        public static IWebsocketMessage? ParsePrivate(JToken oToken, IFuturesSymbol[] aAllSymbols)
        {
            if (!(oToken is JObject)) return null;
            JObject oObject = (JObject)oToken;

            WsCommonPrivateMessage? oCommon = oObject.ToObject<WsCommonPrivateMessage>();   
            if (oCommon == null) return null;
            if( string.IsNullOrEmpty(oCommon.EventType)) return null;   
            if( oCommon.EventType == eEventTypes.SNAPSHOT.ToString() ) 
            {
                return null;
            }
            else if( oCommon.EventType == eEventTypes.ORDER_TRADE_UPDATE.ToString() )
            {
                return OrderMessage.Create(oObject, aAllSymbols);
            }
            return null;
        }
    }
}
