using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common.Storage
{
    internal interface IFundingRateFeeder
    {
        public IFuturesExchange Exchange { get; }

        public delegate Task<IFundingRate[]?> GetFundingDelegate(IFuturesSymbol oSymbol, DateTime dFrom);

        public event GetFundingDelegate? OnGetFunding;
        // Bars
        public Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol oSymbol, DateTime dFrom);
        public Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol[] aSymbols, DateTime dFrom);
    }
}
