using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures.Websockets

{
    public interface IOrderbookHandler
    {

        public IOrderbookPrice? GetBestAsk(string strSymbol, decimal nMoney);
        public IOrderbookPrice? GetBestBid(string strSymbol, decimal nMoney);


        public void Update(IOrderbook oOrderbook);
    }
}
