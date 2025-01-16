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


        private IFuturesSymbol[] m_aSymbols;
        public BitgetOrderbookManager(IFuturesSymbol[] aSymbols) 
        { 
            m_aSymbols = aSymbols;
        }



        public void Put(string strSymbol, BitgetOrderBookUpdate oUpdate)
        {
            IFuturesSymbol? oFound = m_aSymbols.FirstOrDefault(p => p.Symbol == strSymbol);
            if (oFound == null) return;
            IOrderbook oOrderbook = new BitgetOrderbook(oFound,oUpdate);
            this.Update(oOrderbook);
            return;
        }
    }
}
