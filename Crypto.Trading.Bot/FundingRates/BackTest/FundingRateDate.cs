using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.FundingRates.BackTest
{
    internal class FundingRateDate
    {

        public FundingRateDate( DateTime dDate, IFundingRate[] aFundingRates ) 
        { 
            throw new NotImplementedException();    
        }
        public DateTime DateTime { get; }

        public FundingRateData[] Data { get; }  
    }
}
