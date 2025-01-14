﻿using Bitget.Net.Objects.Models.V2;
using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetBalance : IFuturesBalance
    {

        public BitgetBalance(BitgetFuturesBalanceUpdate oUpdate ) 
        {
            Currency = oUpdate.MarginAsset;
            Equity = oUpdate.Equity;

            MarginFreezed = oUpdate.Frozen;
            MarginUsed = oUpdate.Equity - oUpdate.MaxOpenPosAvailable;
            MarginAvaliable = oUpdate.MaxOpenPosAvailable;

        }

        public BitgetBalance(BitgetFuturesBalance oParsed)
        {
            Currency = oParsed.MarginAsset;
            Equity = oParsed.Equity;

            MarginFreezed = oParsed.Locked;
            MarginUsed = oParsed.Equity - oParsed.Available;
            MarginAvaliable = oParsed.Available;

        }

        public string Currency { get; }

        public decimal Equity { get; }

        public decimal ProfitUnrealized { get; } = 0;

        public decimal ProfitRealized { get; } = 0;

        public decimal MarginAvaliable { get; }

        public decimal MarginUsed { get; }

        public decimal MarginFreezed { get; private set; } = 0;
    }
}
