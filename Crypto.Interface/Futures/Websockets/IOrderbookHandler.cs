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

        public IOrderbookPrice? GetBestPrice(string strSymbol, bool bAsk, decimal? nQuantity = null, decimal? nMoney = null);


        public void Update(IOrderbook oOrderbook);
    }
}
