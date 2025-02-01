using BingX.Net.Objects.Models;
using Crypto.Exchanges.All.Common;
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
    internal class BingxBalanceManager : BaseBalanceManager, IWebsocketPrivateManager<IFuturesBalance>
    {

        

        public BingxBalanceManager(IFuturesWebsocketPrivate oWebsocket) : base(oWebsocket)
        { 
        }

        public void Put(BingXFuturesBalanceChange oChange)
        {

            BingxBalance oBalance = new BingxBalance(oChange);
            PutData(oChange.Asset, oBalance);   
            return;
        }

    }
}
