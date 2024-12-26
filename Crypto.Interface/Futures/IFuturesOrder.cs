using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Interface.Futures
{

    public enum FuturesOrderDirection
    {
        Buy,
        Sell
    }

    public enum FuturesOrderType
    {
        Limit,
        Market
    }

    public enum FuturesPositionDirection
    {
        Long,
        Short
    }

    public enum FuturesOrderEvent
    {
        New,
        Canceled, // removed
        Calculated, // order ADL or liquidation
        Expired, // order lapsed
        Trade // transaction

    }

    public enum FuturesOrderStatus
    {
        New,
        PartialFilled,
        Filled,
        Canceled,
        Expired
    }

    public interface IFuturesOrder
    {
        public long Id { get; }
        public IFuturesSymbol Symbol { get; }   
        public FuturesOrderDirection OrderDirection { get; }
        public FuturesPositionDirection PositionDirection { get; }
        public FuturesOrderType OrderType { get; }

        public FuturesOrderEvent OrderEvent { get; }
        public FuturesOrderStatus OrderStatus { get; }  
        public DateTime TimeCreated { get; }
        public DateTime TimeUpdated { get; }
        public decimal Quantity { get; }
        public decimal? Price { get; }  
    }
}
