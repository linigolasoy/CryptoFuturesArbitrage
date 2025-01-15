using Bitget.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
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
            SettleDate = oParsed.FundingTime!.Value.ToLocalTime();    
        }

        public BitgetFuturesFundingRate(IFuturesSymbol oSymbol, BitgetFuturesTickerUpdate oTicker)
        {
            Symbol = oSymbol;
            Rate = oTicker.FundingRate!.Value;
            SettleDate = oTicker.NextFundingTime!.Value.ToLocalTime();

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

    }


    internal class BitgetFuturesFundingRateSnap : IFundingRateSnapShot
    {
        public BitgetFuturesFundingRateSnap( IFuturesSymbol oSymbol, BitgetFundingRate oRate, BitgetFundingTime oTime)
        {
            Symbol = oSymbol;
            SettleDate = oTime.NextFundingTime!.Value.ToLocalTime();
            Rate = oRate.FundingRate;
            SnapshotDate = DateTime.Now;
            Cycle = 8;
            if( oTime.RatePeriod != null ) Cycle = oTime.RatePeriod.Value;  
        }
        public decimal Maximum { get => 100; }

        public decimal Minimum { get => -100; }

        public DateTime SettleDate { get; private set; }

        public IFuturesSymbol Symbol { get; }

        public decimal Rate { get; private set; }

        public DateTime SnapshotDate { get; }

        public int Cycle { get; internal set; }

        public void Update(IFundingRate obj)
        {
            Rate = obj.Rate;
            SettleDate = obj.SettleDate;
        }
    }
}
