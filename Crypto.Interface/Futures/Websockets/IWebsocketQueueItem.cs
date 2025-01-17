using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures.Websockets
{
    public enum WebsocketQueueType
    {
        Order,
        Balance,
        Poisition
    }


    public interface IWebsocketQueueItem
    {
        public WebsocketQueueType QueueType { get; }
    }
}
