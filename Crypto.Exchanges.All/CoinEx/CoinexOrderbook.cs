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

        public CoinexOrderbook( IFuturesSymbol oSymbol, DateTime dTimestamp, CoinExOrderBook oParsed) :
            base(oSymbol, dTimestamp.ToLocalTime(), DateTime.Now)
        { 
            List<IOrderbookPrice> aAsks = new List<IOrderbookPrice>();
            foreach (var item in oParsed.Data.Asks.OrderBy(p => p.Price))
            {
                aAsks.Add( new BaseOrderbookPrice(this, item.Price, item.Quantity));  
            }
            Asks = aAsks.ToArray();

            List<IOrderbookPrice> aBids = new List<IOrderbookPrice>();
            foreach (var item in oParsed.Data.Bids.OrderByDescending(p => p.Price))
            {
                aBids.Add(new BaseOrderbookPrice(this, item.Price, item.Quantity));
            }
            Bids = aBids.ToArray();

        }
    }
}
