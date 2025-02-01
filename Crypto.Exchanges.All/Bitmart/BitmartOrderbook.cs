using BitMart.Net.Objects.Models;
using Crypto.Exchanges.All.Common;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bitmart
{
    internal class BitmartOrderbook : BaseOrderbook, IOrderbook
    {
        public BitmartOrderbook(IFuturesSymbol oSymbol, BitMartFuturesFullOrderBookUpdate oUpdate) :
            base(oSymbol, oUpdate.Timestamp.ToLocalTime(), DateTime.Now)
        {
            List<IOrderbookPrice> aAsks = new List<IOrderbookPrice>();
            foreach (var item in oUpdate.Asks.OrderBy(p => p.Price))
            {
                decimal nQuantity = item.Quantity * oSymbol.ContractSize;
                aAsks.Add(new BaseOrderbookPrice(this, item.Price, nQuantity));
            }
            Asks = aAsks.ToArray();

            List<IOrderbookPrice> aBids = new List<IOrderbookPrice>();
            foreach (var item in oUpdate.Bids.OrderByDescending(p => p.Price))
            {
                decimal nQuantity = item.Quantity * oSymbol.ContractSize;
                aBids.Add(new BaseOrderbookPrice(this, item.Price, nQuantity));
            }
            Bids = aBids.ToArray();

        }


    }
}
