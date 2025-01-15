using CoinEx.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using CoinEx.Net.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using Crypto.Interface.Futures.Market;

namespace Crypto.Exchanges.All.CoinEx.Websocket
{
    internal class CoinexOrderManager : IWebsocketManager<IFuturesOrder>
    {

        private CoinexWebsocketPrivate m_oWebsocket;

        private ConcurrentDictionary<long, IFuturesOrder> m_aOrders = new ConcurrentDictionary<long, IFuturesOrder>();
        public int ReceiveCount { get; private set; } = 0;
        public int Count { get => m_aOrders.Count; }
        public CoinexOrderManager(CoinexWebsocketPrivate oWs) 
        { 
            m_oWebsocket = oWs; 
        }
        public IFuturesOrder[] GetData()
        {
            List<IFuturesOrder> aResult = new List<IFuturesOrder> ();
            foreach( long nId in m_aOrders.Keys )
            {
                IFuturesOrder? oFound = null;
                if( m_aOrders.TryGetValue( nId, out oFound ) ) aResult.Add( oFound );   
            }
            return aResult.ToArray();
        }

        public IFuturesOrder? GetData(string strSymbol)
        {
            IFuturesOrder[] aAll = GetData();
            return aAll.FirstOrDefault(p => p.Symbol.Symbol == strSymbol);
        }


        public void Put(CoinExFuturesOrderUpdate oOrder)
        {
            ReceiveCount++; 
            IFuturesSymbol? oSymbol = m_oWebsocket.FuturesSymbols.FirstOrDefault(p=> p.Symbol == oOrder.Order.Symbol);
            if (oSymbol == null) return;
            bool bBuy = false;
            if( oOrder.Order.Side == OrderSide.Buy ) bBuy = true;
            OrderUpdateType eType = oOrder.Event;

            IFuturesOrder oNew = new CoinexOrder(oSymbol, bBuy, true, oOrder.Order, oOrder.Event );
            m_aOrders.AddOrUpdate(oNew.Id, p => oNew, (s, p) => { p.Update(oNew); return p; });
            return;
        }
    }
}
