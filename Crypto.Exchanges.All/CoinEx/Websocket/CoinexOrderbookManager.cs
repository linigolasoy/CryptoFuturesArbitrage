using CoinEx.Net.Objects.Models.V2;
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

namespace Crypto.Exchanges.All.CoinEx.Websocket
{
    internal class CoinexOrderbookManager : OrderbookHandler, IOrderbookManager
    {
        private IFuturesSymbolManager m_oSymbolManager;

        public CoinexOrderbookManager(IFuturesWebsocketPublic oWebsocket ) 
        {
            m_oSymbolManager = oWebsocket.Exchange.SymbolManager;
        }


        public void Put(CoinExOrderBook oParsed)
        {
            IFuturesSymbol? oFound = m_oSymbolManager.GetSymbol(oParsed.Symbol);
            if (oFound == null) return;
            CoinexOrderbook oBook = new CoinexOrderbook(oFound, oParsed);
            this.Update(oBook);

            return;
        }
    }
}
