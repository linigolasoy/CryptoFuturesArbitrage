using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures
{
    public interface ICryptoFuturesExchange : IExchange
    {

        public Task<IFuturesSymbol[]?> GetSymbols();

        public Task<IFundingRateSnapShot?> GetFundingRates(IFuturesSymbol oSymbol);
        public Task<IFundingRateSnapShot[]?> GetFundingRates(IFuturesSymbol[] aSymbols);


        public Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol oSymbol, DateTime dFrom);
        public Task<IFundingRate[]?> GetFundingRatesHistory(IFuturesSymbol[] aSymbols, DateTime dFrom);

        public IFuturesBarFeeder BarFeeder { get; }

        public Task<IFuturesBalance[]?> GetBalances();

        public Task<IFuturesOrder?> CreateLimitOrder( IFuturesSymbol oSymbol, bool bBuy, decimal nMargin, int nLeverage, decimal nPrice );
    }
}
