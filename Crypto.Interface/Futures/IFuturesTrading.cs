using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures
{
    public interface IFuturesTrading
    {
        public ICryptoFuturesExchange Exchange { get; }

        public Task<bool> SetLeverage(IFuturesSymbol oSymbol, int nLeverage);
        public Task<IFuturesOrder?> CreateLimitOrder(IFuturesSymbol oSymbol, bool bBuy, bool bLong, decimal nQuantity, decimal nPrice);
        public Task<IFuturesOrder?> CreateMarketOrder(IFuturesSymbol oSymbol, bool bBuy, bool bLong, decimal nQuantity);

        public Task<bool> CancelOrder(IFuturesOrder oOrded);

        public Task<IFuturesLeverage[]?> GetLeverages(IFuturesSymbol[]? aSymbols = null);
        public Task<IFuturesLeverage?> GetLeverage( IFuturesSymbol oSymbol);

        public Task<IFuturesOrder[]?> GetOrders();

    }
}
