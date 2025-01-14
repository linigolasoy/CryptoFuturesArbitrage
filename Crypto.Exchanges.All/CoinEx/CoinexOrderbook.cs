using CoinEx.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx
{

    internal class CoinexOrderbookPrice : IOrderbookPrice
    {

        public decimal Price { get; internal set; } = 0;

        public decimal Volume { get; internal set; } = 0;
    }
    internal class CoinexOrderbook : IOrderbook
    {

        public CoinexOrderbook( IFuturesSymbol oSymbol, CoinExOrderBook oParsed) 
        { 
            Symbol = oSymbol;
            UpdateDate = oParsed.Data.UpdateTime.ToLocalTime(); 
            List<IOrderbookPrice> aAsks = new List<IOrderbookPrice>();
            foreach (var item in oParsed.Data.Asks.OrderBy(p => p.Price))
            {
                aAsks.Add( new CoinexOrderbookPrice() { Price = item.Price, Volume = item.Quantity });  
            }
            Asks = aAsks.ToArray();

            List<IOrderbookPrice> aBids = new List<IOrderbookPrice>();
            foreach (var item in oParsed.Data.Bids.OrderByDescending(p => p.Price))
            {
                aBids.Add(new CoinexOrderbookPrice() { Price = item.Price, Volume = item.Quantity });
            }
            Bids = aBids.ToArray();

        }
        public DateTime UpdateDate { get; }

        public IFuturesSymbol Symbol { get; }

        public IOrderbookPrice[] Asks { get; }

        public IOrderbookPrice[] Bids { get; }
    }
}
