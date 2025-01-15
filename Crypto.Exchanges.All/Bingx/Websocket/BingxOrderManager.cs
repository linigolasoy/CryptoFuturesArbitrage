﻿using BingX.Net.Objects.Models;
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

namespace Crypto.Exchanges.All.Bingx.Websocket
{
    internal class BingxOrderManager : IWebsocketManager<IFuturesOrder>
    {


        private ConcurrentDictionary<long, IFuturesOrder> m_aOrders = new ConcurrentDictionary<long, IFuturesOrder> (); 
        private IFuturesWebsocketPrivate m_oWebsocket;

        public int ReceiveCount { get; private set; } = 0;
        public int Count { get => m_aOrders.Count; }
        public BingxOrderManager(IFuturesWebsocketPrivate oWs) 
        { 
            m_oWebsocket = oWs;
        }



        public IFuturesSymbol[] FuturesSymbols { get; internal set; } = Array.Empty<IFuturesSymbol>();

        public IFuturesOrder[] GetData()
        {
            List<IFuturesOrder> aResult = new List<IFuturesOrder>();
            foreach( long nId in m_aOrders.Keys )
            {
                IFuturesOrder? oFound = null;
                if( m_aOrders.TryGetValue( nId, out oFound ) )
                {
                    aResult.Add( oFound );  
                }
            }
            return aResult.ToArray();
        }

        public IFuturesOrder? GetData(string strSymbol)
        {
            IFuturesOrder? oResult = GetData().FirstOrDefault(p=> p.Symbol.Symbol == strSymbol);

            return oResult;
        }

        public void Put(BingXFuturesOrderUpdate oUpdate)
        {
            IFuturesSymbol? oFound = FuturesSymbols.FirstOrDefault(p=> p.Symbol == oUpdate.Symbol);
            if (oFound == null) return;
            ReceiveCount++;
            IFuturesOrder oNewOrder = new BingxOrder(oFound, oUpdate);

            m_aOrders.AddOrUpdate( oUpdate.OrderId, p=> oNewOrder, (s,p)=> { p.Update(oNewOrder); return p; } ); 

        }
    }
}
