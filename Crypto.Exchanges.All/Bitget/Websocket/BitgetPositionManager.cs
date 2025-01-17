using Bitget.Net.Objects.Models.V2;
using Crypto.Exchanges.All.Common;
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

namespace Crypto.Exchanges.All.Bitget.Websocket
{
    internal class BitgetPositionManager : BasePositionManager, IWebsocketPrivateManager<IFuturesPosition>
    {

        public BitgetPositionManager(BitgetWebsocketPrivate oWebsocket): base(oWebsocket)
        {
        }

        public void Put(IEnumerable<BitgetPositionUpdate> aParsed)
        {
            List<IFuturesPosition> aNews = new List<IFuturesPosition>();    
            foreach( var oParsed in aParsed )
            {
                IFuturesSymbol? oFound = PrivateSocket.FuturesSymbols.FirstOrDefault(p=> p.Symbol == oParsed.Symbol);
                if (oFound == null) continue;
                IFuturesPosition oPos = new BitgetPositionLocal(oFound, oParsed);
                aNews.Add(oPos);    
            }

            PutData(aNews.ToArray());
        }

        /*
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
        */

    }
}
