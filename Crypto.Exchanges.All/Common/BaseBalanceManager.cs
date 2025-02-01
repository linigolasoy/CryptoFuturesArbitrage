using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Websockets;
using CryptoClients.Net.Enums;
using Microsoft.VisualBasic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common
{
    internal class BaseBalanceManager
    {

        private ConcurrentDictionary<string, IFuturesBalance> m_aBalances = new ConcurrentDictionary<string, IFuturesBalance>();
        public int ReceiveCount { get; internal set; } = 0;

        public int Count { get => m_aBalances.Count; }
        public DateTime LastUpdate { get; private set; } = DateTime.Now;
        public BaseBalanceManager(IFuturesWebsocketPrivate oWebsocket) 
        { 
            PrivateSocket = oWebsocket; 
        }
        public IFuturesWebsocketPrivate PrivateSocket { get; }

        public IFuturesBalance AddFunction(IFuturesBalance oNew)
        {
            if (PrivateSocket is BasePrivateQueueManager)
            {
                ((BasePrivateQueueManager)PrivateSocket).Put(oNew);
            }
            return oNew;
        }

        public IFuturesBalance UpdateFunction(IFuturesBalance oOld, IFuturesBalance oNew)
        {
            if (PrivateSocket is BasePrivateQueueManager)
            {
                ((BasePrivateQueueManager)PrivateSocket).Put(oNew);
            }
            return oNew;
        }

        public IFuturesBalance[] GetData()
        {
            List<IFuturesBalance> aResult = new List<IFuturesBalance>();
            foreach (string strAsset in m_aBalances.Keys)
            {
                IFuturesBalance? oAdd = null;
                if (m_aBalances.TryGetValue(strAsset, out oAdd)) aResult.Add(oAdd);
            }
            return aResult.ToArray();
        }

        public IFuturesBalance? GetData(string strSymbol)
        {
            IFuturesBalance? oAdd = null;
            if (m_aBalances.TryGetValue(strSymbol, out oAdd)) return oAdd;
            return null;
        }


        internal void PutData( string strKey, IFuturesBalance oNew )
        {
            LastUpdate = DateTime.Now;
            ReceiveCount++;
            m_aBalances.AddOrUpdate(strKey, (strKey) => AddFunction(oNew), (strKey, oSource) => UpdateFunction(oSource, oNew));
            ReceiveCount++;

        }
    }
}
