using Crypto.Interface.Futures;
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


    public interface IArbitrageOrderData
    {
        public IFuturesSymbol Symbol { get; }   
        public decimal Quantity { get; set; }

        public IFuturesOrder? OpenOrder { get; set; }
        public IFuturesOrder? CloseOrder { get; set; }

        public IFuturesPosition? Position { get; set; }

        public IOrderbook? Orderbook { get; set; }   

        public decimal Profit { get; }  
    }


    public interface ICloseResult
    {
        public bool Success { get; }    
        public decimal ProfitOrLoss { get; }    
    }
    public interface IOppositeOrder
    {
        public IArbitrageOrderData LongData { get; }    
        public IArbitrageOrderData ShortData { get; }   

        public decimal Profit { get; }
        public int ProfitUpdates { get; }
        public decimal Fees { get; }
        public int Leverage { get; }
        /*
        public IFuturesSymbol SymbolLong { get; }
        public IFuturesSymbol SymbolShort { get; }

        public int Leverage { get; set; }

        public decimal Quantity { get; }

        public decimal Profit { get; } 
        public decimal ProfitBalance { get; }  
        public decimal Fees { get; }   
        public IFuturesOrder? OpenOrderLong { get; }
        public IFuturesOrder? OpenOrderShort { get; }

        public IFuturesOrder? CloseOrderLong { get; }
        public IFuturesOrder? CloseOrderShort { get; }

        public IFuturesPosition? PositionLong { get; }
        public IFuturesPosition? PositionShort { get; }
        */

        public void Update();

        public Task<bool> SetLeverages();
        public Task<bool> TryOpenLimit(decimal nMoney);
        public Task<bool> TryOpenMarket(decimal nMoney);
        public Task<ICloseResult> TryCloseLimit();
        public Task<ICloseResult> TryCloseMarket();
    }
}
