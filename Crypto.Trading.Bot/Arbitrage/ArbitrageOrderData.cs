using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
using Crypto.Trading.Bot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Arbitrage
{
    internal class ArbitrageOrderData : IArbitrageOrderData
    {
        public ArbitrageOrderData( IFuturesSymbol Symbol ) 
        { 
            this.Symbol = Symbol;
        }

        public IFuturesSymbol Symbol { get; }

        public decimal Quantity { get; set; } = 0;

        public ITrailingOrder? TrailingOrder { get; set; }

        public IFuturesOrder? OpenOrder { get; set; } = null;

        public IFuturesOrder? CloseOrder { get; set; } = null;

        public IFuturesPosition? Position { get; set; } = null;

        public IOrderbook? Orderbook { get; set; } = null;

        public decimal Profit { get; internal set; } = 0;
    }
}
