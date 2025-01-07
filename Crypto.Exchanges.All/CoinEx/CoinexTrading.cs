using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexTrading : IFuturesTrading
    {

        public CoinexTrading(ICryptoFuturesExchange oExchange) 
        { 
            Exchange = oExchange;
        }
        public ICryptoFuturesExchange Exchange { get; }

        public async Task<IFuturesOrder?> CreateLimitOrder(IFuturesSymbol oSymbol, bool bBuy, bool bLong, decimal nQuantity, decimal nPrice)
        {
            throw new NotImplementedException();
        }

        public async Task<IFuturesOrder?> CreateMarketOrder(IFuturesSymbol oSymbol, bool bBuy, bool bLong, decimal nQuantity)
        {
            throw new NotImplementedException();
        }

        public async Task<IFuturesLeverage?> GetLeverage(IFuturesSymbol oSymbol)
        {
            throw new NotImplementedException();
        }

        public async Task<IFuturesLeverage[]?> GetLeverages(IFuturesSymbol[]? aSymbols = null)
        {
            throw new NotImplementedException();
        }

        public Task<IFuturesOrder[]?> GetOrders()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetLeverage(IFuturesSymbol oSymbol, int nLeverage)
        {
            throw new NotImplementedException();
        }
    }
}
