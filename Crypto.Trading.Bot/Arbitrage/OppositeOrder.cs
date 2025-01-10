using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Trading.Bot.Arbitrage
{
    internal class OppositeOrder : IOppositeOrder
    {

        public OppositeOrder(IFuturesSymbol oSymbolLong, IFuturesSymbol oSymbolShort) 
        { 
            SymbolLong = oSymbolLong;
            SymbolShort = oSymbolShort;
        }
        public IFuturesSymbol SymbolLong { get; }

        public IFuturesSymbol SymbolShort { get; }

        public int Leverage { get; set; } = 1;

        public decimal Quantity { get; set; } = 0;

        public IFuturesOrder? OpenOrderLong => throw new NotImplementedException();

        public IFuturesOrder? OpenOrderShort => throw new NotImplementedException();

        public IFuturesOrder? CloseOrderLong => throw new NotImplementedException();

        public IFuturesOrder? CloseOrderShort => throw new NotImplementedException();

        public IFuturesPosition? PositionLong => throw new NotImplementedException();

        public IFuturesPosition? PositionShort => throw new NotImplementedException();

        public async Task<bool> TryCloseLimit()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> TryCloseMarket()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> TryOpenLimit()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> TryOpenMarket()
        {
            throw new NotImplementedException();
        }

        public static IOppositeOrder[]? CreateFromExchanges()
        {
            throw new NotImplementedException();
        }
    }
}
