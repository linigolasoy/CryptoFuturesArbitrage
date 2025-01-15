using CoinEx.Net.Objects.Models.V2;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.CoinEx
{

    internal class CoinexOrderbook: BaseOrderbook, IOrderbook
    {

        public CoinexOrderbook( IFuturesSymbol oSymbol, CoinExOrderBook oParsed) :
            base(oSymbol, oParsed.Data.UpdateTime.ToLocalTime())
        { 
            List<IOrderbookPrice> aAsks = new List<IOrderbookPrice>();
            foreach (var item in oParsed.Data.Asks.OrderBy(p => p.Price))
            {
                aAsks.Add( new BaseOrderbookPrice() { Price = item.Price, Volume = item.Quantity });  
            }
            Asks = aAsks.ToArray();

            List<IOrderbookPrice> aBids = new List<IOrderbookPrice>();
            foreach (var item in oParsed.Data.Bids.OrderByDescending(p => p.Price))
            {
                aBids.Add(new BaseOrderbookPrice() { Price = item.Price, Volume = item.Quantity });
            }
            Bids = aBids.ToArray();

        }
    }
}
