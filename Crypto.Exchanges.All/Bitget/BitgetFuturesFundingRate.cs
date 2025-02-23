using Bitget.Net.Objects.Models.V2;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetFuturesFundingRate : BaseFundingRate, IFundingRate
    {

        public BitgetFuturesFundingRate( IFuturesSymbol oSymbol, BitgetFundingRate oParsed) :
            base(oSymbol, oParsed.FundingRate, oParsed.FundingTime!.Value.ToLocalTime())
        {
        }

        public BitgetFuturesFundingRate(IFuturesSymbol oSymbol, BitgetFuturesTickerUpdate oTicker):
            base(oSymbol, oTicker.FundingRate!.Value, oTicker.NextFundingTime!.Value.ToLocalTime())
        {
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
