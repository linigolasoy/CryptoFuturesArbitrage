﻿using BingX.Net.Objects.Models;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Websockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxBalance : IFuturesBalance
    {
        public BingxBalance(BingXFuturesBalance oBalance) 
        {
            Currency = oBalance.Asset;
            Equity = oBalance.Equity;
            ProfitUnrealized = oBalance.UnrealizedProfit;
            ProfitRealized = oBalance.RealizedProfit;
            MarginAvaliable = oBalance.AvailableMargin;
            MarginUsed = oBalance.UsedMargin;
            MarginFreezed = oBalance.FrozenMargin;
        }

        public BingxBalance(BingXFuturesBalanceChange oChange)
        {
            Currency = oChange.Asset;
            Equity = oChange.Balance;

            ProfitUnrealized = 0;
            ProfitRealized = 0;
            MarginAvaliable = oChange.Balance;
            MarginUsed = 0;
            MarginFreezed = 0;
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
