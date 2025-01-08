using BingX.Net.Objects.Models;
using Crypto.Interface.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Exchanges.All.Bingx
{
    internal class BingxOrder : IFuturesOrder
    {

        private void PutTypes(
                BingX.Net.Enums.OrderSide eSide,
                BingX.Net.Enums.PositionSide ePositionSide,
                BingX.Net.Enums.FuturesOrderType eOrderType
            )
        {
            switch (eSide)
            {
                case BingX.Net.Enums.OrderSide.Buy:
                    OrderDirection = FuturesOrderDirection.Buy;
                    break;
                case BingX.Net.Enums.OrderSide.Sell:
                    OrderDirection = FuturesOrderDirection.Sell;
                    break;
            }
            switch (ePositionSide)
            {
                case BingX.Net.Enums.PositionSide.Long:
                    PositionDirection = FuturesPositionDirection.Long;
                    break;
                case BingX.Net.Enums.PositionSide.Short:
                    PositionDirection = FuturesPositionDirection.Short;
                    break;
            }

            switch (eOrderType)
            {
                case BingX.Net.Enums.FuturesOrderType.Market:
                    OrderType = FuturesOrderType.Market; break;
                case BingX.Net.Enums.FuturesOrderType.Limit:
                    OrderType = FuturesOrderType.Limit; break;
                default:
                    break;
            }


        }


        private void PutStatus(BingX.Net.Enums.OrderStatus eStatus)
        {
            switch (eStatus)
            {
                case BingX.Net.Enums.OrderStatus.New:
                    OrderStatus = FuturesOrderStatus.New;
                    break;
                case BingX.Net.Enums.OrderStatus.Pending:
                    OrderStatus = FuturesOrderStatus.New;
                    break;
                case BingX.Net.Enums.OrderStatus.PartiallyFilled:
                    OrderStatus = FuturesOrderStatus.PartialFilled;
                    break;
                case BingX.Net.Enums.OrderStatus.Filled:
                    OrderStatus = FuturesOrderStatus.Filled;    
                    break;
                case BingX.Net.Enums.OrderStatus.Canceled:
                    OrderStatus = FuturesOrderStatus.Canceled;  
                    break;
                case BingX.Net.Enums.OrderStatus.Failed:
                    OrderStatus = FuturesOrderStatus.Canceled;
                    break;

            }

        }

        public BingxOrder(IFuturesSymbol oSymbol, BingXFuturesOrderDetails oParsed) 
        {
            Symbol = oSymbol;
            Id = oParsed.OrderId;
            PutTypes(oParsed.Side, oParsed.PositionSide!.Value, oParsed.Type);
            if (oParsed.Quantity != null) Quantity = oParsed.Quantity.Value;
            if (oParsed.Price != null) Price = oParsed.Price.Value;
            PutStatus(oParsed.Status);  

        }
        public BingxOrder(IFuturesSymbol oSymbol, BingXFuturesOrder oParsed) 
        { 
            Symbol = oSymbol;
            Id = oParsed.OrderId;

            PutTypes(oParsed.Side, oParsed.PositionSide!.Value, oParsed.Type);

            if (oParsed.Quantity != null) Quantity = oParsed.Quantity.Value;
            if( oParsed.Price != null ) Price = oParsed.Price.Value;    
        }

        public BingxOrder(IFuturesSymbol oSymbol, BingXFuturesOrderUpdate oParsed)
        {
            Symbol = oSymbol;
            Id = oParsed.OrderId;
            PutTypes(oParsed.Side, oParsed.PositionSide, oParsed.Type);

            if (oParsed.Quantity != null) Quantity = oParsed.Quantity.Value;
            if (oParsed.Price != null) Price = oParsed.Price.Value;
            PutStatus(oParsed.Status);
        }
        public long Id { get; }

        public IFuturesSymbol Symbol { get; }

        public FuturesOrderDirection OrderDirection { get; private set; } = FuturesOrderDirection.Buy;

        public FuturesPositionDirection PositionDirection { get; private set; } = FuturesPositionDirection.Long;

        public FuturesOrderType OrderType { get; private set; } = FuturesOrderType.Market;

        public FuturesOrderEvent OrderEvent { get; private set; } = FuturesOrderEvent.New;

        public FuturesOrderStatus OrderStatus { get; private set; } = FuturesOrderStatus.New;

        public DateTime TimeCreated { get; } = DateTime.Now;

        public DateTime TimeUpdated { get; private set; } = DateTime.Now;

        public decimal Quantity { get; private set; } = 0;

        public decimal? Price { get; private set; } = 0;

        public void Update( IFuturesOrder oOrder )
        {
            OrderStatus = oOrder.OrderStatus;
            TimeUpdated = oOrder.TimeUpdated;
            OrderEvent = oOrder.OrderEvent; 
        }
    }
}
