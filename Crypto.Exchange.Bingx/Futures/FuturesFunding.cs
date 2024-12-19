using Crypto.Exchange.Bingx.Responses;
using Crypto.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Bingx.Futures
{
    internal class FuturesFunding : IFundingRate
    {
        public FuturesFunding(IFuturesSymbol oSymbol, FundingHistoryParsed oParsed )
        {
            Symbol = oSymbol;
            Rate = (decimal)oParsed.FundingRate;
            DateTime = BingxCommon.ParseUnixTimestamp(oParsed.SettleTime);
        }
        public IFuturesSymbol Symbol { get; }

        public decimal Rate { get; }

        public DateTime DateTime { get; }

        public int Cycle { get => 8; }
    }
}
