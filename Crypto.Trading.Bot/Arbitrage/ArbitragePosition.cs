using Crypto.Interface.Futures.Account;
using Crypto.Interface.Futures.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Arbitrage
{
    internal class ArbitragePosition : IArbitragePosition
    {
        public ArbitragePosition(IArbitrageChance oChance, IOrderbook oOrderbook) 
        { 
            Chance = oChance;
            Orderbook = oOrderbook;
            Symbol = oOrderbook.Symbol;
        }
        public IArbitrageChance Chance { get; }

        public IOrderbook Orderbook { get; }

        public IFuturesSymbol Symbol { get; }
        public IFuturesPosition? Position { get; set; } = null;
    }
}
