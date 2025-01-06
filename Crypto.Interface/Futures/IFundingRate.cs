using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures
{
    public interface IFundingRateSnapShot : IFundingRate
    {
        public decimal Maximum { get; }
        public decimal Minimum { get; }

        public DateTime SnapshotDate { get; }

    }

    public interface IFundingRate
    {
        public IFuturesSymbol Symbol { get; }
        public decimal Rate { get; }
        public DateTime SettleDate { get; }
        public int Cycle { get; }
    }
}
