using Crypto.Interface;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common
{
    internal class BaseTicker : IFuturesTicker
    {
        public BaseTicker( IFuturesSymbol oSymbol, Decimal nPrice, DateTime dDate, decimal nAsk, decimal nBid ) 
        { 
            Symbol = oSymbol;
            Price = nPrice;
            DateTime = dDate;   
            BestAsk = nAsk;
            BestBid = nBid;
        }
        public IFuturesSymbol Symbol { get; }

        public decimal Price { get; }

        public DateTime DateTime { get; }

        public decimal BestAsk { get; }
        public decimal BestBid { get; }

    }
}
