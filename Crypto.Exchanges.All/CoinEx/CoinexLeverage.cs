using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx
{
    internal class CoinexLeverage : IFuturesLeverage
    {
        public CoinexLeverage(IFuturesSymbol oSymbol) 
        { 
            Symbol = oSymbol;   
        }

        public IFuturesSymbol Symbol { get; }

        public decimal LongLeverage { get; internal set;  } = 1;

        public decimal ShortLeverage { get; internal set; } = 1;
    }
}
