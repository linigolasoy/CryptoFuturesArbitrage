using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface
{
    public interface ICryptoFuturesExchange : IExchange
    {

        public Task<IFuturesSymbol[]?> GetSymbols();

        public Task<IFundingRateSnapShot?> GetFundingRates(IFuturesSymbol oSymbol);
        public Task<IFundingRateSnapShot[]?> GetFundingRates(IFuturesSymbol[] aSymbols);


        public Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol oSymbol);
        public Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol[] aSymbols);

        // Bars
        public Task<IFuturesBar[]?> GetBars( IFuturesSymbol oSymbol, Timeframe eTimeframe, DateTime dFrom, DateTime dTo);
        public Task<IFuturesBar[]?> GetBars(IFuturesSymbol[] aSymbols, Timeframe eTimeframe, DateTime dFrom, DateTime dTo);

    }
}
