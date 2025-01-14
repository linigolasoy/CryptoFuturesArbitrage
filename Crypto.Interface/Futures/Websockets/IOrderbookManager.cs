using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures.Websockets
{
    public interface IOrderbookManager: IWebsocketManager<IOrderbook>, IOrderbookHandler
    {

    }
}
