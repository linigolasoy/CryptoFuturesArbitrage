using CoinEx.Net.Objects.Models.V2;
using Crypto.Exchanges.All.Common;
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
    internal class CoinexBalanceManager : BaseBalanceManager, IWebsocketPrivateManager<IFuturesBalance>
    {
        public CoinexBalanceManager(IFuturesWebsocketPrivate oWebsocket) : base(oWebsocket) 
        { 
        }

        internal async Task LoadInitialBalances()
        {
            IFuturesBalance[]? aBalances = await this.PrivateSocket.Exchange.Account.GetBalances();
            if (aBalances == null || aBalances.Length <= 0) return;
            foreach( var oBalance in aBalances )
            {
                PutData(oBalance.Currency, oBalance);   
            }
        }
        public void Put(CoinExFuturesBalance oUpdate )
        {
            IFuturesBalance oNewBalance = new CoinexBalance(oUpdate);
            PutData(oNewBalance.Currency, oNewBalance); 
            return;
        }

    }
}
