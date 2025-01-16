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
        private IFuturesSymbol[] m_aSymbols;

        public CoinexOrderbookManager(IFuturesSymbol[] aSymbols ) 
        { 
            m_aSymbols = aSymbols;
        }


        public void Put(CoinExOrderBook oParsed)
        {
            IFuturesSymbol? oFound = m_aSymbols.FirstOrDefault(p=> p.Symbol == oParsed.Symbol);
            if (oFound == null) return;
            CoinexOrderbook oBook = new CoinexOrderbook(oFound, oParsed);
            this.Update(oBook);

            return;
        }
    }
}
