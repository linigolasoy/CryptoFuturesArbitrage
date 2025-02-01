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

        public DateTime LastUpdate { get; private set; } = DateTime.Now;

        public IOrderbook[] GetData()
        {
            List<IOrderbook> aResult = new List<IOrderbook>();
            foreach( string strSymbol in m_aOrderbooks.Keys )
            {
                IOrderbook? oFound = GetData( strSymbol );  
                if ( oFound != null ) aResult.Add( oFound );  
            }
            return aResult.ToArray();
        }

        public IOrderbook? GetData(string strSymbol)
        {
            IOrderbook? oResult = null;
            if( m_aOrderbooks.TryGetValue(strSymbol, out oResult) )
            {
                return oResult;
            }
            return null;
        }

        /// <summary>
        /// Add or update orderbook
        /// </summary>
        /// <param name="oOrderbook"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Update(IOrderbook oOrderbook)
        {
            LastUpdate = oOrderbook.UpdateDate;
            m_aOrderbooks.AddOrUpdate(oOrderbook.Symbol.Symbol, p => oOrderbook, (s, p) => { p.Update(oOrderbook); return p; });
        }

        public IOrderbookPrice? GetBestPrice(string strSymbol, bool bAsk, decimal? nQuantity = null, decimal? nMoney = null)
        {
            throw new NotImplementedException();
        }

    }
    
}
