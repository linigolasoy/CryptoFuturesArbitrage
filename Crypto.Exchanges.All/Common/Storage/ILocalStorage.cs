using Crypto.Interface;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common.Storage
{
    internal interface ILocalStorage
    {
        public IFuturesExchange Exchange { get; }

        public IFuturesBar[]? GetBars(IFuturesSymbol oSymbol, Timeframe eFrame, DateTime dDate);

        public void SetBars(IFuturesBar[] aBars);

        public IFundingRate[]? GetFundingRates(string strSymbol);
        public void SetFundingRates(string strSymbol, IFundingRate[] aRates);
    }
}
