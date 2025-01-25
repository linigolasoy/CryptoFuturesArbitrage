using BitMart.Net.Objects.Models;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Websockets;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitmart.Websocket
{


    internal class BitmartBalanceManager : BaseBalanceManager, IWebsocketPrivateManager<IFuturesBalance>
    {

        public BitmartBalanceManager(IFuturesWebsocketPrivate oWebsocket): base(oWebsocket) 
        { 
        }

        public void Put(DataEvent<BitMartFuturesBalanceUpdate> oUpdate)
        {
            if (oUpdate == null || oUpdate.Data == null) return;
            IFuturesBalance oBalance = new BitmartBalance(oUpdate.Data);    
            PutData(oBalance.Currency, oBalance);   
            return;
        }
    }
}
