﻿using Crypto.Interface.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures
{
    public interface IFuturesAccount
    {

        public ICryptoFuturesExchange Exchange { get; }


        public Task<bool> StartSockets();
        public Task<IFuturesBalance[]?> GetBalances();
        public Task<IFuturesPosition[]?> GetPositions();

        public IWebsocketManager<IFuturesBalance> BalanceManager { get; }
        public IWebsocketManager<IFuturesOrder> OrderManager { get; }
        public IWebsocketManager<IFuturesPosition> PositionManager { get; }
    }
}
