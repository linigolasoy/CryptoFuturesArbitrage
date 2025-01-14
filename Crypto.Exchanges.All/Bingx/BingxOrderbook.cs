using BingX.Net.Objects.Models;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx
{

    internal class BingxOrderbookPrice : IOrderbookPrice
    {

        public decimal Price { get; internal set; } = 0;

        public decimal Volume { get; internal set; } = 0;
    }
    internal class BingxOrderbook : IOrderbook
    {
        public BingxOrderbook( IFuturesSymbol oSymbol, DateTime dDate, BingXOrderBook oBook )
        {
            Symbol = oSymbol;   
            UpdateDate = dDate; 

            List<IOrderbookPrice> aAsks = new List<IOrderbookPrice>();
            foreach (var item in oBook.Asks.OrderBy(p => p.Price))
            {
                aAsks.Add(new BingxOrderbookPrice() { Price = item.Price, Volume = item.Quantity });
            }
            Asks = aAsks.ToArray();

            List<IOrderbookPrice> aBids = new List<IOrderbookPrice>();
            foreach (var item in oBook.Bids.OrderByDescending(p => p.Price))
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
