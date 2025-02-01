using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Crypto.Interface.Futures.Websockets.IFuturesWebsocketPrivate;

namespace Crypto.Exchanges.All.Common
{
    internal class BasePrivateQueueManager
    {



        private ConcurrentQueue<IWebsocketQueueItem> m_oQueue = new ConcurrentQueue<IWebsocketQueueItem> ();    
        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();
        private Task? m_oMainLoop = null;
        private IFuturesAccount m_oAccount;
        public BasePrivateQueueManager(IFuturesAccount oAccount) 
        { 
            m_oAccount = oAccount;
        }

        private async Task MainLoop()
        {
            while( !m_oCancelSource.IsCancellationRequested )
            {
                IWebsocketQueueItem? oNewItem = null;   
                if( m_oQueue.TryDequeue( out oNewItem ) )
                {
                    await m_oAccount.PostEvent(oNewItem);
                }
                else await Task.Delay(100);
            }
        }

        internal void Put( IWebsocketQueueItem oItem )
        {
            m_oQueue.Enqueue(oItem);
        }

        internal async Task StopLoop()
        {
            m_oCancelSource.Cancel();
            if( m_oMainLoop != null )
            {
                await m_oMainLoop;
            }
        }

        internal async Task StartLoop()
        {
            await StopLoop();
            m_oCancelSource = new CancellationTokenSource();
            m_oMainLoop = MainLoop();
        }
    }
}
