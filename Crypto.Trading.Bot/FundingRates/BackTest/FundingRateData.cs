using Crypto.Interface.Futures.Market;
using Crypto.Trading.Bot.BackTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.BackTest
{
    internal class FundingRateData
    {
        public FundingRateData(FundingRateDate oFundingDate, IFundingRate[] aRates) 
        { 
            FundingDate = oFundingDate;
            FundingRates = aRates;
        }
        public FundingRateDate FundingDate { get; }

        public IFundingRate[] FundingRates { get; }

    }
}
