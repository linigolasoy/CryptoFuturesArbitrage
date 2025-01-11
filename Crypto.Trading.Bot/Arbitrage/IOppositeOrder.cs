using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Arbitrage
{

    public interface ICloseResult
    {
        public bool Success { get; }    
        public decimal ProfitOrLoss { get; }    
    }
    public interface IOppositeOrder
    {
        public IFuturesSymbol SymbolLong { get; }
        public IFuturesSymbol SymbolShort { get; }

        public int Leverage { get; set; }

        public decimal Quantity { get; set; }

        public decimal Profit { get; set; } 
        public decimal ProfitBalance { get; set; }  
        public IFuturesOrder? OpenOrderLong { get; }
        public IFuturesOrder? OpenOrderShort { get; }

        public IFuturesOrder? CloseOrderLong { get; }
        public IFuturesOrder? CloseOrderShort { get; }

        public IFuturesPosition? PositionLong { get; }
        public IFuturesPosition? PositionShort { get; }

        public Task<bool> TryOpenLimit();
        public Task<bool> TryOpenMarket();
        public Task<ICloseResult> TryCloseLimit();
        public Task<ICloseResult> TryCloseMarket();
    }
}
