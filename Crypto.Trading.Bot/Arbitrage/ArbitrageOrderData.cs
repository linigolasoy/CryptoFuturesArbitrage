using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Trading;
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

        public decimal Quantity { get; internal set; } = 0;

        public IFuturesOrder? OpenOrder { get; internal set; } = null;

        public IFuturesOrder? CloseOrder { get; internal set; } = null;

        public IFuturesPosition? Position { get; internal set; } = null;

        public IOrderbook? Orderbook { get; internal set; } = null;

        public decimal Profit { get; internal set; } = 0;
    }
}
