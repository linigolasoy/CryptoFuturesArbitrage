using Bitget.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget.Websocket
{
    internal class BitgetBalanceManager : IWebsocketManager<IFuturesBalance>
    {

        private ConcurrentDictionary<string, IFuturesBalance> m_aBalances = new ConcurrentDictionary<string, IFuturesBalance>();
        public int ReceiveCount { get; private set; } = 0;
        public int Count { get=> m_aBalances.Count; }   
        public BitgetBalanceManager()
        {

        }
        public IFuturesBalance[] GetData()
        {
            List<IFuturesBalance> aResult = new List<IFuturesBalance>();
            foreach( string strKey in  m_aBalances.Keys ) 
            { 
                IFuturesBalance? oFound = GetData( strKey );    
                if ( oFound != null ) { aResult.Add( oFound ); }
            }
            return aResult.ToArray();
        }

        public IFuturesBalance? GetData(string strSymbol)
        {
            IFuturesBalance? oResult = null;
            if( m_aBalances.TryGetValue(strSymbol, out oResult) ) {  return oResult; }
            return null;
        }

        public void Put( BitgetFuturesBalanceUpdate oUpdate )
        {
            ReceiveCount++;
            IFuturesBalance oBalance = new BitgetBalance(oUpdate);
            m_aBalances.AddOrUpdate( oBalance.Currency, s=> oBalance, (s,p) => oBalance);   
        }
    }
}
