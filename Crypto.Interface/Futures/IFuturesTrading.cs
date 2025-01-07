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
        public Task<IFuturesOrder?> CreateLimitOrder(IFuturesSymbol oSymbol, bool bBuy, decimal nQuantity, decimal nPrice);
        public Task<IFuturesOrder?> CreateMarketOrder(IFuturesSymbol oSymbol, bool bBuy, decimal nQuantity, decimal nPrice);

    }
}
