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

        public FundingRateDate( DateTime dDate, Dictionary<string, List<IFundingRate>> aRates ) 
        {
            DateTime = dDate;
            List<FundingRateData> aData = new List<FundingRateData>();  
            foreach( var oData in aRates )
            {
                aData.Add(new FundingRateData(this, oData.Key, oData.Value.ToArray()));
            }
            Data = aData.ToArray();

        }
        public DateTime DateTime { get; }

        public FundingRateData[] Data { get; }  
    }
}
