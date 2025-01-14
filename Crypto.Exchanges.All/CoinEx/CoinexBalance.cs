using CoinEx.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexBalance : IFuturesBalance
    {

        public CoinexBalance( CoinExFuturesBalance oBalance) 
        {
            Currency = oBalance.Asset;
            Equity = oBalance.Available;
            MarginAvaliable = oBalance.Available;
            MarginUsed = oBalance.Margin;
            ProfitUnrealized = oBalance.UnrealizedPnl;
            ProfitRealized = 0;
            MarginFreezed = 0;  
        }
        public string Currency { get; }

        public decimal Equity { get; }

        public decimal ProfitUnrealized { get; }

        public decimal ProfitRealized { get; }

        public decimal MarginAvaliable { get; }

        public decimal MarginUsed { get; }

        public decimal MarginFreezed { get; }
    }
}
