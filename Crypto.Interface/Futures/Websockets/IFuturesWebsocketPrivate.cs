using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures.Websockets
{
    public interface IFuturesWebsocketPrivate
    {

        public IFuturesExchange Exchange { get; }

        public Task<bool> Start();
        public Task Stop();

        public IWebsocketPrivateManager<IFuturesBalance> BalanceManager { get; }
        public IWebsocketPrivateManager<IFuturesOrder> OrderManager { get; }
        public IWebsocketPrivateManager<IFuturesPosition> PositionManager { get; }
    }
}
