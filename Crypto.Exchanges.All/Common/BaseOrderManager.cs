using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using Microsoft.VisualBasic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common
{
    internal class BaseOrderManager
    {
        private ConcurrentDictionary<string, IFuturesOrder> m_aOrders = new ConcurrentDictionary<string, IFuturesOrder>();

        public int ReceiveCount { get; private set; } = 0;
        public int Count { get => m_aOrders.Count; }

        public DateTime LastUpdate { get; private set; } = DateTime.Now;
        public IFuturesWebsocketPrivate PrivateSocket { get; }
        public BaseOrderManager( IFuturesWebsocketPrivate oWebsocket )
        {
            PrivateSocket = oWebsocket; 
        }


        public IFuturesOrder[] GetData()
        {
            List<IFuturesOrder> aResult = new List<IFuturesOrder>();
            foreach (string strId in m_aOrders.Keys)
            {
                IFuturesOrder? oFound = null;
                if (m_aOrders.TryGetValue(strId, out oFound))
                {
                    aResult.Add(oFound);
                }
            }
            return aResult.ToArray();
        }

        public IFuturesOrder? GetData(string strSymbol)
        {
            IFuturesOrder? oResult = GetData().FirstOrDefault(p => p.Symbol.Symbol == strSymbol);

            return oResult;
        }

        internal void PutData( string nKey, IFuturesOrder oNew )
        {
            ReceiveCount++;
            LastUpdate = oNew.TimeUpdated;
            m_aOrders.AddOrUpdate(oNew.Id, p => AddFunction(oNew), (s, p) => UpdateFunction(p,oNew) );
        }

        internal void PutData(IFuturesOrder[] aNew)
        {
            foreach (var oNew in aNew)
            {
                ReceiveCount++;
                m_aOrders.AddOrUpdate(oNew.Id, p => AddFunction(oNew), (s, p) => UpdateFunction(p, oNew));
                LastUpdate = oNew.TimeUpdated;

            }
        }

        public IFuturesOrder AddFunction(IFuturesOrder oNew)
        {
            if (PrivateSocket is BasePrivateQueueManager)
            {
                ((BasePrivateQueueManager)PrivateSocket).Put(oNew);
            }
            return oNew;
        }
        public IFuturesOrder UpdateFunction(IFuturesOrder oOld, IFuturesOrder oNew)
        {
            oOld.Update(oNew);
            if (PrivateSocket is BasePrivateQueueManager)
            {
                ((BasePrivateQueueManager)PrivateSocket).Put(oOld);
            }
            return oOld;
        }

    }
}
