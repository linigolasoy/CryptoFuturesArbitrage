using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Spread
{

    internal enum SpreadStatus
    {
        Init,
        CreateOpen,
        WaitForOpen,
        WaitForPosition,
        CreateClose,
        WaitForClose,
        Closed
    }

    internal class SpreadChance
    {
        public SpreadChance( IOrderbook oOrderbook ) 
        {
            Orderbook = oOrderbook;
            Symbol = oOrderbook.Symbol;

            Refresh();

            if( PriceAsk > 0 && PriceBid > 0 )
            {
                PercentSpread = 100.0M * (PriceAsk - PriceBid) / PriceBid;
            }
        }

        public IOrderbook Orderbook { get; }
        public IFuturesSymbol Symbol { get; }   

        public SpreadStatus Status { get; set; } = SpreadStatus.Init;
        public decimal PercentSpread { get; } = 0;  

        public decimal PriceAsk { get; private set; } = 0;
        public decimal PriceBid { get; private set; } = 0;

        public decimal ChanceQuantity { get; internal set; } = 0;

        public IFuturesOrder? BuyOrder { get; internal set; } = null;

        public string? BuyOrderId { get; internal set; } = null;
        public IFuturesOrder? SellOrder { get; internal set; } = null;

        public string? SellOrderId { get; internal set; } = null;
        public IFuturesPosition? Position { get; internal set; } = null;
        public void Refresh()
        {
            PriceAsk = 0;
            if(Orderbook.Asks.Length > 0 )
            {
                PriceAsk = Orderbook.Asks.Select(p=> p.Price).Min();
            }
            PriceBid = 0;
            if (Orderbook.Bids.Length > 0)
            {
                PriceBid = Orderbook.Bids.Select(p => p.Price).Max();
            }

        }
    }
}
