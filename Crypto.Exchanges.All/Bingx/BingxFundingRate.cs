using BingX.Net.Objects.Models;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxFundingRateSnapshot : IFundingRateSnapShot
    {

        public BingxFundingRateSnapshot(IFuturesSymbol oSymbol, BingXFundingRate oRate )
        {
            Symbol = oSymbol;
            SnapshotDate = DateTime.Now;
            Rate = oRate.LastFundingRate;
            SettleDate = oRate.NextFundingTime.ToLocalTime();
        }
        public decimal Maximum { get => 100.0M; }

        public decimal Minimum { get => -100.0M; }

        public DateTime SettleDate { get; }

        public IFuturesSymbol Symbol { get; }

        public decimal Rate { get;}

        public DateTime SnapshotDate { get; }

        public int Cycle { get => 8; }
    }

    internal class BingxFundingRate : IFundingRate
    {

        public BingxFundingRate(IFuturesSymbol oSymbol, BingXFundingRateHistory oRate)
        {
            Symbol = oSymbol;
            SettleDate = oRate.FundingTime.ToLocalTime();
            Rate = oRate.FundingRate;
        }
        public IFuturesSymbol Symbol { get; }

        public decimal Rate { get; }

        public DateTime SettleDate { get; }

        public int Cycle { get => 8; }
    }
}
