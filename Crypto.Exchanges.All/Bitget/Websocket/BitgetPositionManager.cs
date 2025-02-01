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

        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();    
        private Task? m_oMainLoop = null;
        public BitgetPositionManager(BitgetWebsocketPrivate oWebsocket): base(oWebsocket)
        {
        }

        public void Put(IEnumerable<BitgetPositionUpdate> aParsed)
        {
            List<IFuturesPosition> aNews = new List<IFuturesPosition>();    
            foreach( var oParsed in aParsed )
            {
                IFuturesSymbol? oFound = PrivateSocket.Exchange.SymbolManager.GetSymbol(oParsed.Symbol);
                if (oFound == null) continue;
                IFuturesPosition oPos = new BitgetPositionLocal(oFound, oParsed);
                aNews.Add(oPos);    
            }

            PutData(aNews.ToArray());
        }

        internal async Task<bool> Start()
        {
            await Stop();
            m_oCancelSource = new CancellationTokenSource();
            m_oMainLoop = MainLoop();
            return true;
        }

        internal async Task Stop()
        {
            m_oCancelSource.Cancel();
            if( m_oMainLoop != null)
            {
                await m_oMainLoop;
                m_oMainLoop = null;
            }
        }


        private async Task MainLoop()
        {
            while(!m_oCancelSource.IsCancellationRequested)
            {
                try
                {
                    IFuturesPosition[]? aPositions = await PrivateSocket.Exchange.Account.GetPositions();
                    if( aPositions != null)
                    {
                        PutData(aPositions);    
                    }
                }
                catch( Exception e ) { }
                await Task.Delay(500);
            }

            await Task.Delay(500);
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
