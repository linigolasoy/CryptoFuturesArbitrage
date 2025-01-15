using Bitget.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget.Websocket
{
    internal class BitgetOrderManager : IWebsocketManager<IFuturesOrder>
    {
        private ConcurrentDictionary<long, IFuturesOrder> m_aOrders = new ConcurrentDictionary<long, IFuturesOrder> (); 
        private BitgetWebsocketPrivate m_oWebsocket;
        public int ReceiveCount { get; private set; } = 0;
        public int Count { get=> m_aOrders.Count; } 
        public BitgetOrderManager(BitgetWebsocketPrivate oWebsocket)
        {
            m_oWebsocket = oWebsocket;
        }

        public IFuturesOrder[] GetData()
        {
            List<IFuturesOrder> aResult = new List<IFuturesOrder> ();

            foreach( long nId in m_aOrders.Keys )
            {
                IFuturesOrder? oFound = null;
                if(m_aOrders.TryGetValue( nId, out oFound ) ) aResult.Add( oFound );
            }
            return aResult.ToArray();
        }

        public IFuturesOrder? GetData(string strSymbol)
        {
            IFuturesOrder[] aAll = GetData();
            return aAll.FirstOrDefault(p=> p.Symbol.Symbol ==  strSymbol);  
        }

        /// <summary>
        /// Put data
        /// </summary>
        /// <param name="aOrders"></param>
        public void Put(IEnumerable<BitgetFuturesOrderUpdate> aOrders)
        {
            ReceiveCount++;
            foreach( var oParsed in aOrders )
            {
                IFuturesSymbol? oSymbol = m_oWebsocket.FuturesSymbols.FirstOrDefault(p=> p.Symbol == oParsed.Symbol);
                if (oSymbol == null) continue;
                IFuturesOrder oNew = new BitgetOrder(oSymbol, oParsed);

                m_aOrders.AddOrUpdate(oNew.Id, p => oNew, (s, p) => { p.Update(oNew); return p; });
            }
        }
    }
}
