using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchange.Mexc
{
    public partial class MexcFuturesExchange
    {
        public async Task<IFuturesOrder?> CreateLimitOrder(IFuturesSymbol oSymbol, bool bBuy, decimal nMargin, int nLeverage, decimal nPrice)
        {
            throw new NotImplementedException();
        }
    }
}
