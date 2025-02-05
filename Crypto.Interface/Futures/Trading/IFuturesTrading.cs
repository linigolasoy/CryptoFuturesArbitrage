using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;

namespace Crypto.Interface.Futures.Trading
{
    public interface IFuturesTrading
    {
        public IFuturesExchange Exchange { get; }

        public Task<ITradingResult<bool>> SetLeverage(IFuturesSymbol oSymbol, int nLeverage);
        public Task<ITradingResult<IFuturesOrder?>> CreateLimitOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity, decimal nPrice);
        public Task<ITradingResult<IFuturesOrder?>> CreateMarketOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity);

        public Task<ITradingResult<bool>> ClosePosition(IFuturesPosition oPosition, decimal? nPrice = null);

        public Task<ITradingResult<bool>> CancelOrder(IFuturesOrder oOrded);

        public Task<IFuturesLeverage[]?> GetLeverages(IFuturesSymbol[]? aSymbols = null);
        public Task<IFuturesLeverage?> GetLeverage( IFuturesSymbol oSymbol);

        public Task<IFuturesOrder[]?> GetOrders();

    }
}
