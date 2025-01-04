using Bitget.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetFuturesFundingRate : IFundingRate
    {
        public BitgetFuturesFundingRate( IFuturesSymbol oSymbol, BitgetFundingRate oParsed) 
        {
            Symbol = oSymbol;
            Rate = oParsed.FundingRate;
            DateTime = oParsed.FundingTime!.Value.ToLocalTime();    
        }
        public IFuturesSymbol Symbol { get; }

        public decimal Rate { get; }

        public DateTime DateTime { get; }

        public int Cycle { get => 8; }
    }


    internal class BitgetFuturesFundingRateSnap : IFundingRateSnapShot
    {
        public BitgetFuturesFundingRateSnap( IFuturesSymbol oSymbol, BitgetFundingRate oRate, BitgetFundingTime oTime)
        {
            Symbol = oSymbol;
            NextSettle = oTime.NextFundingTime!.Value.ToLocalTime();
            Rate = oRate.FundingRate;
            DateTime = DateTime.Now;
            Cycle = 8;
            if( oTime.RatePeriod != null ) Cycle = oTime.RatePeriod.Value;  
        }
        public decimal Maximum { get => 100; }

        public decimal Minimum { get => -100; }

        public DateTime NextSettle { get; }

        public IFuturesSymbol Symbol { get; }

        public decimal Rate { get; }

        public DateTime DateTime { get; }

        public int Cycle { get; internal set; }
    }
}
