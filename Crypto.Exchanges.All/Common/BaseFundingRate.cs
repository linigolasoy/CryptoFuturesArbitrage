using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common
{
    internal class BaseFundingRate : IFundingRate
    {

        public BaseFundingRate(IFuturesSymbol oSymbol, decimal nRate, DateTime dDate ) 
        { 
            Symbol = oSymbol;
            Rate = nRate;
            SettleDate = dDate;
        }

        public IFuturesSymbol Symbol { get; }

        public decimal Rate { get; private set; }

        public DateTime SettleDate { get; private set; }

        public int Cycle { get => 8; }

        public void Update(IFundingRate obj)
        {
            Rate = obj.Rate;
            SettleDate = obj.SettleDate;
        }

        public override string ToString()
        {
            return $"{SettleDate.ToShortDateString()} {SettleDate.ToShortTimeString()} {Symbol.ToString()} {Rate.ToString()}";
        }
    }
}
