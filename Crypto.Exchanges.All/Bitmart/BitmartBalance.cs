using BitMart.Net.Objects.Models;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitmart
{
    internal class BitmartBalance: IFuturesBalance
    {
        public BitmartBalance(BitMartFuturesBalanceUpdate oUpdate) 
        {
            Currency = oUpdate.Asset;
            Equity = oUpdate.Frozen + oUpdate.PositionMargin + oUpdate.Available;
            ProfitUnrealized = 0;
            ProfitRealized = 0;
            MarginAvaliable = oUpdate.Available;
            MarginUsed = oUpdate.PositionMargin;
            MarginFreezed = oUpdate.Frozen;
        }
        public WebsocketQueueType QueueType { get => WebsocketQueueType.Balance; }
        public string Currency { get; }

        public decimal Equity { get; }

        public decimal ProfitUnrealized { get; }

        public decimal ProfitRealized { get; }

        public decimal MarginAvaliable { get; }

        public decimal MarginUsed { get; }

        public decimal MarginFreezed { get; }
    }
}
