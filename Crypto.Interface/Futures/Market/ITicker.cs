using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures.Market
{
    public interface IFuturesTicker
    {
        public IFuturesSymbol Symbol { get; }
        public decimal Price { get; }
        public DateTime DateTime { get; }   
    }
}
