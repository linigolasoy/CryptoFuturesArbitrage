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
        public IOrderbook Orderbook { get; }    
        public decimal Price { get; }
        public decimal Volume { get; }
    }

    /// <summary>
    /// Represents orderbook data
    /// </summary>
    public interface IOrderbook: IUpdateableObject<IOrderbook>
    {
        public DateTime ReceiveDate { get; }
        public DateTime UpdateDate { get; }
        public IFuturesSymbol Symbol { get; }

        public IOrderbookPrice[] Asks { get; }
        public IOrderbookPrice[] Bids { get; }

        public IOrderbookPrice? GetBestPrice(bool bAsk, decimal? nQuantity = null, decimal? nMoney = null);

    }
}
