using Crypto.Exchange.Mexc.Responses;
using Crypto.Interface;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc.Futures
{
    internal class FuturesFunding : IFundingRate
    {

        public FuturesFunding( IFuturesSymbol oSymbol, FundingHistoryParsed oParsed ) 
        {
            Symbol = oSymbol;
            Rate = (decimal)oParsed.FundingRate;
            Cycle = oParsed.Cycle;
            DateTime = MexcCommon.ParseUnixTimestamp(oParsed.SettleTime);
        }
        public IFuturesSymbol Symbol { get; }

        public decimal Rate { get; }

        public DateTime DateTime { get; }
        public int Cycle { get; }
    }

    internal class FuturesFundingStapshot : IFundingRateSnapShot
    {

        public FuturesFundingStapshot( IFuturesSymbol oSymbol, FuturesFundingParsed oParsed ) 
        { 
            Symbol = oSymbol;
            Rate = (decimal)oParsed.FundingRate;
            Maximum = (decimal)oParsed.MaxFundingRate;
            Minimum = (decimal)oParsed.MinFundingRate;
            Cycle = oParsed.CollectCycle;
            NextSettle = MexcCommon.ParseUnixTimestamp(oParsed.NextSettleTime);
            DateTime = MexcCommon.ParseUnixTimestamp(oParsed.ActualTimestamp);
        }
        public IFuturesSymbol Symbol { get; }

        public decimal Rate { get; }

        public decimal Maximum { get; }

        public decimal Minimum { get; }

        public int Cycle { get; }

        public DateTime NextSettle { get; }

        public DateTime DateTime { get; }
    }
}
