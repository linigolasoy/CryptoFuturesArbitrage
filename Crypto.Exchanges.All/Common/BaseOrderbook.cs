using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Common
{

    internal class BaseOrderbookPrice : IOrderbookPrice
    {

        public decimal Price { get; internal set; } = 0;

        public decimal Volume { get; internal set; } = 0;
    }

    internal class BaseOrderbook
    {
        public BaseOrderbook( IFuturesSymbol oSymbol, DateTime dUpdateDate)
        {
            Symbol = oSymbol;
            UpdateDate = dUpdateDate;   
        }
        public DateTime UpdateDate { get; internal set; }

        public IFuturesSymbol Symbol { get; }

        public IOrderbookPrice[] Asks { get; internal set; } = Array.Empty<IOrderbookPrice>();

        public IOrderbookPrice[] Bids { get; internal set; } = Array.Empty<IOrderbookPrice>();

        public void Update(IOrderbook oNew)
        {
            this.UpdateDate = oNew.UpdateDate;
            this.Asks = oNew.Asks;
            this.Bids = oNew.Bids;
            return;
        }

    }
}
