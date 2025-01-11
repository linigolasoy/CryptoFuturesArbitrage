using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Websockets
{
    public interface IWebsocketPrivate
    {
        public ICryptoFuturesExchange Exchange { get; }

        public IFuturesSymbol[] FuturesSymbols { get; }

        public Task<bool> Start();

        public IWebsocketManager<IFuturesBalance> BalanceManager { get; }
        public IWebsocketManager<IFuturesOrder> OrderManager { get; }
        public IWebsocketManager<IFuturesPosition> PositionManager { get; }
    }
}
