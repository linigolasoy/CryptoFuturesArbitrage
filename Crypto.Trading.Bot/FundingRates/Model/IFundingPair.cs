using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.Model
{
    /// <summary>
    /// Represents a funding pair with its possible gain
    /// </summary>
    public interface IFundingPair
    {
        public IFundingDate FundingDate { get; }

        public decimal Percent { get; }

        public IFuturesSymbol BuySymbol { get; }
        public IFuturesSymbol SellSymbol { get; }

        public IFundingRate? BuyFunding { get; }
        public IFundingRate? SellFunding { get; }
    }

}
