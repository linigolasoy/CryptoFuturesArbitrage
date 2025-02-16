using Crypto.Interface.Futures.History;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Interface;

namespace Crypto.Exchanges.All.Common
{
    internal interface IFuturesBarFeeder
    {
        public IFuturesExchange Exchange { get; }

        public delegate Task<IFuturesBar[]?> GetBarsDayDelegate(IFuturesSymbol oSymbol, Timeframe eTimeframe, DateTime dDate);

        public event GetBarsDayDelegate? OnGetBarsDay;
        // Bars
        public Task<IFuturesBar[]?> GetBars(IFuturesSymbol oSymbol, Timeframe eTimeframe, DateTime dFrom, DateTime dTo);
        public Task<IFuturesBar[]?> GetBars(IFuturesSymbol[] aSymbols, Timeframe eTimeframe, DateTime dFrom, DateTime dTo);
    }
}
