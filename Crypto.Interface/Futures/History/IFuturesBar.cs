using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures.History
{
    public interface IFuturesBar : IBarData
    {
        public IFuturesSymbol Symbol { get; }
    }
}
