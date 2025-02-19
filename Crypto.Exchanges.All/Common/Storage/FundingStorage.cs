using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common.Storage
{
    internal class FundingStorage : IFundingRate
    {
        public FundingStorage(IFuturesSymbol oSymbol, FundingJson oJson) 
        { 
            Symbol = oSymbol;
            Rate = oJson.Rate;
            SettleDate = oJson.DateTime;
        }
        public IFuturesSymbol Symbol { get; }

        public decimal Rate { get; }

        public DateTime SettleDate { get; }

        public int Cycle { get => 8; }

        public void Update(IFundingRate obj)
        {
            throw new NotImplementedException();
        }
    }
}
