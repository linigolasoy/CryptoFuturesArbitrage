using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxTrading : IFuturesTrading
    {

        public BingxTrading(ICryptoFuturesExchange oExchange) 
        { 
            Exchange = oExchange;   
        }

        public ICryptoFuturesExchange Exchange { get; }

        public async Task<IFuturesOrder?> CreateLimitOrder(IFuturesSymbol oSymbol, bool bBuy, decimal nQuantity, decimal nPrice)
        {
            throw new NotImplementedException();
        }

        public async Task<IFuturesOrder?> CreateMarketOrder(IFuturesSymbol oSymbol, bool bBuy, decimal nQuantity, decimal nPrice)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetLeverage(IFuturesSymbol oSymbol, int nLeverage)
        {
            throw new NotImplementedException();
        }
    }
}
