using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Websockets
{
    public interface IOrderbookManager: IWebsocketManager<IOrderbook>
    {

        public IOrderbookPrice? GetBestAsk(string strSymbol, decimal nMoney);
        public IOrderbookPrice? GetBestBid(string strSymbol, decimal nMoney);
    }
}
