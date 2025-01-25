using Bitget.Net.Objects.Models.V2;
using Crypto.Interface.Futures;
using Bitget.Net.Enums.V2;
using BitgetOrderType = Bitget.Net.Enums.V2.OrderType;
using BitgetOrderStatus = Bitget.Net.Enums.V2.OrderStatus;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Interface.Futures.Trading;
using Crypto.Interface.Futures.Market;
using Crypto.Interface.Futures.Websockets;

namespace Crypto.Exchanges.All.Bitget
{
    internal class BitgetOrder : IFuturesOrder
    {

        public BitgetOrder( IFuturesSymbol oSymbol, BitgetOrderId oId, bool bBuy, bool bLong, FuturesOrderType eType, decimal nQuantity, decimal? nPrice) 
        {
            Symbol = oSymbol;
            Id = oId.OrderId;
            PositionDirection = (bLong ? FuturesPositionDirection.Long : FuturesPositionDirection.Short);
            OrderDirection = (bBuy? FuturesOrderDirection.Buy : FuturesOrderDirection.Sell);
            OrderType = eType;
            Quantity = nQuantity;
            Price = nPrice; 
        }

        public BitgetOrder( IFuturesSymbol oSymbol, BitgetFuturesOrderUpdate oUpdate )
        {
            Symbol =oSymbol;
            Id = oUpdate.OrderId;
            PositionDirection = (oUpdate.PositionSide == PositionSide.Long ? FuturesPositionDirection.Long : FuturesPositionDirection.Short);
            OrderDirection = (oUpdate.Side == OrderSide.Buy ? FuturesOrderDirection.Buy : FuturesOrderDirection.Sell);
            OrderType = (oUpdate.OrderType == BitgetOrderType.Limit ? FuturesOrderType.Limit : FuturesOrderType.Market);
            PutStatus(oUpdate.Status);
            Quantity = oUpdate.Quantity;    
            Price = oUpdate.Price;  
        }
        public BitgetOrder(IFuturesSymbol oSymbol, BitgetFuturesOrder oUpdate)
        {
            Symbol = oSymbol;
            Id = oUpdate.OrderId;
            PositionDirection = (oUpdate.PositionSide == PositionSide.Long ? FuturesPositionDirection.Long : FuturesPositionDirection.Short);
            OrderDirection = (oUpdate.Side == OrderSide.Buy ? FuturesOrderDirection.Buy : FuturesOrderDirection.Sell);
            OrderType = (oUpdate.OrderType == BitgetOrderType.Limit ? FuturesOrderType.Limit : FuturesOrderType.Market);
            PutStatus(oUpdate.Status);
            Quantity = oUpdate.Quantity;
            Price = oUpdate.Price;
        }

        private void PutStatus(BitgetOrderStatus eStatus )
        {
            switch ( eStatus )
            {
                case BitgetOrderStatus.Initial:
                case BitgetOrderStatus.Live:
                case BitgetOrderStatus.New:
                    OrderStatus = FuturesOrderStatus.New; break;
                case BitgetOrderStatus.Filled:
                    OrderStatus = FuturesOrderStatus.Filled; break;
                case BitgetOrderStatus.PartiallyFilled:
                    OrderStatus = FuturesOrderStatus.PartialFilled; break;
                case BitgetOrderStatus.Canceled:
                case BitgetOrderStatus.Rejected:
                    OrderStatus = FuturesOrderStatus.Canceled; break;  
            }
        }

        public WebsocketQueueType QueueType { get => WebsocketQueueType.Order; }
        public string Id { get; }

        public IFuturesSymbol Symbol { get; }

        public FuturesOrderDirection OrderDirection { get; }

        public FuturesPositionDirection PositionDirection { get; }

        public FuturesOrderType OrderType { get; }

        public FuturesOrderEvent OrderEvent { get; } = FuturesOrderEvent.New;

        public FuturesOrderStatus OrderStatus { get; private set; } = FuturesOrderStatus.New;

        public DateTime TimeCreated { get; } = DateTime.Now;

        public DateTime TimeUpdated { get; private set; } = DateTime.Now;

        public decimal Quantity { get; }

        public decimal? Price { get; } = null;

        public void Update(IFuturesOrder oOrder)
        {
            OrderStatus = oOrder.OrderStatus;
            TimeUpdated = oOrder.TimeUpdated;
        }
    }
}
