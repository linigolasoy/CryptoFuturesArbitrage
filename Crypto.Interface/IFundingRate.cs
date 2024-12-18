using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface
{


    public interface IFundingRateSnapShot: IFundingRate
    {
        public decimal Maximum { get; }
        public decimal Minimum { get; }

        public DateTime NextSettle { get; }

    }

    public interface IFundingRate
    {
        public IFuturesSymbol Symbol { get; }   
        public decimal Rate { get; }
        public DateTime DateTime { get; }
        public int Cycle { get; }
    }
}
