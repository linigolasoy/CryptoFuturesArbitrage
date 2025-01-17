using Bitget.Net.Objects.Models.V2;
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

namespace Crypto.Exchanges.All.Bitget.Websocket
{
    internal class BitgetBalanceManager : BaseBalanceManager, IWebsocketPrivateManager<IFuturesBalance>
    {

        public BitgetBalanceManager(IFuturesWebsocketPrivate oWebsocket): base(oWebsocket)
        {
        }

        public void Put( BitgetFuturesBalanceUpdate oUpdate )
        {
            IFuturesBalance oBalance = new BitgetBalance(oUpdate);
            PutData(oBalance.Currency, oBalance);
        }
    }
}
