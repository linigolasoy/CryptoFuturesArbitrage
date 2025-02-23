using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common.Storage
{
    internal class FundingStorage : BaseFundingRate, IFundingRate
    {
        public FundingStorage(IFuturesSymbol oSymbol, FundingJson oJson) :
            base(oSymbol, oJson.Rate, oJson.DateTime)
        { 
        }
    }
}
