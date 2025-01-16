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
        public BaseOrderbookPrice( IOrderbook oOrderbook, decimal nPrice, decimal nVolume ) 
        { 
            Orderbook = oOrderbook;
            Price = nPrice;
            Volume = nVolume;
        }
        public IOrderbook Orderbook { get; }  
        public decimal Price { get; } = 0;

        public decimal Volume { get; } = 0;
    }

    internal class BaseOrderbook
    {
        public BaseOrderbook( IFuturesSymbol oSymbol, DateTime dUpdateDate, DateTime dReceiveDate )
        {
            Symbol = oSymbol;
            UpdateDate = dUpdateDate;   
            ReceiveDate = dReceiveDate;
        }
        public DateTime UpdateDate { get; internal set; }
        public DateTime ReceiveDate { get; internal set; }

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
