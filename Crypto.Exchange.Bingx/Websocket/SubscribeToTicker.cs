using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx.Websocket
{
    internal class SubscribeToTicker
    {
        // string strSend = "{\"id\":\"Tururu\",\"reqType\": \"sub\",\"dataType\":\"BTC-USDT@bookTicker\"}";
        private bool m_bUnsubscribe = false;
        private string m_strTicker;
        public SubscribeToTicker(string strId, string strTicker, bool bUnsubscribe )
        {
            Id = strId;
            m_bUnsubscribe = bUnsubscribe;
            m_strTicker = strTicker;
        }

        [JsonProperty("id")]
        public string Id { get; }

        [JsonProperty("reqType")]
        public string RequestType { get => (m_bUnsubscribe ? "unsub": "sub"); }

        [JsonProperty("dataType")]
        public string DataType { get => string.Format("{0}@bookTicker", m_strTicker); }
    }
}
