using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Websocket
{

    internal enum WebsocketMessageType
    {
        Ping,
        Ticker
    }

    internal interface IWebsocketMessage
    {
        public WebsocketMessageType MessageType { get; }    
    }
}
