using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx.Websocket
{
    internal enum WebsocketMessageType
    {
        Ticker,
        PrivateSnapshot,
        PrivateOrder
    }

    internal interface IWebsocketMessage
    {
        public WebsocketMessageType MessageType { get; }
    }
}
