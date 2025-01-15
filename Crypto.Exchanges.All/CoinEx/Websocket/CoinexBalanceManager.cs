using CoinEx.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx.Websocket
{
    internal class CoinexBalanceManager : IWebsocketManager<IFuturesBalance>
    {
        private IFuturesExchange m_oExchange;
        private Task m_oBalanceTask;

        private ConcurrentDictionary<string, IFuturesBalance> m_aBalances = new ConcurrentDictionary<string, IFuturesBalance>();
        public int ReceiveCount { get; private set; } = 0;
        public int Count { get => m_aBalances.Count; }
        public CoinexBalanceManager(IFuturesExchange oExchange) 
        { 
            m_oExchange = oExchange;
            m_oBalanceTask = DoInitialBalance();
        }

        private async Task DoInitialBalance()
        {
            try
            {
                int nRetries = 10;
                while(m_oExchange.Account == null && nRetries > 0 )
                {
                    await Task.Delay(500);
                    nRetries--;
                }
                if (m_oExchange.Account == null) return;
                IFuturesBalance[]? aBalances = await m_oExchange.Account.GetBalances();
                if (aBalances == null) return;
                foreach( var oBalance in aBalances )
                {
                    m_aBalances.AddOrUpdate( oBalance.Currency, p=> oBalance, (s,p)=> p);
                }


            }
            catch (Exception ex)
            {

            }
        }

        public IFuturesBalance[] GetData()
        {
            List<IFuturesBalance> aResult = new List<IFuturesBalance>();
            foreach( string strKey  in m_aBalances.Keys )
            {
                IFuturesBalance? oFound = GetData( strKey );
                if( oFound != null ) aResult.Add( oFound );   
            }
                
            return aResult.ToArray();
        }

        public IFuturesBalance? GetData(string strSymbol)
        {
            IFuturesBalance? oResult = null;
            if( m_aBalances.TryGetValue(strSymbol, out oResult) )
            {
                return oResult;
            }
            return null;
        }

        public void Put(CoinExFuturesBalance oUpdate )
        {
            ReceiveCount++;
            IFuturesBalance oNewBalance = new CoinexBalance(oUpdate);
            m_aBalances.AddOrUpdate(oNewBalance.Currency, p => oNewBalance, (s, p) => oNewBalance);
            return;
        }

    }
}
