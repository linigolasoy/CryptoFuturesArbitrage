using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures.Account
{
    public interface IFuturesBalance: IWebsocketQueueItem
    {
        // equity":"120.2596","unrealizedProfit":"0.0000","realisedProfit":"0","availableMargin":"120.2596","usedMargin":"0.0000","freezedMargin":"0.0000"

        public string Currency { get; }
        public decimal Equity { get; }
        public decimal ProfitUnrealized { get; }
        public decimal ProfitRealized { get; }
        public decimal MarginAvaliable { get; }
        public decimal MarginUsed { get; }
        public decimal MarginFreezed { get; }
    }
}
