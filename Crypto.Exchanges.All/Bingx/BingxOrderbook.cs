using BingX.Net.Objects.Models;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx
{

    internal class BingxOrderbook : BaseOrderbook, IOrderbook
    {
        public BingxOrderbook( IFuturesSymbol oSymbol, DateTime dDate, BingXOrderBook oBook ):
            base(oSymbol, dDate)
        {

            List<IOrderbookPrice> aAsks = new List<IOrderbookPrice>();
            foreach (var item in oBook.Asks.OrderBy(p => p.Price))
            {
                aAsks.Add(new BaseOrderbookPrice() { Price = item.Price, Volume = item.Quantity });
            }
            Asks = aAsks.ToArray();

            List<IOrderbookPrice> aBids = new List<IOrderbookPrice>();
            foreach (var item in oBook.Bids.OrderByDescending(p => p.Price))
            {
                aBids.Add(new BaseOrderbookPrice() { Price = item.Price, Volume = item.Quantity });
            }
            Bids = aBids.ToArray();
        }
    }
}
