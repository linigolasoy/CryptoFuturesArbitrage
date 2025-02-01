using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Websockets;
using CryptoExchange.Net.CommonObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common
{
    internal class BasePositionManager
    {


        private ConcurrentDictionary<string, IFuturesPosition> m_aPositions = new ConcurrentDictionary<string, IFuturesPosition>();
        private ConcurrentBag<IFuturesPosition> m_aClosedPositions = new ConcurrentBag<IFuturesPosition>(); 
        public int ReceiveCount { get; private set; } = 0;
        public int Count { get => m_aPositions.Count; }
        public IFuturesWebsocketPrivate PrivateSocket { get; }

        public DateTime LastUpdate { get; private set; } = DateTime.Now;
        public BasePositionManager(IFuturesWebsocketPrivate oWs)
        { 
            PrivateSocket = oWs;
        }

        public IFuturesPosition[] GetData()
        {
            List<IFuturesPosition> aResults = new List<IFuturesPosition>();
            foreach (string strKey in m_aPositions.Keys)
            {
                IFuturesPosition? oData = GetData(strKey);
                if (oData != null) aResults.Add(oData);
            }
            return aResults.ToArray();
        }

        public IFuturesPosition? GetData(string strSymbol)
        {
            IFuturesPosition? oFound = null;
            if (m_aPositions.TryGetValue(strSymbol, out oFound)) { return oFound; }
            return null;
        }


        internal void PutData( IFuturesPosition[] aNew ) 
        {
            
            // Remove uniexistent
            string[] aDeleteKeys = m_aPositions.Keys.Where(p => !aNew.Any(q => q.Symbol.Symbol == p)).ToArray();
            foreach (string strDelete in aDeleteKeys)
            {
                ReceiveCount++;
                IFuturesPosition? oRemoved = null;
                if( m_aPositions.TryRemove(strDelete, out oRemoved) )
                {
                    oRemoved.Closed = true;
                    m_aClosedPositions.Add(oRemoved);
                    if (PrivateSocket is BasePrivateQueueManager)
                    {
                        ((BasePrivateQueueManager)PrivateSocket).Put(oRemoved);
                    }
                }
            }

            foreach (var oPos in aNew)
            {
                ReceiveCount++;
                m_aPositions.AddOrUpdate(oPos.Symbol.Symbol, p => AddFunction(oPos), (s, p) => UpdateFunction(p,oPos) );
            }
            LastUpdate = DateTime.Now;
        }

        internal void PutData( IFuturesPosition oNew )
        {
            m_aPositions.AddOrUpdate(oNew.Symbol.Symbol, p => AddFunction(oNew), (s, p) => UpdateFunction(p, oNew));
            LastUpdate = DateTime.Now;
        }

        internal void RemoveData( IFuturesPosition oNew )
        {
            IFuturesPosition? oRemoved = null;
            if (m_aPositions.TryRemove(oNew.Symbol.Symbol, out oRemoved))
            {
                oRemoved.Closed = true;
                m_aClosedPositions.Add(oRemoved);
                if (PrivateSocket is BasePrivateQueueManager)
                {
                    ((BasePrivateQueueManager)PrivateSocket).Put(oRemoved);
                }
            }

        }
        public IFuturesPosition UpdateFunction(IFuturesPosition oOld, IFuturesPosition oNew)
        {
            oOld.Update(oNew);
            if (PrivateSocket is BasePrivateQueueManager)
            {
                ((BasePrivateQueueManager)PrivateSocket).Put(oOld);
            }
            return oOld;
        }
        public IFuturesPosition AddFunction(IFuturesPosition oNew)
        {
            if (PrivateSocket is BasePrivateQueueManager)
            {
                ((BasePrivateQueueManager)PrivateSocket).Put(oNew);
            }
            return oNew;
        }

    }
}
