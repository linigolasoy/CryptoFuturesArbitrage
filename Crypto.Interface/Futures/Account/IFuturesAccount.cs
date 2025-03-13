using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures.Account
{
    public interface IFuturesAccount
    {

        public IFuturesExchange Exchange { get; }

        public delegate Task PrivateDelegate(IWebsocketQueueItem oItem);

        public event PrivateDelegate? OnPrivateEvent;

        public Task PostEvent(IWebsocketQueueItem oItem);

        public Task<bool> StartSockets();
        public Task StopSockets();
        public Task<IFuturesBalance[]?> GetBalances();
        public Task<IFuturesPosition[]?> GetPositions();

        public IWebsocketManager<IFuturesBalance> BalanceManager { get; }
        public IWebsocketManager<IFuturesOrder> OrderManager { get; }
        public IWebsocketManager<IFuturesPosition> PositionManager { get; }

        // Money operations
        public Task<ITradingResult<decimal>> Transfer(decimal nAmoun, bool bFromSpot);
    }
}
