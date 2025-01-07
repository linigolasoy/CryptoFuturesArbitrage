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
    internal class CoinexBalanceManager : IWebsocketManager<IFuturesBalance>
    {
        private ICryptoFuturesExchange m_oExchange;
        private Task m_oBalanceTask;

        private ConcurrentDictionary<string, IFuturesBalance> m_aBalances = new ConcurrentDictionary<string, IFuturesBalance>();
        public CoinexBalanceManager(ICryptoFuturesExchange oExchange) 
        { 
            m_oExchange = oExchange;
            m_oBalanceTask = DoInitialBalance();
        }

        private async Task DoInitialBalance()
        {
            try
            {
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
            return;
        }

    }
}
