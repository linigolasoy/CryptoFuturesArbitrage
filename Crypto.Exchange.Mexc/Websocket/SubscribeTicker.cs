using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Websocket
{

    internal class SubscribeTickerParams
    {
        public SubscribeTickerParams(string strSymbol)
        {
            Symbol = strSymbol; 
        }
        [JsonProperty("symbol")]
        public string Symbol { get; }


    }
    internal class SubscribeTicker
    {
        private bool m_bUnsubscribe = false;

        public SubscribeTicker( string strSymbol, bool bUnsubscribe )
        {
            m_bUnsubscribe |= bUnsubscribe; 
            Params = new SubscribeTickerParams( strSymbol );
        }
         
        [JsonProperty("method")]
        public string Method { get => (m_bUnsubscribe ? "unsub.ticker" : "sub.ticker"); }

        [JsonProperty("param")]

        public SubscribeTickerParams Params { get; }
        /*
{
    "method":"sub.ticker",
    "param":{
        "symbol":"BTC_USDT"
    }
}         */
    }
}
