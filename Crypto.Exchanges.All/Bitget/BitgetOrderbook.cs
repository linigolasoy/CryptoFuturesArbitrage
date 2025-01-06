using Bitget.Net.Objects.Models.V2;
using Crypto.Exchanges.All.Bingx;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitget
{

    internal class BitgetOrderbookPrice : IOrderbookPrice
    {

        public decimal Price { get; internal set; } = 0;

        public decimal Volume { get; internal set; } = 0;
    }

    internal class BitgetOrderbook : IOrderbook
    {

        public BitgetOrderbook(IFuturesSymbol oSymbol, BitgetOrderBookUpdate oUpdate) 
        { 
            Symbol = oSymbol;
            UpdateDate = oUpdate.Timestamp.ToLocalTime();

            List<IOrderbookPrice> aAsks = new List<IOrderbookPrice>();
            foreach (var item in oUpdate.Asks.OrderBy(p => p.Price))
            {
                aAsks.Add(new BingxOrderbookPrice() { Price = item.Price, Volume = item.Quantity });
            }
            Asks = aAsks.ToArray();

            List<IOrderbookPrice> aBids = new List<IOrderbookPrice>();
            foreach (var item in oUpdate.Bids.OrderByDescending(p => p.Price))
            {
                aBids.Add(new BingxOrderbookPrice() { Price = item.Price, Volume = item.Quantity });
            }
            Bids = aBids.ToArray();

        }
        public DateTime UpdateDate { get; }

        public IFuturesSymbol Symbol { get; }

        public IOrderbookPrice[] Asks { get; }

        public IOrderbookPrice[] Bids { get; }
    }
}
