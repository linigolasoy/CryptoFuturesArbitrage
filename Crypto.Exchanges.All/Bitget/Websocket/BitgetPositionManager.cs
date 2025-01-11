using Bitget.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using Crypto.Interface.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget.Websocket
{
    internal class BitgetPositionManager : IWebsocketManager<IFuturesPosition>
    {

        private BitgetWebsocketPrivate m_oWebsocket;

        private ConcurrentDictionary<string, IFuturesPosition> m_aPositions = new ConcurrentDictionary<string, IFuturesPosition>();
        public int ReceiveCount { get; private set; } = 0;
        public BitgetPositionManager(BitgetWebsocketPrivate oWebsocket)
        {
            m_oWebsocket = oWebsocket;
        }
    
        public IFuturesPosition[] GetData()
        {
            List<IFuturesPosition> aResult = new List<IFuturesPosition>();
            foreach( string strSymbol in m_aPositions.Keys )
            {
                IFuturesPosition? oFound = null;
                if( m_aPositions.TryGetValue(strSymbol, out oFound ) ) { if( oFound != null ) aResult.Add( oFound ); }  
            }
            return aResult.ToArray();
        }

        public IFuturesPosition? GetData(string strSymbol)
        {
            IFuturesPosition[] aAll = GetData();    
            return aAll.FirstOrDefault(p=> p.Symbol.Symbol ==  strSymbol);  
        }

        public void Put(IEnumerable<BitgetPositionUpdate> aParsed)
        {
            ReceiveCount++;
            List<IFuturesPosition> aNews = new List<IFuturesPosition>();    
            foreach( var oParsed in aParsed )
            {
                IFuturesSymbol? oFound = m_oWebsocket.FuturesSymbols.FirstOrDefault(p=> p.Symbol == oParsed.Symbol);
                if (oFound == null) continue;
                IFuturesPosition oPos = new BitgetPositionLocal(oFound, oParsed);

                m_aPositions.AddOrUpdate(oFound.Symbol, p => oPos, (s, p) => { p.Update(oPos); return p; });
                aNews.Add(oPos);    
            }


        }

        public void PutHistory(IEnumerable<BitgetPositionHistoryUpdate> aParsed)
        {
            ReceiveCount++;
            foreach( var oParsed in aParsed )
            {
                IFuturesPosition? oPos = null;
                if( m_aPositions.TryGetValue(oParsed.Symbol, out oPos) )
                {
                    if (oPos == null) continue;
                    // if( oPos.u)
                    if( oPos.LastUpdate < oParsed.UpdateTime.ToLocalTime() ) 
                    { 
                        m_aPositions.TryRemove(oParsed.Symbol, out oPos);   
                    }
                }
            }
            return;


        }

    }
}
