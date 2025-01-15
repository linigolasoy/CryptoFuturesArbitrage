using BingX.Net.Objects.Models;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Websockets;
using CryptoExchange.Net.RateLimiting.Guards;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx.Websocket
{
    internal class BingxBalanceManager : IWebsocketManager<IFuturesBalance>
    {

        private IFuturesWebsocketPrivate m_oWebsocket;

        private ConcurrentDictionary<string, BingxBalance> m_aBalances = new ConcurrentDictionary<string, BingxBalance>();
        public int ReceiveCount { get; private set; } = 0;

        public int Count { get=> m_aBalances.Count; }
        public BingxBalanceManager(IFuturesWebsocketPrivate oWebsocket) 
        { 
            m_oWebsocket = oWebsocket;
        }
        public IFuturesBalance[] GetData()
        {
            List<IFuturesBalance> aResult = new List<IFuturesBalance>();
            foreach( string strAsset in m_aBalances.Keys ) 
            {
                BingxBalance? oAdd = null;
                if( m_aBalances.TryGetValue(strAsset, out oAdd)) aResult.Add(oAdd); 
            }
            return aResult.ToArray();
        }

        public IFuturesBalance? GetData(string strSymbol)
        {
            BingxBalance? oAdd = null;
            if (m_aBalances.TryGetValue(strSymbol, out oAdd)) return oAdd;
            return null;
        }

        public void Put(BingXFuturesBalanceChange oChange)
        {

            ReceiveCount++;
            BingxBalance oBalance = new BingxBalance(oChange);
            m_aBalances.AddOrUpdate(oChange.Asset, (strKey) => { return oBalance; }, (strKey, oSource) =>
            {
                return oBalance;
            });
            return;
        }
    }
}
