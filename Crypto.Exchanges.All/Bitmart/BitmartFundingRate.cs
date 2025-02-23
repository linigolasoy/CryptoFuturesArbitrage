using BitMart.Net.Objects.Models;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitmart
{
    internal class BitmartFundingRateLocal : BaseFundingRate, IFundingRate
    {

        public BitmartFundingRateLocal(IFuturesSymbol oSymbol, BitMartFundingRateUpdate oRate):
            base(oSymbol, oRate.ExpectedFundingRate, oRate.NextFundingTime!.Value.ToLocalTime())
        {
        }

        public BitmartFundingRateLocal(IFuturesSymbol oSymbol, BitMartFundingRateHistory oRate) :
            base(oSymbol, oRate.FundingRate, oRate.FundingTime.ToLocalTime())
        {
        }
    }

    internal class BitmartFundingRateSnapshot : IFundingRateSnapShot
    {
        public BitmartFundingRateSnapshot( IFuturesSymbol oSymbol, BitMartFundingRate oRate )
        {
            Symbol = oSymbol;
            SettleDate = oRate.NextFundingTime!.Value.ToLocalTime();
            Rate = oRate.ExpectedFundingRate;
            Maximum = oRate.FundingUpperLimit;
            Minimum = oRate.FundingLowerLimit;
            SnapshotDate = DateTime.Now;
        }
        public decimal Maximum { get; }

        public decimal Minimum { get; }

        public DateTime SnapshotDate { get; }

        public IFuturesSymbol Symbol { get; }

        public decimal Rate { get; private set; }

        public DateTime SettleDate { get; private set; }

        public int Cycle { get => 8; }

        public void Update(IFundingRate obj)
        {
            throw new NotImplementedException();
        }
    }
}
