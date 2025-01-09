using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetLeverage : IFuturesLeverage
    {
        public BitgetLeverage(IFuturesSymbol oSymbol)
        {
            Symbol = oSymbol;   
        }
        public IFuturesSymbol Symbol { get; }

        public decimal LongLeverage { get; internal set; } = 1;

        public decimal ShortLeverage { get; internal set; } = 1;
    }
}
