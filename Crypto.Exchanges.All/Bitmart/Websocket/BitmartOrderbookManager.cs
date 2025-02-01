using BitMart.Net.Objects.Models;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitmart.Websocket
{
    internal class BitmartOrderbookManager : OrderbookHandler, IOrderbookManager
    {
        private IFuturesWebsocketPublic m_oWebsocket;

        public BitmartOrderbookManager(IFuturesWebsocketPublic oWebsocket)
        {
            m_oWebsocket = oWebsocket;
        }

        public void Put(DataEvent<BitMartFuturesFullOrderBookUpdate> oUpdate)
        {
            if (oUpdate == null || oUpdate.Data == null) return;
            IFuturesSymbol? oSymbol = m_oWebsocket.Exchange.SymbolManager.GetSymbol(oUpdate.Data.Symbol);
            if (oSymbol == null) return;
            IOrderbook oOrderbook = new BitmartOrderbook(oSymbol, oUpdate.Data);
            this.Update(oOrderbook);    
        }
    }
}
