using Bitget.Net.Objects.Models.V2;
using Crypto.Exchanges.All.Bingx;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget.Websocket
{
    internal class BitgetOrderbookManager : OrderbookHandler, IOrderbookManager
    {


        private IFuturesSymbolManager m_oSymbolManager;
        public BitgetOrderbookManager(IFuturesWebsocketPublic oWebsocket) 
        {
            m_oSymbolManager = oWebsocket.Exchange.SymbolManager;
        }



        public void Put(string strSymbol, BitgetOrderBookUpdate oUpdate)
        {
            IFuturesSymbol? oFound = m_oSymbolManager.GetSymbol(strSymbol);
            if (oFound == null) return;
            IOrderbook oOrderbook = new BitgetOrderbook(oFound,oUpdate);
            this.Update(oOrderbook);
            return;
        }
    }
}
