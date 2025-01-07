using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures
{
    public interface IFuturesLeverage
    {
        public IFuturesSymbol Symbol { get; }
        public decimal LongLeverage { get; }
        public decimal ShortLeverage { get; }
    }
}
