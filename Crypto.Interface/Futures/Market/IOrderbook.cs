using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures.Market
{


    /// <summary>
    /// Orderbook price (Asks, Bids)
    /// </summary>
    public interface IOrderbookPrice
    {
        public decimal Price { get; }
        public decimal Volume { get; }
    }

    /// <summary>
    /// Represents orderbook data
    /// </summary>
    public interface IOrderbook
    {
        public DateTime UpdateDate { get; }
        public IFuturesSymbol Symbol { get; }

        public IOrderbookPrice[] Asks { get; }
        public IOrderbookPrice[] Bids { get; }
    }
}
