using CoinEx.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx.Websocket
{
    internal class CoinexPoisitionManager : IWebsocketManager<IFuturesPosition>
    {

        private ConcurrentDictionary<string, IFuturesPosition> m_aPositions = new ConcurrentDictionary<string, IFuturesPosition>(); 
        private CoinexWebsocketPrivate m_oWebsocket;

        public CoinexPoisitionManager(CoinexWebsocketPrivate oWebsocket)
        {
            m_oWebsocket = oWebsocket;
        }
    
        public IFuturesPosition[] GetData()
        {
            List<IFuturesPosition> aResult = new List<IFuturesPosition>();
            foreach( string strId in m_aPositions.Keys )
            {
                IFuturesPosition? oFound = null;
                if( m_aPositions.TryGetValue( strId, out oFound ) ) aResult.Add( oFound );
            }
            return aResult.ToArray();
        }

        public IFuturesPosition? GetData(string strSymbol)
        {
            IFuturesPosition[] aAll = GetData();
            return aAll.FirstOrDefault(p=> p.Symbol.Symbol ==  strSymbol);  
        }

        public void Put(CoinExPositionUpdate oUpdate)
        {
            IFuturesSymbol? oSymbol = m_oWebsocket.FuturesSymbols.FirstOrDefault(p => p.Symbol == oUpdate.Position.Symbol);
            if (oSymbol == null) return;
            IFuturesPosition oPosition = new CoinexPoisitionLocal(oSymbol, oUpdate);
            if (oPosition.Quantity <= 0)
            {
                IFuturesPosition? oFound = null;
                m_aPositions.TryRemove(oPosition.Id, out oFound);
            }
            else
            {
                m_aPositions.AddOrUpdate(oPosition.Id, p => oPosition, (s, p) => { p.Update(oPosition); return p; });
            }
            return;
        }
    }
}
