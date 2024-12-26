using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Websocket
{
    internal class WsMessageFactory
    {

        private const string SUB_RESPONSE = "rs";
        private const string PUSH = "push";


        private const string TICKER = "ticker";


        public static IWebsocketMessage? Parse( JToken oToken )
        {
            if (!(oToken is JObject)) return null;
            JObject oObject = (JObject)oToken;  
            ChannelMessage? oChannelMessage = oObject.ToObject<ChannelMessage>();   
            if (oChannelMessage == null) return null;
            if( oChannelMessage.Channel == null) return null;
            string[] aChannelSplit = oChannelMessage.Channel.Split('.');
            if( aChannelSplit.Length < 2 ) return null; 
            switch(aChannelSplit[0])
            {
                case SUB_RESPONSE:
                    return null;
                case PUSH:
                    switch (aChannelSplit[1])
                    {
                        // case TICKER:
                        //     return TickerMessage.Create(oChannelMessage);
                        default:
                            return null;
                    }

                default:
                    return null;
            }


        }
    }
}
