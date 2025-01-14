using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common
{
    
    internal class OrderbookHandler: IOrderbookHandler
    {
        private ConcurrentDictionary<string, IOrderbook> m_aOrderbooks = new ConcurrentDictionary<string, IOrderbook> ();
        private int m_nUpdates = 0;
        public OrderbookHandler() 
        { 
        }

        public int Count { get => m_aOrderbooks.Keys.Count; }

        public int ReceiveCount { get => m_nUpdates; }

        public IOrderbook[] GetData()
        {
            throw new NotImplementedException();
        }

        public IOrderbook? GetData(string strSymbol)
        {
            throw new NotImplementedException();
        }

        public void Update(IOrderbook oOrderbook)
        {
            throw new NotImplementedException();
        }

        public IOrderbookPrice? GetBestAsk(string strSymbol, decimal nMoney)
        {
            throw new NotImplementedException ();   
        }
        public IOrderbookPrice? GetBestBid(string strSymbol, decimal nMoney)
        {
            throw new NotImplementedException();
        }

    }
    
}
