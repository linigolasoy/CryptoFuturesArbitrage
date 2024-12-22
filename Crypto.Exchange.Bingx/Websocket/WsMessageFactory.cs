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
    }
}
