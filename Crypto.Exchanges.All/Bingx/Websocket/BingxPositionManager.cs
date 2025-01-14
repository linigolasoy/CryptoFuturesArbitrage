using BingX.Net.Objects.Models;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx.Websocket
{
    internal class BingxPositionManager : IWebsocketManager<IFuturesPosition>
    {

        private IFuturesWebsocketPrivate m_oWebsocket;

        private ConcurrentDictionary<string, IFuturesPosition> m_aPositions = new ConcurrentDictionary<string, IFuturesPosition>();
        public int ReceiveCount { get; private set; } = 0;
        public BingxPositionManager( IFuturesWebsocketPrivate oWs) 
        { 
            m_oWebsocket = oWs; 
        }

        public IFuturesSymbol[] FuturesSymbols { get; internal set; } = Array.Empty<IFuturesSymbol>();

        public IFuturesPosition[] GetData()
        {
            List<IFuturesPosition> aResults = new List<IFuturesPosition>(); 
            foreach( string strKey  in m_aPositions.Keys )
            {
                IFuturesPosition? oData = GetData( strKey );    
                if( oData != null ) aResults.Add( oData );
            }
            return aResults.ToArray();
        }

        public IFuturesPosition? GetData(string strSymbol)
        {
            IFuturesPosition? oFound = null;
            if( m_aPositions.TryGetValue(strSymbol, out oFound) ) { return oFound; }
            return null;    
        }

        public void Put( IEnumerable<BingXFuturesPositionChange> aUpdated )
        {
            List<IFuturesPosition> aPositions = new List<IFuturesPosition>();
            ReceiveCount++;
            foreach( var oPos in aUpdated)
            {
                IFuturesSymbol? oSymbol = FuturesSymbols.FirstOrDefault(p=> p.Symbol == oPos.Symbol );
                if (oSymbol == null) continue;
                IFuturesPosition oNew = new BingxPositionLocal(oSymbol, oPos);
                aPositions.Add(oNew);
            }


            // Remove uniexistent
            string[] aDeleteKeys = m_aPositions.Keys.Where(p => !aPositions.Any(q => q.Symbol.Symbol == p)).ToArray();
            foreach( string strDelete in aDeleteKeys )
            {
                IFuturesPosition? oRemoved = null;
                m_aPositions.TryRemove(strDelete, out oRemoved);
            }

            foreach( var oPos in aPositions )
            {
                m_aPositions.AddOrUpdate(oPos.Symbol.Symbol, p => oPos, (s, p) => { p.Update(oPos); return p; });
            }
        }
    }
}
